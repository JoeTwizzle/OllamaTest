using LiteNetLib.Utils;

namespace Backend.Messages;
public sealed class GeneratedDescriptionInfo : INetSerializable
{
    public string RawDescription { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public GeneratedDescriptionInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public GeneratedDescriptionInfo(string rawDescription)
    {
        RawDescription = rawDescription;
    }

    public void Deserialize(NetDataReader reader)
    {
        RawDescription = reader.GetString();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(RawDescription);
    }
}
