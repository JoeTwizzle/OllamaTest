using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace Backend.Messages;

internal class GeneratedResponseInfo : INetSerializable
{
    public string GeneratedDescription { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public GeneratedResponseInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public GeneratedResponseInfo(string rawDescription)
    {
        GeneratedDescription = rawDescription;
    }

    public void Deserialize(NetDataReader reader)
    {
        GeneratedDescription = reader.GetString();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(GeneratedDescription);
    }
}
#nullable restore