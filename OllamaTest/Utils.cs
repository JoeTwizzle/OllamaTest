namespace Backend;
internal static class Utils
{
    public static void Log(string message, ConsoleColor color)
    {
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = prevColor;
    }

    public static void LogError(string message)
    {
        Log(message, ConsoleColor.Red);
    }
}
