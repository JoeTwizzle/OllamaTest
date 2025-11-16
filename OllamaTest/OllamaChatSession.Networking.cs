using Backend.Messages;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Diagnostics;
using System.Net;

namespace Backend;
partial class OllamaChatSession
{
    public bool ShouldRun { get; set; }

    private NetPeer? _unityPeer;
    private readonly NetPacketProcessor _netPacketProcessor = new();
    private readonly NetDataWriter _writer = new();
    private QuestInfo? _rootQuestInfo;
    private WorldInfo? _worldInfo;

    public void RunServer(IPAddress ipv4, IPAddress ipv6, int port, string connectionKey)
    {
        ShouldRun = true;
        EventBasedNetListener listener = new();
        NetManager server = new(listener);
        server.DisconnectTimeout = 20000;
        if (!server.Start(ipv4, ipv6, port))
        {
            LogError("Error starting server!");
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
        _netPacketProcessor.SubscribeNetSerializable<InitBackendMessage>(OnInitBackendRecieved);
        _netPacketProcessor.SubscribeNetSerializable<NPCItemChangeInfo>(OnNPCItemChanged);
        _netPacketProcessor.SubscribeNetSerializable<NPCInventoryChangeInfo>(OnNPCInventoryChanged);
        _netPacketProcessor.SubscribeNetSerializable<QuestInfo>(OnQuestRootRecieved);
        _netPacketProcessor.SubscribeNetSerializable<UpdateQuestsInfo>(OnQuestInfoRecieved);
        _netPacketProcessor.SubscribeNetSerializable<UpdateTasksInfo>(OnTaskInfoRecieved);
        _netPacketProcessor.SubscribeNetSerializable<SetCharacterInfo>(OnCharacterRecieved);
        _netPacketProcessor.SubscribeNetSerializable<UpdateActiveQuestsInfo>(OnActiveQuestInfoRecieved);
        _netPacketProcessor.SubscribeNetSerializable<GeneratedDescriptionInfo>(OnDescriptionInfoRecieved);
        _netPacketProcessor.SubscribeNetSerializable<SaveToFileInfo>(OnSaveInfoRecieved);
        _netPacketProcessor.SubscribeNetSerializable<LoadFromFileInfo>(OnLoadInfoRecieved);

        _netPacketProcessor.SubscribeNetSerializable<TimeOfDayInfo>(OnTimeOfDayRecieved);
        _netPacketProcessor.SubscribeNetSerializable<WeatherInfo>(OnWeatherRecieved);
        _netPacketProcessor.SubscribeNetSerializable<PlayerAnimalInfo>(OnPlayerAnimalRecieved);
        listener.PeerConnectedEvent += peer =>
        {
            _unityPeer = peer;
            GameLogger.Init($"Logs/Session {DateTime.Now:yyyy-MM-dd HH-mm-ss}.csv");
            LogEvent("Unity connected!");
        };
        listener.PeerDisconnectedEvent += async (peer, info) =>
        {
            LogEvent("Unity disconnected!");
            ClearNpcStates();
            Debug.Assert(await GameLogger.Shutdown());
        };
        listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
        listener.NetworkReceiveUnconnectedEvent += Listener_NetworkReceiveUnconnectedEvent;
        LogEvent("Server started. Waiting for connection", ConsoleColor.Green);
        while (ShouldRun)
        {
            server.PollEvents();
            Thread.Sleep(1);
        }
        server.Stop();
    }

    private void OnTimeOfDayRecieved(TimeOfDayInfo timeOfDayInfo)
    {
        worldTimeOfDayText = $"It is currently {timeOfDayInfo.TimeOfDay}";
    }
    private void OnWeatherRecieved(WeatherInfo weatherInfo)
    {
        worldWeatherText = $"The current weather is: {weatherInfo.Weather}";
    }
    private void OnPlayerAnimalRecieved(PlayerAnimalInfo playerAnimalInfo)
    {
        playerAnimalText = $"The player's name is {playerAnimalInfo.AnimalName} they are a {playerAnimalInfo.AnimalSpecies}";
    }

    private readonly NetDataWriter _resultWriter = new();
    private void SendResult(string id, bool success)
    {
        if (_unityPeer == null)
        {
            LogError("Cannot send result. Unity disconnected.");
            return;
        }
        try
        {
            LogEvent("Sending result ok");
            var response = new ResultInfo(id, success);
            _netPacketProcessor.WriteNetSerializable(_resultWriter, ref response);
            _unityPeer.Send(_resultWriter, DeliveryMethod.ReliableUnordered);
            _resultWriter.Reset();
        }
        catch (Exception e)
        {
            LogError($"Cannot send result. {e}");
        }
    }

    private async void OnInitBackendRecieved(InitBackendMessage message)
    {
        var success = await InitOllama(message.OllamaUri, message.LanguageModel, message.EmbeddingModel);
        SendResult(nameof(InitBackendMessage), success);
    }

    private async void OnNPCItemChanged(NPCItemChangeInfo info)
    {
        LogEvent("Items changed: " + info.ItemName + " for " + info.NPCName);
        bool success = false;
        try
        {
            var doc = await GetEmbeddedInventoryAsync(info.NPCName);

            if (info.Added)
            {
                AddItem(info.NPCName, info.ItemName);
            }
            else
            {
                RemoveItem(info.NPCName, info.ItemName);
            }
            if (doc != null)
            {
                RemoveDocument(info.NPCName, doc.Text);
            }

            var newdoc = await GetEmbeddedInventoryAsync(info.NPCName);
            if (newdoc != null)
            {
                AddDocument(info.NPCName, newdoc);
            }
            success = true;
        }
        catch (Exception e)
        {
            success = false;
            LogError(e.ToString());
        }
        SendResult(nameof(NPCItemChangeInfo), success);
    }

    private void OnNPCInventoryChanged(NPCInventoryChangeInfo info)
    {
        bool success = false;
        LogEvent("inventory changed for " + info.NPCName, ConsoleColor.Green);
        LogEvent("ItemNames" + info.NPCName, ConsoleColor.DarkGreen);
        foreach (var item in info.ItemNames)
        {
            LogEvent(item, ConsoleColor.DarkGreen);
        }
        LogEvent("LockedItemNames", ConsoleColor.DarkGreen);
        foreach (var item in info.LockedItemNames)
        {
            LogEvent(item, ConsoleColor.DarkGreen);
        }
        LogEvent("LockedItemReasons", ConsoleColor.DarkGreen);
        foreach (var item in info.LockedItemReasons)
        {
            LogEvent(item, ConsoleColor.DarkGreen);
        }
        try
        {
            var oldInvText = GetInventoryString(info.NPCName);

            if (!string.IsNullOrWhiteSpace(oldInvText))
            {
                RemoveDocument(info.NPCName, oldInvText);
            }

            SetInventory(info.NPCName, info);

            var newdoc = GetEmbeddedInventoryAsync(info.NPCName).Result;
            if (newdoc != null)
            {
                AddDocument(info.NPCName, newdoc);
            }
            success = true;
        }
        catch (Exception e)
        {
            success = false;
            LogError($"Failed to set inventory for {info.NPCName} {e}");
        }
        SendResult(nameof(NPCInventoryChangeInfo), success);
    }

    private void OnHeartBeatRecieved(HeartBeatInfo info)
    {
        if (_unityPeer == null)
        {
            LogError("Dead");
            return;
        }
        LogInfo("Alive");
        _netPacketProcessor.Write(_writer, new HeartBeatInfo());

        _unityPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
        _writer.Reset();
    }

    private void OnSaveInfoRecieved(SaveToFileInfo info)
    {
        LogEvent($"Saving info to: {info.Path}");
        Save(info.Path);
    }

    private void OnLoadInfoRecieved(LoadFromFileInfo info)
    {
        LogEvent($"Loading info from: {info.Path}");
        Load(info.Path);
    }

    private void OnQuestInfoRecieved(UpdateQuestsInfo info)
    {
        var state = GetNpcState(info.Name);
        var prevQuest = state.AvailableQuests;
        state.AvailableQuests = info.Quests;
        try
        {
            string input;
            if (string.IsNullOrWhiteSpace(info.Quests))
            {
                input = "You currently don't need help with anything";
            }
            else
            {
                input = $"""
                The things (quests) {info.Name} wants the player to help with are:           
                {info.Quests}
                """;
            }

            LogEvent($"Added UpdateQuestsInfo to RAG:{Environment.NewLine}{input}{Environment.NewLine}", ConsoleColor.Cyan);

            string prevDoc;
            if (string.IsNullOrWhiteSpace(prevQuest))
            {
                prevDoc = "You currently don't need help with anything";
            }
            else
            {
                prevDoc = $"""
                The things (quests) {info.Name} wants the player to help with are:           
                {prevQuest}
                """;
            }
            RemoveDocument(info.Name, prevDoc);


            AddDocument(info.Name, input).Wait();
        }
        catch
        {
            LogError("Could not add UpdateQuestsInfo to RAG");
        }
    }

    private async void OnActiveQuestInfoRecieved(UpdateActiveQuestsInfo info)
    {
        var state = GetNpcState(info.Name);
        var prevActiveQuest = state.CurrentQuest;
        state.CurrentQuest = info.Quest;
        try
        {
            string input;
            if (string.IsNullOrWhiteSpace(state.CurrentQuest))
            {
                input = $"{info.Name} has not tasked the player with any quest at the moment.";
            }
            else
            {
                input = $"The current quest that {info.Name} has tasked the player with has Identifier:\n{info.Quest}";
            }
            LogEvent($"Added UpdateActiveQuestsInfo to RAG:{Environment.NewLine}{input}{Environment.NewLine}", ConsoleColor.Cyan);
            //TODO Maybe this is the problem?

            string prevDoc;
            if (string.IsNullOrWhiteSpace(prevActiveQuest))
            {
                prevDoc = $"{info.Name} has not tasked the player with any quest at the moment.";
            }
            else
            {
                prevDoc = $"The current quest that {info.Name} has tasked the player with has Identifier:\n{prevActiveQuest}";
            }
            RemoveDocument(info.Name, prevDoc);


            await AddDocument(info.Name, input);
        }
        catch
        {
            LogError("Could not add UpdateActiveQuestsInfo to RAG");
        }
    }

    private async void OnTaskInfoRecieved(UpdateTasksInfo info)
    {
        var state = GetNpcState(info.Name);
        state.CompletableTasks = info.Quests;
        try
        {
            string input = $"The tasks, that {info.Name} can mark as completed for the current active quest, are:\n{info.Quests}";
            LogEvent($"Added UpdateTasksInfo to RAG:{Environment.NewLine}{input}{Environment.NewLine}");
            await AddDocument(info.Name, input);
        }
        catch
        {
            LogError("Could not add UpdateTasksInfo to RAG");
        }
    }

    private void OnDescriptionInfoRecieved(GeneratedDescriptionInfo info)
    {
        LogInfo(info.RawDescription);
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
        LogEvent("Loading character...");
        LogInfo(characterInfo.ToString());
        while (Interlocked.CompareExchange(ref waitForEvaluation, 1, 0) != 0) ;
        LoadCharacter(characterInfo.NPCCharacterInfo, characterInfo.ForceReload).Wait();
        Interlocked.Exchange(ref waitForEvaluation, 0);
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
        PersistMessageHistory();
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
        TryRestoreMessageHistory();
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
        ClearActiveNpcState();

        if (_unityPeer != null)
        {
            var info = new AcknowledgeInfo();
            _netPacketProcessor.Write(_writer, info);
            _unityPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
            _writer.Reset();
        }
    }
}
