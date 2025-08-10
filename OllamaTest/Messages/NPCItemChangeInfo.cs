using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Messages;
sealed class NPCItemChangeInfo : INetSerializable
{
    public string NPCName;
    public string ItemName;
    public bool Added;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public NPCItemChangeInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public NPCItemChangeInfo(string nPCName, string itemName, bool added)
    {
        NPCName = nPCName;
        ItemName = itemName;
        Added = added;
    }

    public void Deserialize(NetDataReader reader)
    {
        NPCName = reader.GetString();
        ItemName = reader.GetString();
        Added = reader.GetBool();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(NPCName);
        writer.Put(ItemName);
        writer.Put(Added);
    }
}
