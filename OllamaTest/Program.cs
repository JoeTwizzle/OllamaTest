global using static Backend.Utils;
using Backend.Messages;
namespace Backend;

internal class Program
{
    const int Port = 9050;
    const string ActiveModel = "hf.co/unsloth/Qwen3-4B-Instruct-2507-GGUF:Q4_K_M";
    const string EmbeddingModel = "dengcao/Qwen3-Embedding-0.6B:Q8_0";
    const string ConnectionKey = "eotwConnectionKey";
    const string Url = "http://127.0.0.1:11434/";
    const string HelpMessage = "You may specify the game port via the argument 1 and the Ollama url via argument 2. For local mode specify \"-l\"";

    static async Task Main(string[] args)
    {
        bool runStandAlone = false;
        string url = Url;
        int port = Port;
        if (args.Length == 0)
        {
            Log(HelpMessage, ConsoleColor.White, LogLevel.Critical);
        }
        // If the application is started with an argument check if it is a valid port and switch to networked mode.
        else if (args.Length == 1 && int.TryParse(args[0], out port))
        {
            runStandAlone = false;
        }
        else if (args.Length == 1 && args[0].Trim() == "-l")
        {
            runStandAlone = true;
        }
        else if (args.Length == 2 && int.TryParse(args[0], out port))
        {
            runStandAlone = false;
            url = args[1];
        }
        else
        {
            LogError("Incorrect arguments specified! Cannot start.");
            Log(HelpMessage, ConsoleColor.White);
            return;
        }
        OllamaChatSession session = new();
        await session.InitOllama(url, ActiveModel, EmbeddingModel);

        //Local chat only. No server communication.
        if (runStandAlone)
        {
            await RunLocal(session);
        }
        else
        {
            session.RunServer(port, ConnectionKey);
        }
    }

    static async Task RunLocal(OllamaChatSession session)
    {

        string systemPrompt = $@"""Transcript of a story, where the main character interacts with a Bee named Bob.
            Bob is stubborn, stingy, hard working, and never fails to protect the hive at any cost.
            Only behave as a regular bee from a storybook would. Bob only speaks in short scentences and inserts random bee noises at times.
            You have the ability to call specific tools in order gather more knowledge about the world, only do so when absolutly required and if you know a tool exists.
            If anyone talks to you in a non conversational manner, DO NOT respond to them and stay in character.
            """;

        var info = new NPCCharacterInfo("Bob", systemPrompt, ["GetCurrentWeatherTool", "GetCurrentNewsTool"], [], []);

        await session.LoadCharacter(info, false);
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            var message = Console.ReadLine() ?? "";
            await session.ChatAsync(message);
            Console.WriteLine();
        }
    }
}
