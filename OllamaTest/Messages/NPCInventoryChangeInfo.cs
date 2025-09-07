using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Messages;

sealed class NPCInventoryChangeInfo : INetSerializable
{
    public string NPCName;
    public string[] ItemNames;
    public string[] LockedItemNames;
    public string[] LockedItemReasons;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public NPCInventoryChangeInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public NPCInventoryChangeInfo(string nPCName, string[] itemNames, string[] lockedItemNames, string[] lockedItemReasons)
    {
        NPCName = nPCName;
        ItemNames = itemNames;
        LockedItemNames = lockedItemNames;
        LockedItemReasons = lockedItemReasons;
    }

    public void Deserialize(NetDataReader reader)
    {
        NPCName = reader.GetString();
        ItemNames = reader.GetStringArray();
        LockedItemNames = reader.GetStringArray();
        LockedItemReasons = reader.GetStringArray();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(NPCName);
        writer.PutArray(ItemNames);
        writer.PutArray(LockedItemNames);
        writer.PutArray(LockedItemReasons);
    }
}