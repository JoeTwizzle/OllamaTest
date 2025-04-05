using Backend.Messages;
using LiteNetLib;
using LiteNetLib.Utils;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System.Net;

namespace Backend;

class OllamaChatSession
{
    public bool ShouldRun { get; set; }

    private NetPeer? _unityPeer;
    private readonly NetPacketProcessor _netPacketProcessor = new NetPacketProcessor();
    private readonly NetDataWriter _writer = new NetDataWriter();

    public void RunServer(int port, string connectionKey)
    {
        ShouldRun = true;
        EventBasedNetListener listener = new EventBasedNetListener();
        NetManager server = new NetManager(listener);
        server.Start(IPAddress.Loopback, IPAddress.IPv6Loopback, port);

        listener.ConnectionRequestEvent += request =>
        {
            //Only allow one connection max
            if (server.ConnectedPeersCount == 0)
            {
                request.AcceptIfKey(connectionKey);
            }
            else
            {
                request.Reject();
            }
        };
        _netPacketProcessor.SubscribeNetSerializable<MessageInfo>(OnMessageRecieved);
        _netPacketProcessor.SubscribeNetSerializable<CharacterInfo>(OnCharacterRecieved);
        listener.PeerConnectedEvent += peer =>
        {
            _unityPeer = peer;
        };
        while (ShouldRun)
        {
            server.PollEvents();
        }
        server.Stop();
    }

    private void OnCharacterRecieved(CharacterInfo characterInfo)
    {
        LoadCharacter(characterInfo, true);
    }

    private async void OnMessageRecieved(MessageInfo messageInfo)
    {
        await ChatAsync(messageInfo.Message);
    }

    OllamaApiClient? ollama;
    public async Task InitOllama(string activeModel)
    {
        ollama = await ConnectAsync();
        await PullModel(ollama, activeModel);
        ollama.SelectedModel = activeModel;

        Console.WriteLine("System initialized!");
        Console.WriteLine();
    }

    Chat? chat;
    IEnumerable<Tool>? activeTools;
    public void LoadCharacter(CharacterInfo characterInfo, bool forceReload)
    {
        //TODO: save context of existing character/s 
        //TODO: handle force reloading
        //TODO: structured character info handling

        if (ollama == null) throw new InvalidOperationException("Ollama must be initialized before loading a character.");
        chat = new Chat(ollama, characterInfo.Prompt);
        activeTools = Tools.Tools.SelectTools(characterInfo.AvailableTools);
    }

    public async Task ChatAsync(string message)
    {
        if (chat == null) throw new InvalidOperationException("A character must be loaded before you may chat.");
        //Send message
        await foreach (var answerToken in chat.SendAsync(message, activeTools))
        {
            //Stream response
            if (_unityPeer != null)
            {
                _writer.Put(new AnswerTokenInfo(answerToken));
                _unityPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
            }
            Console.Write(answerToken);
        }
    }

    private static async Task PullModel(OllamaApiClient ollama, string selectedModel)
    {
        var models = await ollama.ListLocalModelsAsync();
        if (!models.Any(x => x.Name == selectedModel))
        {
            Console.WriteLine($"Model {selectedModel} not found. Downloading...");
            bool completed = false;
            await foreach (var response in ollama.PullModelAsync(selectedModel))
            {
                if (response == null) { continue; }
                Console.WriteLine($"Downloaded {response.Percent:F1}%");
                completed = response.Total == response.Completed;
            }
            while (!completed) ;
        }
    }

    private static async Task<OllamaApiClient> ConnectAsync()
    {
        OllamaApiClient? ollama = null;
        var connected = false;
        do
        {
            var url = "http://localhost:11434";
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
