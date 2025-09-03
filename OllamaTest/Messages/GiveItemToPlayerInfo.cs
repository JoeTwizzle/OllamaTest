using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Messages;
internal sealed class GiveItemToPlayerInfo : INetSerializable
{
    public string Item;
    public string Npc;
    public string Dummy;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public GiveItemToPlayerInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public GiveItemToPlayerInfo(string item, string npc)
    {
        Item = item;
        Npc = npc;
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Item);
        writer.Put(Npc);
        writer.Put("NOPE");
    }

    public void Deserialize(NetDataReader reader)
    {
        Item = reader.GetString();
        Npc = reader.GetString();
        _ = reader.GetString();
    }
}
