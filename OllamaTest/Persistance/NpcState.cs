using OllamaSharp.Models.Chat;
using System.Text.Json.Serialization;

namespace Backend.Persistance;
internal sealed partial class NpcState
{
    public readonly Dictionary<string, int> InventoryState = [];
    public readonly List<Message> MessageHistory = [];
    public readonly List<Document> RagDocuments = [];
    public string? CurrentQuest;
    public string? AvailableQuests;
    public string? CompletableTasks;
}
