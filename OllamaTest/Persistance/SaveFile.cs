using OllamaSharp.Models.Chat;
using System.Text.Json.Serialization;

namespace Backend.Persistance;

[JsonSerializable(typeof(SaveFile))]
internal sealed partial class SaveFile
{
    public Dictionary<string, List<Message>> MessageHistory { get; set; }
    public Dictionary<string, List<Document>> Documents { get; set; }

}
