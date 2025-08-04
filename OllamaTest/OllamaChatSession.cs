using Backend.Extensions;
using Backend.Messages;
using Backend.Persistance;
using LiteNetLib;
using LiteNetLib.Utils;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System;
using System.Data;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Backend;

partial class OllamaChatSession
{
    OllamaApiClient? _ollama;
    Chat? _chat;
    IEnumerable<Tool>? _activeTools;
    NPCCharacterInfo? _activeCharacter;
    string? _embeddingModel;
    public async Task InitOllama(string activeModel, string? embeddingModel = null)
    {
        try
        {
            _ollama = await ConnectAsync();
        }
        catch (Exception)
        {
            var tempCol = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR] intializing Ollama. Ensure the Ollama background service is running!" + Environment.NewLine);
            Console.ForegroundColor = tempCol;
            throw;
        }
        await PullModel(_ollama, activeModel);
        if (embeddingModel != null)
        {
            await PullModel(_ollama, embeddingModel);
        }
        _embeddingModel = embeddingModel;
        _ollama.SelectedModel = activeModel;

        Console.WriteLine("System initialized!");
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
            Console.WriteLine("NO TOOLS ACTIVE");
        }
        else
        {
            Console.WriteLine("TOOLS: " + string.Join(", ", _activeTools.Select(t => t.Function?.Name)));
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
            Console.WriteLine($"Error Saving savefile. {Environment.NewLine} {e}");
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
                Console.WriteLine("Error Loading savefile. Abort.");
                return;
            }
            MessageHistory = saveFile.MessageHistory;
            _documents = saveFile.Documents;
            TryLoadContext();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Loading savefile. {e}");
            return;
        }
    }


    public async Task ChatAsync(string message)
    {
        try
        {
            if (_activeCharacter == null || _chat == null) throw new InvalidOperationException("A character must be loaded before you may chat.");
            var prompt = await GetFinalPromptAsync(_activeCharacter.Name, message);
            Console.WriteLine($"Final Prompt: {Environment.NewLine} {prompt}");
            //Send prompt
            string response = "";
            await foreach (var answerToken in _chat.SendAsync(prompt, _activeTools))
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
            Console.WriteLine(response);

            //send response
            if (_unityPeer != null && !string.IsNullOrWhiteSpace(response))
            {
                var token = new AnswerTokenInfo(!_activeTools?.Any() ?? true, _activeCharacter?.Name ?? "", response);
                _netPacketProcessor.WriteNetSerializable(_writer, ref token);
                _unityPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
                _writer.Reset();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in chatAsync {Environment.NewLine} {e}");
        }
    }

    private static async Task PullModel(OllamaApiClient ollama, string selectedModel)
    {
        var models = await ollama.ListLocalModelsAsync();
        if (!models.Any(x => x.Name == selectedModel))
        {
            Console.WriteLine($"Model: \"{selectedModel}\" not found. Downloading...");
            bool completed = false;
            int prevVal = 0;
            await foreach (var response in ollama.PullModelAsync(selectedModel))
            {
                if (response == null) { continue; }
                if (prevVal != (int)(response.Percent * 1000))
                {
                    Console.WriteLine($"Downloaded {response.Percent:F1}%");
                }
                prevVal = (int)(response.Percent * 1000);
                completed = response.Total == response.Completed;
            }
            while (!completed) ;
            Console.WriteLine($"Model \"{selectedModel}\" successfully downloaded.");
        }
    }

    private static async Task<OllamaApiClient> ConnectAsync()
    {
        OllamaApiClient? ollama = null;
        var connected = false;
        do
        {
            var url = "http://127.0.0.1:11434/";
            var uri = new Uri(url);
            Console.WriteLine($"Connecting to {uri} ...");
            try
            {
                ollama = new OllamaApiClient(url);
                connected = await ollama.IsRunningAsync();
                Console.WriteLine($"Connected status: {connected}");
            }
            catch (Exception ex)
            {
                ollama?.Dispose();

                Console.WriteLine(ex.ToString());
                Console.WriteLine();
            }
        } while (!connected);

        if (ollama == null)
        {
            throw new InvalidOperationException();
        }
        return ollama;
    }
}
