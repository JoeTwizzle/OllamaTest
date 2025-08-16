using Backend.Extensions;
using Backend.Messages;
using Backend.Persistance;
using LiteNetLib;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Backend;

partial class OllamaChatSession
{
    OllamaApiClient? _ollama;
    Chat? _chat;
    IEnumerable<Tool>? _activeTools;
    NPCCharacterInfo? _activeCharacter;
    string? _embeddingModel;
    public async Task InitOllama(string url, string activeModel, string? embeddingModel = null)
    {
        try
        {
            _ollama = await ConnectAsync(url);
        }
        catch (Exception)
        {
            LogError("[ERROR] Intializing Ollama. Ensure the Ollama background service is running!" + Environment.NewLine);
            throw;
        }
        try
        {
            await PullModel(_ollama, activeModel);
            if (embeddingModel != null)
            {
                await PullModel(_ollama, embeddingModel);
            }
        }
        catch (Exception)
        {
            LogError("[ERROR] Could not find model at given URL");
            throw;
        }

        _embeddingModel = embeddingModel;
        _ollama.SelectedModel = activeModel;

        Log("System initialized!", ConsoleColor.Green);
        Console.WriteLine();
    }

    public void LoadCharacter(NPCCharacterInfo characterInfo, bool forceReload)
    {
        //TODO: structured character info handling
        if (_ollama == null) throw new InvalidOperationException("Ollama must be initialized before loading a character.");
        _chat = new Chat(_ollama, characterInfo.Prompt)
        {
            Options = new OllamaSharp.Models.RequestOptions()
            {
                PresencePenalty = 0.5f,
                TopP = 0.8f,
                TopK = 20,
                MinP = 0,
                Temperature = 0.7f,
            }
        };

        _activeTools = SelectTools(characterInfo.AvailableTools);
        if (!_activeTools.Any())
        {
            LogEvent("NO TOOLS ACTIVE");
        }
        else
        {
            LogEvent("TOOLS: " + string.Join(", ", _activeTools.Select(t => t.Function?.Name)));
        }
        SaveContext();
        _activeCharacter = characterInfo;

        if (forceReload)
        {
            ClearContext();
            AddWarmupDialogue();
        }
        else
        {
            if (!TryLoadContext())
            {
                AddWarmupDialogue();
            }
        }
    }

    void AddWarmupDialogue()
    {
        if (_chat == null || _activeCharacter == null)
        {
            return;
        }
        if (_chat.Model.Contains("qwen3"))
        {
            _chat.Think = false;
        }
        _chat.Messages.Add(new Message(ChatRole.System, _activeCharacter.Prompt));
        foreach ((var i, var message) in _activeCharacter.WarmUpDialogue.Index())
        {
            ChatRole role = (i & 1) == 0 ? ChatRole.User : ChatRole.Assistant;
            _chat.Messages.Add(new Message(role, message));
        }
    }

    Dictionary<string, List<Message>> MessageHistory = new();
    private void SaveContext()
    {
        if (_chat == null || _activeCharacter == null)
        {
            return;
        }

        if (!MessageHistory.TryGetValue(_activeCharacter.Name, out var messages))
        {
            messages = new();
            MessageHistory.Add(_activeCharacter.Name, messages);
        }

        messages.AddRange(_chat.Messages);
    }

    private bool TryLoadContext()
    {

        if (_chat == null || _activeCharacter == null)
        {
            return false;
        }

        if (MessageHistory.TryGetValue(_activeCharacter.Name, out var messages))
        {
            _chat.Messages = messages;
            return true;
        }

        return false;
    }

    private void ClearContext()
    {
        if (_chat == null || _activeCharacter == null)
        {
            return;
        }

        _chat.Messages.Clear();
        if (MessageHistory.TryGetValue(_activeCharacter.Name, out var messages))
        {
            messages.Clear();
        }

    }

    public void Save(string path)
    {
        try
        {
            SaveContext();
            var dir = Path.GetDirectoryName(path);
            if (dir != null)
            {
                Directory.CreateDirectory(dir);
            }

            using var stream = File.Create(path);

            var saveFile = new SaveFile() { MessageHistory = MessageHistory, Documents = _documents };

            JsonSerializer.Serialize(stream, saveFile, SourceGenContext.Default.SaveFile);
        }
        catch (Exception e)
        {
            LogError($"Error Saving savefile. {Environment.NewLine} {e}");
            return;
        }
    }

    public void Load(string location)
    {
        try
        {
            using var stream = File.OpenRead(location);
            SaveFile? saveFile = JsonSerializer.Deserialize(stream, SourceGenContext.Default.SaveFile);
            if (saveFile == null)
            {
                LogError("Error Loading savefile. Abort.");
                return;
            }
            MessageHistory = saveFile.MessageHistory;
            _documents = saveFile.Documents;
            TryLoadContext();
        }
        catch (Exception e)
        {
            LogError($"Error Loading savefile. {e}");
            return;
        }
    }
    const string InstructorName = "Instructor";
    const string InstructorPrompt = """
        You are an AI tasked with evaluating messages for relevancy and calling functions based on the content of said messages.
        Here are the tasks you MUST fulfill:
        1. Analyze the conversation for one of the topics listed in:
        GetQuestsForPlayer,
        GetCurrentPlayerActiveQuest,
        GetCompletableJobs,

        2. Call one of the following functions when appropriate
        StartPlayerQuest
        MarkJobAsComplete,

        It is of utmost importance that you preform this job with urgency and care.
        Do not talk. Simply concentrate on your task. Only ever call a function ONCE!
        """;
    public async Task ChatAsync(string message)
    {
        try
        {
            if (_activeCharacter == null || _chat == null) throw new InvalidOperationException("A character must be loaded before you may chat.");
            string response = await GetAIMessageAsync(message, Enumerable.Empty<object>());
            Console.WriteLine();

            response = StripNonLatin().Replace(response, "");
            Log($"{_activeCharacter.Name}: {response}", ConsoleColor.Gray);
            var tempCharacter = _activeCharacter;
            Console.WriteLine();
            LoadCharacter(new NPCCharacterInfo(InstructorName, InstructorPrompt, tempCharacter.AvailableTools, []), true);
            string prompt = $"""
            Evaluate the following set of messages:
            User:
            {message}

            {tempCharacter.Name}:
            {response}
            """;
            Console.WriteLine();
            //Hack to make quests related to this character visible for the instructor
            Instance!._activeCharacter!.Name = tempCharacter.Name;
            string instructorResponse = await GetAIMessageAsync(prompt, _activeTools ?? []);
            Log($"Instructor: {instructorResponse}", ConsoleColor.Yellow, LogLevel.Information);

            LoadCharacter(tempCharacter, false);
            //send response
            if (_unityPeer != null && !string.IsNullOrWhiteSpace(response))
            {
                var token = new AnswerTokenInfo(false, _activeCharacter?.Name ?? "", response);
                _netPacketProcessor.WriteNetSerializable(_writer, ref token);
                _unityPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
                _writer.Reset();
            }
        }
        catch (Exception e)
        {
            LogError($"Error in chatAsync! {Environment.NewLine} {e}");
        }
    }

    private async Task<string> GetAIMessageAsync(string message, IEnumerable<object> tools)
    {
        if (_activeCharacter == null || _chat == null) throw new InvalidOperationException("A character must be loaded before you may chat.");
        var prompt = await GetFinalPromptAsync(_activeCharacter.Name, message);
        Log($"Final Prompt: {Environment.NewLine} {prompt}", ConsoleColor.DarkGray);
        //Send prompt
        string response = "";
        await foreach (var answerToken in _chat.SendAsync(prompt, tools))
        {
            response += answerToken;
        }
        int start = response.IndexOf("<think>");
        int end = response.IndexOf("</think>") + "</think>".Length;
        if (start != -1 && end != -1)
        {
            response = response.Remove(start, end - start);
        }
        response = response.Trim('\n', '\r', ' ');
        return response;
    }

    private static async Task PullModel(OllamaApiClient ollama, string selectedModel)
    {
        var models = await ollama.ListLocalModelsAsync();
        if (!models.Any(x => x.Name == selectedModel))
        {
            Log($"Model: \"{selectedModel}\" not found. Downloading...", ConsoleColor.Gray);
            bool completed = false;
            int prevVal = 0;
            await foreach (var response in ollama.PullModelAsync(selectedModel))
            {
                if (response == null) { continue; }
                if (prevVal != (int)(response.Percent * 100))
                {
                    LogInfo($"Downloaded {response.Percent:F2}%");
                }
                prevVal = (int)(response.Percent * 100);
                completed = response.Total == response.Completed;
            }
            while (!completed) ;
            Log($"Model \"{selectedModel}\" successfully downloaded.", ConsoleColor.Green);
        }
    }

    private static async Task<OllamaApiClient> ConnectAsync(string url)
    {
        OllamaApiClient? ollama = null;
        var connected = false;
        do
        {
            var uri = new Uri(url);
            LogInfo($"Connecting to Ollama at: {uri} ...");
            try
            {
                ollama = new OllamaApiClient(url);
                connected = await ollama.IsRunningAsync();
                LogEvent($"Connected status: {connected}");
            }
            catch (Exception ex)
            {
                ollama?.Dispose();

                string errorMessage = $"""
                    Connection failed! Please make sure Ollama is running at the url specified!
                    Error log written to: 
                    {Path.Combine(Environment.CurrentDirectory, "ErrorLog.txt")}
                    """;

                LogError(errorMessage);
                File.WriteAllText("ErrorLog.txt", ex.ToString());
                Console.WriteLine();
                LogInfo("Retrying connection...");
            }
        } while (!connected);

        if (ollama == null)
        {
            throw new InvalidOperationException();
        }
        return ollama;
    }

    [GeneratedRegex(@"[^\p{IsBasicLatin}\p{IsLatin-1Supplement}\p{IsLatinExtended-A}\p{IsLatinExtended-B}\p{P}\p{Zs}]")]
    private static partial Regex StripNonLatin();
}
