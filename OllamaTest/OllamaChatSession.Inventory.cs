using OllamaSharp.Models;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Backend;

partial class OllamaChatSession
{
    public void AddItem(string npc, string item)
    {
        var state = GetNpcState(npc);

        ref var itemStack = ref CollectionsMarshal.GetValueRefOrAddDefault(state.InventoryState, item, out var exists);
        if (!exists)
        {
            itemStack = 0;
        }
        itemStack++;
    }

    public void RemoveItem(string npc, string item)
    {
        var state = GetNpcState(npc);

        ref var itemStack = ref CollectionsMarshal.GetValueRefOrNullRef(state.InventoryState, item);
        if (Unsafe.IsNullRef(ref itemStack))
        {
            return;
        }
        itemStack--;
        if (itemStack == 0)
        {
            state.InventoryState.Remove(item);
        }
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
        var state = GetNpcState(npcName);
        if (state.InventoryState.Count > 0)
        {
            StringBuilder sb = new();
            sb.AppendLine("You have the following items in your inventory:");
            foreach (var item in state.InventoryState)
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
