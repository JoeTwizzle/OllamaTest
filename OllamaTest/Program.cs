global using static Backend.Utils;
using System.Net;
namespace Backend;

internal class Program
{
    //TODO: INVESTIGATE NPC MEMORY
    //TODO: ADD BETTER LOGGING
    const int Port = 9050;
    const string ActiveModel = "hf.co/unsloth/Qwen3-4B-Instruct-2507-GGUF:Q4_K_M";
    const string EmbeddingModel = "qwen3-embedding:0.6b";
    const string ConnectionKey = "eotwConnectionKey";
    const string Url = "http://127.0.0.1:11434/";
    const string HelpMessage = "You may specify the game port via the argument 1 and the Ollama url via argument 2. For local mode specify \"-l\"";

    static async Task Main(string[] args)
    {
        string url = Url;
        int port = Port;
        IPAddress listenIpv4Addr = IPAddress.Loopback;
        IPAddress listenIpv6Addr = IPAddress.IPv6Loopback;

        if (args.Length == 0)
        {
            Log(HelpMessage, ConsoleColor.White, LogLevel.Critical);
        }
        else if (args.Length == 1 && int.TryParse(args[0], out port))
        {

        }
        else if (args.Length == 2 && int.TryParse(args[0], out port))
        {
            url = args[1];

        }
        else
        {
            LogError("Incorrect arguments specified! Cannot start.");
            Log(HelpMessage, ConsoleColor.White);
            return;
        }
        OllamaChatSession session = new();

        session.RunServer(listenIpv4Addr, listenIpv6Addr, port, ConnectionKey);
    }
}
