using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace Backend.Messages;

internal class UpdateQuestsInfo : INetSerializable
{
    //The NPC Name
    public string Name { get; set; }
    //The Quests Ids
    public string Quests { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public UpdateQuestsInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public UpdateQuestsInfo(string name, string quests)
    {
        Name = name;
        Quests = quests;
    }

    public void Deserialize(NetDataReader reader)
    {
        Name = reader.GetString();
        Quests = reader.GetString();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Name);
        writer.Put(Quests);
    }
}
#nullable restore