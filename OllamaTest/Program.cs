global using static Backend.Utils;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
namespace Backend;

internal class Program
{
    //TODO: INVESTIGATE NPC MEMORY
    //TODO: STRUCTURE LOGS AS CSV
    //TODO: LOG FUNCTION CALLS
    const int Port = 9050;
    const string ConnectionKey = "eotwConnectionKey";

    static async Task Main(string[] args)
    {
        IPAddress listenIpv4Addr = IPAddress.Loopback;
        IPAddress listenIpv6Addr = IPAddress.IPv6Loopback;

        OllamaChatSession session = new();
        GameLogger.Init($"Logs/Session {DateTime.Now:yyyy-MM-dd HH-mm-ss}.txt");
        session.RunServer(listenIpv4Addr, listenIpv6Addr, Port, ConnectionKey);
        Debug.Assert(await GameLogger.Shutdown());
    }
}
