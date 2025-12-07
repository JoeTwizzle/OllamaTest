
using System;

namespace Backend;
internal static class Utils
{
    public static LogLevel LogLevel = LogLevel.Information;

    public static void LogGame(string? message, ConsoleColor color)
    {
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = prevColor;
    }

    public static void Log(string? message, ConsoleColor color, LogLevel logLevel = LogLevel.Debug)
    {
        if (logLevel < LogLevel) return;

        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = prevColor;
    }

    public static void LogEvent(string? message, ConsoleColor color = ConsoleColor.Blue)
    {
        Log(message, color, LogLevel.Information);
    }

    public static void LogInfo(string? message)
    {
        Log(message, ConsoleColor.Gray, LogLevel.Information);
    }

    public static void LogWarning(string? message)
    {
        Log(message, ConsoleColor.Yellow, LogLevel.Warn);
    }

    public static void LogError(string? message)
    {
        Log(message, ConsoleColor.Red, LogLevel.Error);
    }
}

public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warn,
    Error,
    Critical,
}