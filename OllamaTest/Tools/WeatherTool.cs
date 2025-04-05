using OllamaSharp;

namespace Backend.Tools;

static partial class Tools
{
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
