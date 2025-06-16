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

    public static readonly IEnumerable<Tool> AllTools =
    [
        new GetCurrentWeatherTool(),
        new GetCurrentNewsTool(),
        new GetMyHomeTool(),
        new GetMyQuestsTool(),
        new SetQuestCompletedTool(),
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
    /// Get what quests you want someone to do for you.
    /// </summary>
    /// <returns>The Id and descriptions of quests that you want someone to do for you.</returns>
    [OllamaTool]
    public static string GetMyQuests()
    {
        Console.WriteLine($"{nameof(GetMyQuests)} called");
        if (Instance == null || Instance._rootQuestInfo == null || Instance.activeCharacter == null)
        {
            return "No quests available.";
        }


        


        return "Not yet implemented";
    }


    /// <summary>
    /// Sets a quest as completed. Use GetMyQuests() before calling this to get the quest id.
    /// </summary>
    /// <param name="questId">The identifier of the quest to complete.</param>
    /// <returns>Status message about the operation</returns>
    [OllamaTool]
    public static string SetQuestOrSubquestCompleted(string questId)
    {
        Console.WriteLine($"{nameof(SetQuestOrSubquestCompleted)} called with id: {questId}");

        if (Instance == null || Instance._rootQuestInfo == null || Instance.activeCharacter == null)
        {
            return "No info available.";
        }

        return "Not yet implemented.";
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
