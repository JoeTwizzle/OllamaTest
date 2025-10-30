using System.Text;

namespace Backend;

public static class GameLogger
{
    private static string? _filePath;
    private static readonly object _lock = new object();
    private static bool _initialized;
    private static StreamWriter? _writer;


    public static void Init(string filePath)
    {
        if (_initialized) return;
        _filePath = Path.GetFullPath(filePath ?? throw new ArgumentNullException(nameof(filePath)));
        var dir = Path.GetDirectoryName(_filePath);
        if (dir != null)
        {
            Directory.CreateDirectory(dir);
        }

        _writer = new StreamWriter(new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8)
        {
            AutoFlush = false
        };


        AppDomain.CurrentDomain.ProcessExit += (a, b) =>
        {
            Flush();
        };
        AppDomain.CurrentDomain.UnhandledException += (a, b) =>
        {
            Log(LogLevel.Critical, ((Exception)b.ExceptionObject).ToString(), (Exception)b.ExceptionObject);
            Flush();
        };

        _initialized = true;
    }


    public static void Log(LogLevel level, string? message, Exception? ex = null)
    {
        if (!_initialized || _writer == null)
        {
            throw new InvalidOperationException("Logger not initialized.");
        }

        if (message == null)
        {
            return;
        }
        Utils.Log(message, ConsoleColor.Gray, level);

        var ts = DateTime.Now;
        _writer.WriteLine("-------------Start-------------");
        string line = $"{ts:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
        lock (_lock)
        {
            _writer.WriteLine(line);
            if (ex != null)
            {
                _writer.WriteLine($"Exception: {ex.Message}");
                _writer.WriteLine(ex.StackTrace);
            }
        }
        _writer.WriteLine("--------------End---------------");
        _writer.WriteLine();
    }


    public static void Info(string? message) => Log(LogLevel.Information, message);
    public static void Warn(string? message) => Log(LogLevel.Critical, message);
    public static void Error(string? message, Exception? ex = null) => Log(LogLevel.Error, message, ex);


    public static void Flush()
    {
        lock (_lock)
        {
            _writer?.Flush();
        }
    }


    public static void Shutdown()
    {
        lock (_lock)
        {
            _writer?.Flush();
            _writer?.Dispose();
            _initialized = false;
        }
    }
}
