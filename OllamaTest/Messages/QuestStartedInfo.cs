using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace Backend.Messages;

internal class QuestStartedInfo : INetSerializable
{
    public string QuestId { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public QuestStartedInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public QuestStartedInfo(string questId)
    {
        QuestId = questId;
    }

    public void Deserialize(NetDataReader reader)
    {
        QuestId = reader.GetString();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(QuestId);
    }
}
#nullable restore