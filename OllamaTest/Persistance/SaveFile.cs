using OllamaSharp.Models.Chat;
using System.Text.Json.Serialization;

namespace Backend.Persistance;

[JsonSerializable(typeof(NpcSaveInfo))]
sealed partial class NpcSaveInfo
{
    public Dictionary<string, int> InventoryState { get; set; }
    public List<Message> MessageHistory { get; set; }
    public List<Document> RagDocuments { get; set; }
    public string? CurrentQuest { get; set; }
    public string? AvailableQuests { get; set; }
    public string? CompletableTasks { get; set; }
    public NpcSaveInfo(NpcState npcState)
    {
        InventoryState = npcState.InventoryState;
        MessageHistory = npcState.MessageHistory;
        RagDocuments = npcState.RagDocuments;
        CurrentQuest = npcState.CurrentQuest;
        AvailableQuests = npcState.AvailableQuests;
        CompletableTasks = npcState.CompletableTasks;
    }
}

[JsonSerializable(typeof(SaveFile))]
internal sealed partial class SaveFile
{
    public Dictionary<string, NpcSaveInfo> NpcInfo { get; set; }

    public SaveFile(Dictionary<string, NpcState> npcStates)
    {
        NpcInfo = npcStates.ToDictionary(
            kvp => kvp.Key,
            kvp => new NpcSaveInfo(kvp.Value)
        );
    }
}
