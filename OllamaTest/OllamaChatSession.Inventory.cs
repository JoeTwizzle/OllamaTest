using OllamaSharp.Models;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Backend;

partial class OllamaChatSession
{
    readonly Dictionary<string, Dictionary<string, int>> _itemDict = [];
    bool _inventoryDirty = true;
    public void AddItem(string npc, string item)
    {
        ref var items = ref CollectionsMarshal.GetValueRefOrAddDefault(_itemDict, npc, out var exists);
        if (!exists)
        {
            items = [];
        }
        ref var itemStack = ref CollectionsMarshal.GetValueRefOrAddDefault(items!, item, out exists);
        if (!exists)
        {
            itemStack = 0;
        }
        itemStack++;
        _inventoryDirty = true;
    }

    public void RemoveItem(string npc, string item)
    {
        if (!_itemDict.TryGetValue(npc, out var items))
        {
            return;
        }
        ref var itemStack = ref CollectionsMarshal.GetValueRefOrNullRef(items, item);
        if (Unsafe.IsNullRef(ref itemStack))
        {
            return;
        }
        itemStack--;
        if (itemStack == 0)
        {
            items.Remove(item);
        }
        _inventoryDirty = true;
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
        string text;
        if (_itemDict.TryGetValue(npcName, out var items))
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("You have the following items in your inventory:");
            foreach (var item in items)
            {
                //E.g.: Mushroom (x10)
                sb.AppendLine($"{item.Key} (x{item.Value})");
            }
            text = sb.ToString();
        }
        else
        {
            text = "You have nothing in your inventory";
        }


        var request = new EmbedRequest() { Input = [text], Model = _embeddingModel };
        var embedding = await _ollama.EmbedAsync(request);

        return new Document(text, embedding.Embeddings);
    }
}
