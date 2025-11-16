using OllamaSharp.Models.Chat;
using System.Text.Json.Serialization;

namespace Backend.Persistance;

[JsonSerializable(typeof(SaveFile))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class SourceGenContext : JsonSerializerContext;

[JsonSerializable(typeof(ChatRequest))]
[JsonSerializable(typeof(ChatResponseStream))]
[JsonSerializable(typeof(ChatDoneResponseStream))]
[JsonSerializable(typeof(OllamaSharp.Models.ListModelsResponse))]
[JsonSerializable(typeof(OllamaSharp.Models.EmbedRequest))]
[JsonSerializable(typeof(OllamaSharp.Models.EmbedResponse))]

[JsonSerializable(typeof(GetMyHomeTool))]
[JsonSerializable(typeof(GetQuestsForPlayerTool))]
[JsonSerializable(typeof(StartPlayerQuestTool))]
[JsonSerializable(typeof(GetCurrentPlayerActiveQuestTool))]
[JsonSerializable(typeof(GetCompletableJobsTool))]
[JsonSerializable(typeof(MarkJobAsCompleteTool))]
[JsonSerializable(typeof(GiveItemToPlayerTool))]
[JsonSerializable(typeof(GetItemsTool))]
// Add any other types that might be serialized in your messages
public partial class MyCustomJsonContext : JsonSerializerContext;
