using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Backend;

public enum Role
{
    System,
    Player,
    NPC,
    Instructor,
    ToolInvoke,
    ToolResult
}

public static class GameLogger
{
    private static string? _filePath;
    private static readonly object _lock = new object();
    private static bool _initialized;
    private static StreamWriter? _writer;
    private static int GroupId = 0;

    public static void NextGroup()
    {
        GroupId++;
    }

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
        _writer.WriteLine("Role\tTime\tName\tMessage\tGroupId");

        AppDomain.CurrentDomain.ProcessExit += (a, b) =>
        {
            Flush();
        };
        AppDomain.CurrentDomain.UnhandledException += (a, b) =>
        {
            Log(Role.System, "System", ((Exception)b.ExceptionObject).ToString());
            Flush();
        };

        _initialized = true;
    }


    public static void Log(Role role, string name, string? message)
    {
        message ??= "";
        message = message.Replace('\t', ' ');
        message = message.Replace('\n', ' ');
        message = message.Replace('\r', ' ');
        ConsoleColor color = role switch
        {
            Role.System => ConsoleColor.Red,
            Role.Player => ConsoleColor.DarkBlue,
            Role.NPC => ConsoleColor.DarkGreen,
            Role.Instructor => ConsoleColor.DarkYellow,
            Role.ToolInvoke => ConsoleColor.Cyan,
            Role.ToolResult => ConsoleColor.Magenta,
            _ => ConsoleColor.Gray,
        };
        Utils.Log("[" + role + " | " + name + "] " + message, color);
        if (!_initialized || _writer == null)
        {
            throw new InvalidOperationException("Logger not initialized.");
        }

        var ts = DateTime.UtcNow;
        var sb = new StringBuilder();
        //|ROLE|TIME|NAME|MESSAGE|GroupId|
        sb.Append(role.ToString());
        sb.Append('\t');
        sb.Append(ts.ToString(CultureInfo.InvariantCulture));
        sb.Append('\t');
        sb.Append(name);
        sb.Append('\t');
        sb.Append(message);
        sb.Append('\t');
        sb.Append(GroupId);
        _writer.WriteLine(sb.ToString());
    }

    public static void Flush()
    {
        lock (_lock)
        {
            if (_initialized)
            {
                _writer?.Flush();
            }
        }
    }

    static readonly HttpClient httpClient = new();
    public static async Task<bool> UploadLog()
    {
        try
        {
            if (_filePath == null)
            {
                throw new InvalidOperationException("File path not set.");
            }
            Flush();
            var fileBytes = await File.ReadAllBytesAsync(_filePath);
            var fileContent = new ByteArrayContent(fileBytes);
            // fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            using var form = new MultipartFormDataContent
            {
                { fileContent, "file", Path.GetFileName(_filePath) }
            };
            var response = await httpClient.PostAsync("https://eotw.briem.cc/upload", form);
            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> Shutdown()
    {
        lock (_lock)
        {
            GroupId = 0;
            _writer?.Flush();
            _writer?.Dispose();
            _initialized = false;
        }
        return await UploadLog();
    }
}
