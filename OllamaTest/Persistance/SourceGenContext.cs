using System.Text.Json.Serialization;

namespace Backend.Persistance;

[JsonSerializable(typeof(SaveFile))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class SourceGenContext : JsonSerializerContext;

