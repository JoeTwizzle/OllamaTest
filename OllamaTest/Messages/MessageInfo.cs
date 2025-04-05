using LiteNetLib.Utils;

namespace Backend.Messages;

sealed class MessageInfo : INetSerializable
{
    public string Message { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    //Serialization only
    public MessageInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public MessageInfo(string message)
    {
        Message = message;
    }

    public void Deserialize(NetDataReader reader)
    {
        Message = reader.GetString();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Message);
    }
}
