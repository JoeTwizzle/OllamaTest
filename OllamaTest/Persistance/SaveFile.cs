using Backend.Messages;
using OllamaSharp.Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Backend.Persistance;

[JsonSerializable(typeof(SaveFile))]
internal sealed partial class SaveFile
{
    public Dictionary<NPCCharacterInfo, List<Message>> MessageHistory { get; set; }
    public List<Document> Documents { get; set; }

}
