using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Messages;
internal class SaveToFileInfo : INetSerializable
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public SaveToFileInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public SaveToFileInfo(string path)
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
