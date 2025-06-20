using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace Backend.Messages;

internal class TaskCompletedInfo : INetSerializable
{
    public string TaskId { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TaskCompletedInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public TaskCompletedInfo(string taskId)
    {
        TaskId = taskId;
    }

    public void Deserialize(NetDataReader reader)
    {
        TaskId = reader.GetString();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(TaskId);
    }
}
#nullable restore