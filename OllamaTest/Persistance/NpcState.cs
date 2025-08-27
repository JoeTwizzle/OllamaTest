using OllamaSharp.Models.Chat;
using System.Text.Json.Serialization;

namespace Backend.Persistance;
internal sealed class NpcState
{
    public readonly Dictionary<string, int> InventoryState = [];
    public readonly List<Message> MessageHistory = [];
    public readonly List<Document> RagDocuments = [];
    public string? CurrentQuest;
    public string? AvailableQuests;
    public string? CompletableTasks;

    public NpcState()
    {
    }

    public NpcState(Dictionary<string, int> inventoryState, List<Message> messageHistory, List<Document> ragDocuments, string? currentQuest, string? availableQuests, string? completableTasks)
    {
        InventoryState = inventoryState;
        MessageHistory = messageHistory;
        RagDocuments = ragDocuments;
        CurrentQuest = currentQuest;
        AvailableQuests = availableQuests;
        CompletableTasks = completableTasks;
    }
}
