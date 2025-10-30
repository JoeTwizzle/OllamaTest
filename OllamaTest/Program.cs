global using static Backend.Utils;
using System.Net;
namespace Backend;

internal class Program
{
    //TODO: INVESTIGATE NPC MEMORY
    const int Port = 9050;
    const string ConnectionKey = "eotwConnectionKey";

    static void Main(string[] args)
    {
        IPAddress listenIpv4Addr = IPAddress.Loopback;
        IPAddress listenIpv6Addr = IPAddress.IPv6Loopback;

        OllamaChatSession session = new();
        GameLogger.Init($"Logs/Session {DateTime.Now:yyyy-MM-dd HH-mm-ss}.txt");
        session.RunServer(listenIpv4Addr, listenIpv6Addr, Port, ConnectionKey);
        GameLogger.Shutdown();
    }
}
