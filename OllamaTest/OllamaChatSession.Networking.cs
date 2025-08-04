using Backend.Messages;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Backend;
partial class OllamaChatSession
{
    public bool ShouldRun { get; set; }

    private NetPeer? _unityPeer;
    private readonly NetPacketProcessor _netPacketProcessor = new();
    private readonly NetDataWriter _writer = new();
    private QuestInfo? _rootQuestInfo;
    private WorldInfo? _worldInfo;

    public void RunServer(int port, string connectionKey)
    {
        ShouldRun = true;
        EventBasedNetListener listener = new EventBasedNetListener();
        NetManager server = new(listener);
        if (!server.Start(IPAddress.Loopback, IPAddress.IPv6Loopback, port))
        {
            Console.WriteLine("Error starting server!");
            return;
        }

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
        _netPacketProcessor.SubscribeReusable<MessageInfo>(OnMessageRecieved);
        _netPacketProcessor.SubscribeReusable<SaveContextInfo>(OnSaveCommandRecieved);
        _netPacketProcessor.SubscribeReusable<LoadContextInfo>(OnLoadCommandRecieved);
        _netPacketProcessor.SubscribeReusable<ClearContextInfo>(OnClearCommandRecieved);
        _netPacketProcessor.SubscribeReusable<HeartBeatInfo>(OnHeartBeatRecieved);
        _netPacketProcessor.SubscribeNetSerializable<WorldInfo>(OnWorldInfoRecieved);
        _netPacketProcessor.SubscribeNetSerializable<QuestInfo>(OnQuestRootRecieved);
        _netPacketProcessor.SubscribeNetSerializable<UpdateQuestsInfo>(OnQuestInfoRecieved);
        _netPacketProcessor.SubscribeNetSerializable<UpdateTasksInfo>(OnTaskInfoRecieved);
        _netPacketProcessor.SubscribeNetSerializable<SetCharacterInfo>(OnCharacterRecieved);
        _netPacketProcessor.SubscribeNetSerializable<UpdateActiveQuestsInfo>(OnActiveQuestInfoRecieved);
        _netPacketProcessor.SubscribeNetSerializable<GeneratedDescriptionInfo>(OnDescriptionInfoRecieved);
        _netPacketProcessor.SubscribeNetSerializable<SaveToFileInfo>(OnSaveInfoRecieved);
        _netPacketProcessor.SubscribeNetSerializable<LoadFromFileInfo>(OnLoadInfoRecieved);
        listener.PeerConnectedEvent += peer =>
        {
            _unityPeer = peer;
            Console.WriteLine("Unity connected!");
        };
        listener.PeerDisconnectedEvent += (peer, info) =>
        {
            Console.WriteLine("Unity disconnected!");
            ClearDocuments();
            ClearContext();
        };
        listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
        listener.NetworkReceiveUnconnectedEvent += Listener_NetworkReceiveUnconnectedEvent;
        while (ShouldRun)
        {
            server.PollEvents();
        }
        server.Stop();
    }

    private void OnHeartBeatRecieved(HeartBeatInfo info)
    {
        if (_unityPeer == null)
        {
            Console.WriteLine("Dead");
            return;
        }
        Console.WriteLine("Alive");
        _netPacketProcessor.Write(_writer, new HeartBeatInfo());

        _unityPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
        _writer.Reset();
    }

    private void OnSaveInfoRecieved(SaveToFileInfo info)
    {
        Console.WriteLine($"Saving info to: {info.Path}");
        Save(info.Path);
    }

    private void OnLoadInfoRecieved(LoadFromFileInfo info)
    {
        Console.WriteLine($"Loading info from: {info.Path}");
        Load(info.Path);
    }

    readonly Dictionary<string, string> NpcCurrentQuest = new();
    readonly Dictionary<string, string> NpcAvailableQuests = new();
    readonly Dictionary<string, string> NpcCompletableTasks = new();

    private async void OnQuestInfoRecieved(UpdateQuestsInfo info)
    {
        ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(NpcAvailableQuests, info.Name, out _);
        value = info.Quests;
        try
        {
            string input = $"""
            The things (quests) {info.Name} wants the player to help with are:           
            {info.Quests}
            Remember to call {nameof(StartPlayerQuest)} when the player has said they want to help with a specific quest!
            """;
            Console.WriteLine($"Added UpdateQuestsInfo to RAG:{Environment.NewLine}{input}{Environment.NewLine}");
            await AddDocument(info.Name, input);
        }
        catch
        {
            Console.WriteLine("Could not add UpdateQuestsInfo to RAG");
        }
    }

    private async void OnActiveQuestInfoRecieved(UpdateActiveQuestsInfo info)
    {
        ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(NpcCurrentQuest, info.Name, out _);
        value = info.Quest;
        try
        {
            string input = $"The current quest that {info.Name} has tasked the player with has Identifier:\n{info.Quest}";
            Console.WriteLine($"Added UpdateActiveQuestsInfo to RAG:{Environment.NewLine}{input}{Environment.NewLine}");
            await AddDocument(info.Name,input);
        }
        catch
        {
            Console.WriteLine("Could not add UpdateActiveQuestsInfo to RAG");
        }
    }

    private async void OnTaskInfoRecieved(UpdateTasksInfo info)
    {
        ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(NpcCompletableTasks, info.Name, out _);
        value = info.Quests;
        try
        {
            string input = $"The tasks, that {info.Name} can mark as completed for the current active quest, are:\n{info.Quests}";
            Console.WriteLine($"Added UpdateTasksInfo to RAG:{Environment.NewLine}{input}{Environment.NewLine}");
            await AddDocument(info.Name,input);
        }
        catch
        {
            Console.WriteLine("Could not add UpdateTasksInfo to RAG");
        }
    }

    private void OnDescriptionInfoRecieved(GeneratedDescriptionInfo info)
    {
        Console.WriteLine(info.RawDescription);
        var task = GenerateDescription(info);
        task.Wait();
        var answer = task.Result;
        var response = new GeneratedResponseInfo(answer, info.RawDescription);
        if (_unityPeer != null)
        {
            _netPacketProcessor.Write(_writer, response);
            Console.WriteLine(answer);

            _unityPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
            _writer.Reset();
        }
    }

    async Task<string> GenerateDescription(GeneratedDescriptionInfo info)
    {
        if (_chat == null) throw new InvalidOperationException("A character must be loaded before you may chat.");
        //Send message
        string answer = "";
        await foreach (var answerToken in _chat.SendAsync(info.RawDescription, _activeTools))
        {
            answer += answerToken;
        }
        return answer.Trim().Replace("\n", null);
    }

    private void OnWorldInfoRecieved(WorldInfo info)
    {
        _worldInfo = info;
    }

    private void OnQuestRootRecieved(QuestInfo info)
    {
        _rootQuestInfo = info;
    }

    private void Listener_NetworkReceiveUnconnectedEvent(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        _netPacketProcessor.ReadAllPackets(reader);
    }

    private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        _netPacketProcessor.ReadAllPackets(reader);
    }

    private void OnCharacterRecieved(SetCharacterInfo characterInfo)
    {
        Console.WriteLine("Loading character...");
        Console.WriteLine(characterInfo);
        LoadCharacter(characterInfo.NPCCharacterInfo, characterInfo.ForceReload);
        if (_unityPeer != null)
        {
            var info = new AcknowledgeInfo();
            _netPacketProcessor.Write(_writer, info);
            _unityPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
            _writer.Reset();
        }
    }

    private async void OnMessageRecieved(MessageInfo messageInfo)
    {
        Console.WriteLine($"Got message: \"{messageInfo.Message}\"");
        try
        {
            await ChatAsync(messageInfo.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void OnSaveCommandRecieved(SaveContextInfo saveContextInfo)
    {
        Console.WriteLine($"Saving context...");
        SaveContext();
        if (_unityPeer != null)
        {
            var info = new AcknowledgeInfo();
            _netPacketProcessor.Write(_writer, info);
            _unityPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
            _writer.Reset();
        }
    }

    private void OnLoadCommandRecieved(LoadContextInfo loadContextInfo)
    {
        Console.WriteLine($"Loading context...");
        TryLoadContext();
        if (_unityPeer != null)
        {
            var info = new AcknowledgeInfo();
            _netPacketProcessor.Write(_writer, info);
            _unityPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
            _writer.Reset();
        }
    }

    private void OnClearCommandRecieved(ClearContextInfo clearContextInfo)
    {
        Console.WriteLine($"Clearing context...");
        ClearContext();

        if (_unityPeer != null)
        {
            var info = new AcknowledgeInfo();
            _netPacketProcessor.Write(_writer, info);
            _unityPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
            _writer.Reset();
        }
    }
}
