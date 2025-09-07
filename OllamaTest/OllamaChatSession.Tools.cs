using Backend.Messages;
using LiteNetLib;
using LiteNetLib.Utils;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Backend;

partial class OllamaChatSession
{
    //I hate this but tools need to be static
    public static OllamaChatSession? Instance;
    public OllamaChatSession()
    {
        Instance ??= this;
    }
    //TODO: RENAME THIS SHIT
    //CommissionedQuest?
    //UnfulfilledQuests?
    //change npc prompt
    //MAKE MORE CONSISTENT
    //Change temp settings
    public static readonly IEnumerable<Tool> AllTools =
    [
        //new GetCurrentWeatherTool(),
        //new GetCurrentNewsTool(),
        new GetMyHomeTool(),
        new GetQuestsForPlayerTool(),
        new StartPlayerQuestTool(),
        new GetCurrentPlayerActiveQuestTool(),
        new GetCompletableJobsTool(),
        new MarkJobAsCompleteTool(),
        new GiveItemToPlayerTool(),
        new GetItemsTool(),
    ];

    public static IEnumerable<Tool> SelectTools(string[] tools)
    {
        //Not optimal perfomance but whatever
        return AllTools.Where(x => tools.Contains(x.GetType().Name));
    }

    /// <summary>
    /// Get which biome you have your home in.
    /// </summary>
    /// <returns>The biome you have your home located in.</returns>
    [OllamaTool]
    public static string GetMyHome()
    {
        Console.WriteLine($"{nameof(GetMyHome)} called");
        if (Instance == null || Instance._worldInfo == null || Instance._activeCharacter == null)
        {
            return "No info available.";
        }

        var biome = Instance._worldInfo.NpcBiomeInfos.Where(x => x.NpcName == Instance._activeCharacter.Name).FirstOrDefault().BiomeName;
        if (biome == null)
        {
            return "Your home biome is unknown.";
        }
        return $"You live in the {biome} biome!";
    }

    /// <summary>
    /// Get what quests you want the player to do for you.
    /// </summary>
    /// <returns>The Id and descriptions of quests that you want the player to do for you.</returns>
    [OllamaTool]
    public static string GetQuestsForPlayer()
    {
        LogInfo($"{nameof(GetQuestsForPlayer)} called");

        if (Instance == null || Instance._activeCharacter == null)
        {
            LogError($"Failed. Instance: {Instance == null} ActiveCharacter: {Instance?._activeCharacter == null}");
            return "No quests available";
        }
        var state = Instance.GetActiveNpcState();

        if (string.IsNullOrWhiteSpace(state.AvailableQuests))
        {
            LogError($"Failed. No available quests for {Instance._activeCharacter.Name}");
            return "No quests available";
        }
        LogInfo(state.AvailableQuests);
        return "The quests that you want the player to do are: \"" + state.AvailableQuests + "\"";
    }
    /// <summary>
    /// Returns the current quest you have asked the player to do
    /// </summary>
    /// <returns>Your currently active quest</returns>
    [OllamaTool]
    public static string GetCurrentPlayerActiveQuest()
    {
        LogInfo($"{nameof(GetCurrentPlayerActiveQuest)} called");

        if (Instance == null || Instance._activeCharacter == null)
        {
            return "No quest activated";
        }
        var state = Instance.GetActiveNpcState();
        LogInfo(state.CurrentQuest);
        return state.CurrentQuest ?? "No quest activated";
    }
    /// <summary>
    /// Starts a quest for the player! MUST Use GetQuestsForPlayer to get the id
    /// </summary>
    /// <param name="id">The Id of the quest to start</param>
    /// <returns>Info about the result of this operation</returns>
    [OllamaTool]
    public static string StartPlayerQuest(string id)
    {
        LogInfo($"{nameof(StartPlayerQuest)} called with id: {id}");


        if (Instance == null || Instance._unityPeer == null || Instance._activeCharacter == null)
        {
            return $"Could not start quest with id: {id} Not connected to the game.";
        }
        var state = Instance.GetActiveNpcState();

        if (state.AvailableQuests?.Contains(id) ?? false)
        {
            var response = new QuestStartedInfo(id);
            Instance._netPacketProcessor.Write(Instance._writer, response);
            Instance._unityPeer.Send(Instance._writer, DeliveryMethod.ReliableOrdered);
            Instance._writer.Reset();
            return $"Successfully started quest with id: {id}";
        }

        return $"Could not start quest with id: {id} Make sure its id was spelled correctly and try again.";
    }

    /// <summary>
    /// Get which jobs you can mark as complete
    /// </summary>
    /// <returns>Info about the result of this operation</returns>
    [OllamaTool]
    public static string GetCompletableJobs()
    {
        LogInfo($"{nameof(GetCompletableJobs)} called");

        if (Instance == null)
        {
            return "No tasks available";
        }

        if (Instance._activeCharacter == null)
        {
            return "No tasks available";
        }

        return Instance.GetActiveNpcState().CompletableTasks ?? "No tasks available";
    }

    /// <summary>
    /// Sets a job as completed. Must always use GetCompletableJobs before calling this to get the job id.
    /// </summary>
    /// <param name="jobId">The identifier of the job to complete.</param>
    /// <returns>Status message about the operation</returns>
    [OllamaTool]
    public static string MarkJobAsComplete(string jobId)
    {
        LogInfo($"{nameof(MarkJobAsComplete)} called with id: {jobId}");

        if (Instance == null || Instance._unityPeer == null || Instance._activeCharacter == null)
        {
            return $"Could not complete job id: {jobId} Not connected to the game.";
        }
        var state = Instance.GetActiveNpcState();
        if (state.CompletableTasks?.Contains(jobId) ?? false)
        {
            var response = new TaskCompletedInfo(jobId);
            Instance._netPacketProcessor.Write(Instance._writer, response);
            Instance._unityPeer.Send(Instance._writer, DeliveryMethod.ReliableOrdered);
            Instance._writer.Reset();
            return $"Successfully completed job with id: {jobId}";
        }

        return $"Could not complete job with id: {jobId} Make sure its id was spelled correctly and try again.";
    }

    /// <summary>
    /// Gets which items are in the Npc inventory. Must call GiveItemToPlayer after if an item should be transfered
    /// </summary>
    /// <returns>A list of items and their counts in the Npc inventory.</returns>
    [OllamaTool]
    public static string GetItems()
    {
        LogInfo($"{nameof(GetItems)} called");

        if (Instance == null || Instance._unityPeer == null || Instance._activeCharacter == null)
        {
            return $"ERROR: Could not get items. Not connected to the game.";
        }
        var state = Instance.GetActiveNpcState();
        string text;
        if (state.InventoryState.Count > 0)
        {
            StringBuilder sb = new();
            sb.AppendLine($"{Instance._activeCharacter.Name} has the following items in their inventory:");
            foreach (var item in state.InventoryState)
            {
                //E.g.: Mushroom (x10)
                sb.AppendLine($"{item.Name} Additional info: {item.Condition ?? ""}");
            }
            text = sb.ToString();
        }
        else
        {
            text = $"{Instance._activeCharacter.Name} has nothing in their inventory";
        }
        LogInfo(text);
        return text;
    }

    /// <summary>
    /// Gives an item from the Npc inventory to the player. Must always use GetItems before calling this to get the name of the item to give.
    /// </summary>
    /// <param name="itemName">The item to give to the player</param>
    /// <returns>Info about the result of this operation</returns>
    [OllamaTool]
    public static string GiveItemToPlayer(string itemName)
    {
        LogInfo($"{nameof(GiveItemToPlayer)} called with item: {itemName}");

        if (Instance == null || Instance._unityPeer == null || Instance._activeCharacter == null)
        {
            return $"ERROR: Could not give item \"{itemName}\" to player. Not connected to the game.";
        }
        var state = Instance.GetActiveNpcState();
        if (state.InventoryState.Any(x => x.Name == itemName))
        {
            var response = new GiveItemToPlayerInfo(itemName, Instance._activeCharacter.Name);
            NetPacketProcessor _netPacketProcessor = new();
            var writer = new NetDataWriter();
            _netPacketProcessor.WriteNetSerializable(writer, ref response);
            Instance._unityPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            writer.Reset();
            LogInfo($"{nameof(GiveItemToPlayer)} called with item: {itemName}");
            return $"Successfully gave item \"{itemName}\" to player";
        }
        LogInfo("Item not found: " + itemName);
        return $"Could not give item \"{itemName}\" to player. The item was not found in the inventory. Please ensure that the name is spelled correctly and try again!";
    }

    /// <summary>
    /// Get the current news for a specified location
    /// </summary>
    /// <param name="location">The location to get the news for, e.g. San Francisco, CA</param>
    /// <param name="category">The category of news to look for</param>
    /// <returns>The current news for the specified location, in the specified category</returns>
    [OllamaTool]
    public static string GetCurrentNews(string location, string category)
    {
        category = string.IsNullOrEmpty(category) ? "all" : category;
        return $"Could not find news for {location} (category: {category}).";
    }

    public enum Unit
    {
        Celsius,
        Fahrenheit
    }
    /// <summary>
    /// Get the current weather for a location
    /// </summary>
    /// <param name="location">The location to get the weather for, e.g. San Francisco, CA</param>
    /// <param name="unit">The Unit to return the weather in, 'celsius' or 'fahrenheit'</param>
    /// <returns>The temperature for the given location</returns>
    [OllamaTool]
    public static string GetCurrentWeather(string location, Unit unit)
    {
        var (temperature, foramt) = unit switch
        {
            Unit.Fahrenheit => (Random.Shared.Next(23, 104), "°F"),
            _ => (Random.Shared.Next(-5, 40), "°C"),
        };

        return $"{temperature} {foramt} in {location}";
    }
}
