using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Messages;

public class QuestInfo : INetSerializable
{
    public string Id;

    public string Description;

    public QuestInfo[] Subquests;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public QuestInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public QuestInfo(string id, string description, QuestInfo[] subquests)
    {
        Id = id;
        Description = description;
        Subquests = subquests;
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetString();
        Description = reader.GetString();
        Subquests = reader.GetArray<QuestInfo>();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.Put(Description);
        writer.PutArray(Subquests);
    }
}
