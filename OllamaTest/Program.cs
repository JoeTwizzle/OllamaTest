using Backend.Messages;
namespace Backend;

internal class Program
{
    static bool RunStandalone = false;
    static int Port = 9050;
    static readonly string ActiveModel = "qwen3:latest";
    static readonly string EmbeddingModel = "nomic-embed-text";
    static readonly string ConnectionKey = "eotwConnectionKey";
    static async Task Main(string[] args)
    {
        // If the application is started with an argument check if it is a valid port and switch to networked mode.
        if (args.Length == 1 && int.TryParse(args[0], out Port))
        {
            RunStandalone = false;
        }

        OllamaChatSession session = new();
        await session.InitOllama(ActiveModel, EmbeddingModel);

        //Local chat only. No server communication.
        if (RunStandalone)
        {
            await RunLocal(session);
        }
        else
        {
            session.RunServer(Port, ConnectionKey);
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

        var info = new NPCCharacterInfo("Bob", systemPrompt, ["GetCurrentWeatherTool", "GetCurrentNewsTool"], []);

        session.LoadCharacter(info, false);
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            var message = Console.ReadLine() ?? "";
            await session.ChatAsync(message);
            Console.WriteLine();
        }
    }
}
