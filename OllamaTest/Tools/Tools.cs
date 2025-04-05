using OllamaSharp.Models.Chat;

namespace Backend.Tools;

static partial class Tools
{
    //TODO: Automatically update this list. (Reflection?)
    public static readonly IEnumerable<Tool> AllTools = [new GetCurrentWeatherTool(), new GetCurrentNewsTool()];
    public static IEnumerable<Tool> SelectTools(string[] tools)
    {
        //Not optimal perfomance but whatever
        return AllTools.Where(x => tools.Contains(x.GetType().Name));
    }
}
