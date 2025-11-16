using Backend.Messages;
using OllamaSharp.Models;
using System.Text;

namespace Backend;

partial class OllamaChatSession
{
    public void SetInventory(string npc, NPCInventoryChangeInfo info)
    {
        var state = GetNpcState(npc);
        state.InventoryState.Clear();
        LogEvent($"Inventory set to new state for: {npc}", ConsoleColor.Green);
        foreach (var item in info.ItemNames)
        {
            LogEvent(item, ConsoleColor.DarkGreen);
            state.InventoryState.Add(new Persistance.ItemInfo(item, ""));
        }
        for (int i = 0; i < info.LockedItemNames.Length; i++)
        {
            string? item = info.LockedItemNames[i];
            string? reason = info.LockedItemReasons[i];
            LogEvent(item, ConsoleColor.DarkGreen);
            state.InventoryState.Add(new Persistance.ItemInfo(item, reason));
        }
    }

    public void AddItem(string npc, string item)
    {
        var state = GetNpcState(npc);

        state.InventoryState.Add(new Persistance.ItemInfo(item, ""));
    }

    public void RemoveItem(string npc, string item)
    {
        var state = GetNpcState(npc);

        state.InventoryState.Remove(new Persistance.ItemInfo(item, ""));
        LogWarning($"{item} completely removed from {npc}");
    }

    public string GetInventoryString(string npcName)
    {
        string text;
        var state = GetNpcState(npcName);
        if (state.InventoryState.Count > 0)
        {
            StringBuilder sb = new();
            sb.AppendLine("You have the following items in your inventory:");
            foreach (var item in state.InventoryState)
            {
                if (!string.IsNullOrWhiteSpace(item.Condition))
                {
                    sb.AppendLine($"Item: {item.Name} Additional info: {item.Condition ?? ""}");
                }
                else
                {
                    sb.AppendLine($"Item: {item.Name}");
                }
            }
            text = sb.ToString();
        }
        else
        {
            text = "You have nothing in your inventory";
        }
        return text;
    }

    public async Task<Document?> GetEmbeddedInventoryAsync(string npcName)
    {
        if (_ollama == null)
        {
            LogError("Could not add document! Ollama not loaded.");
            return null;
        }

        if (_embeddingModel == null)
        {
            LogError("Could not add document! No embedding model specified.");
            return null;
        }

        var text = GetInventoryString(npcName);

        var request = new EmbedRequest() { Input = [text], Model = _embeddingModel };
        var embedding = await _ollama.EmbedAsync(request);

        return new Document(text, embedding.Embeddings);
    }
}
