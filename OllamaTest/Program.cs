global using static Backend.Utils;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
namespace Backend;

internal class Program
{
    //TODO: INVESTIGATE NPC MEMORY
    const int Port = 9050;
    const string ConnectionKey = "eotwConnectionKey";

    static async Task Main(string[] args)
    {
        IPAddress listenIpv4Addr = IPAddress.Loopback;
        IPAddress listenIpv6Addr = IPAddress.IPv6Loopback;

        OllamaChatSession session = new();
        session.RunServer(listenIpv4Addr, listenIpv6Addr, Port, ConnectionKey);
    }
}
