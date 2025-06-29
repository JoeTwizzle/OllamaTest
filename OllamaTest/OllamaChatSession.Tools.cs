using Backend.Messages;
using LiteNetLib;
using OllamaSharp;
using OllamaSharp.Models.Chat;

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
        if (Instance == null || Instance._worldInfo == null || Instance.activeCharacter == null)
        {
            return "No info available.";
        }

        var biome = Instance._worldInfo.NpcBiomeInfos.Where(x => x.NpcName == Instance.activeCharacter.Name).FirstOrDefault().BiomeName;
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
        Console.WriteLine($"{nameof(GetQuestsForPlayer)} called");

        if (Instance == null || Instance.activeCharacter == null)
        {
            Console.WriteLine($"Failed. Instance: {Instance == null} ActiveCharacter: {Instance?.activeCharacter == null}");
            return "No quests available";
        }

        if (!Instance.NpcAvailableQuests.TryGetValue(Instance.activeCharacter.Name, out var quests) || string.IsNullOrWhiteSpace(quests))
        {
            Console.WriteLine($"Failed. No active quests for {Instance.activeCharacter.Name}");
            return "No quests available";
        }
        Console.WriteLine(quests);
        return "The quests that you want the player to do are: \"" + quests + "\"";
    }
    /// <summary>
    /// Returns the current quest you have asked the player to do
    /// </summary>
    /// <returns>Your currently active quest</returns>
    [OllamaTool]
    public static string GetCurrentPlayerActiveQuest()
    {
        Console.WriteLine($"{nameof(GetCurrentPlayerActiveQuest)} called");

        if (Instance == null || Instance.activeCharacter == null || !Instance.NpcCurrentQuest.TryGetValue(Instance.activeCharacter.Name, out var quest))
        {
            return "No quest activated";
        }
        Console.WriteLine(quest);
        return quest;
    }
    /// <summary>
    /// Starts a quest for the player! MUST Use GetQuestsForPlayer to get the id
    /// </summary>
    /// <param name="id">The Id of the quest to start</param>
    /// <returns>Info about the result of this operation</returns>
    [OllamaTool]
    public static string StartPlayerQuest(string id)
    {
        Console.WriteLine($"{nameof(StartPlayerQuest)} called with id: {id}");


        if (Instance == null || Instance._unityPeer == null || Instance.activeCharacter == null)
        {
            return $"Could not start quest with id: {id} Not connected to the game.";
        }

        if (Instance.NpcAvailableQuests.TryGetValue(Instance.activeCharacter.Name, out var quests) && quests.Contains(id))
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
        Console.WriteLine($"{nameof(GetCompletableJobs)} called");

        if (Instance == null || Instance.activeCharacter == null || !Instance.NpcCompletableTasks.TryGetValue(Instance.activeCharacter.Name, out var tasks))
        {
            return "No tasks available";
        }

        return tasks;
    }

    /// <summary>
    /// Sets a job as completed. Must always use GetCompletableJobs before calling this to get the job id.
    /// </summary>
    /// <param name="jobId">The identifier of the job to complete.</param>
    /// <returns>Status message about the operation</returns>
    [OllamaTool]
    public static string MarkJobAsComplete(string jobId)
    {
        Console.WriteLine($"{nameof(MarkJobAsComplete)} called with id: {jobId}");

        if (Instance == null || Instance._unityPeer == null || Instance.activeCharacter == null)
        {
            return $"Could not complete job id: {jobId} Not connected to the game.";
        }

        if (Instance.NpcCompletableTasks.TryGetValue(Instance.activeCharacter.Name, out var tasks) && tasks.Contains(jobId))
        {
            var response = new TaskCompletedInfo(jobId);
            Instance._netPacketProcessor.Write(Instance._writer, response);
            Instance._unityPeer.Send(Instance._writer, DeliveryMethod.ReliableOrdered);
            Instance._writer.Reset();
            return $"Successfully completed job with id: {jobId}";
        }

        return $"Could not completet job with id: {jobId} Make sure its id was spelled correctly and try again.";
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
