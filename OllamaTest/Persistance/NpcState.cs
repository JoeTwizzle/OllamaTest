using OllamaSharp.Models.Chat;

namespace Backend.Persistance;
public partial struct ItemInfo
{
    public string Name;
    public string Condition;

    public ItemInfo(string name, string condition)
    {
        Name = name;
        Condition = condition;
    }
}
internal sealed class NpcState
{
    public readonly List<ItemInfo> InventoryState = [];
    public readonly List<Message> MessageHistory = [];
    public readonly List<Document> RagDocuments = [];
    public string? CurrentQuest;
    public string? AvailableQuests;
    public string? CompletableTasks;

    public NpcState()
    {
    }

    public NpcState(List<ItemInfo> inventoryState, List<Message> messageHistory, List<Document> ragDocuments, string? currentQuest, string? availableQuests, string? completableTasks)
    {
        InventoryState = inventoryState;
        MessageHistory = messageHistory;
        RagDocuments = ragDocuments;
        CurrentQuest = currentQuest;
        AvailableQuests = availableQuests;
        CompletableTasks = completableTasks;
    }
}
