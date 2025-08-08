using LiteNetLib.Utils;

namespace Backend.Messages;
internal class LoadFromFileInfo : INetSerializable
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public LoadFromFileInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public LoadFromFileInfo(string path)
    {
        Path = path;
    }

    public string Path { get; set; }

    public void Deserialize(NetDataReader reader)
    {
        Path = reader.GetString();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Path);
    }
}