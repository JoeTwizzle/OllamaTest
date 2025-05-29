using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace Backend.Messages;

public struct NpcBiomeInfo : INetSerializable
{
    public string NpcName;
    public string BiomeName;
    public string? AdditionalInfo;

    public NpcBiomeInfo(string npcName, string biomeName, string? additionalInfo)
    {
        NpcName = npcName;
        BiomeName = biomeName;
        AdditionalInfo = additionalInfo;
    }

    public void Deserialize(NetDataReader reader)
    {
        NpcName = reader.GetString();
        BiomeName = reader.GetString();
        if (reader.GetBool())
        {
            AdditionalInfo = reader.GetString();
        }
    }

    public readonly void Serialize(NetDataWriter writer)
    {
        writer.Put(NpcName);
        writer.Put(BiomeName);
        var hasInfo = AdditionalInfo != null;
        writer.Put(hasInfo);
        if (hasInfo)
        {
            writer.Put(AdditionalInfo);
        }
    }
}

public sealed class WorldInfo : INetSerializable
{
    public NpcBiomeInfo[] NpcBiomeInfos;
    public string[] BiomesPresent;
    public string[] PointsOfInterest;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public WorldInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public WorldInfo(NpcBiomeInfo[] npcBiomeInfos, string[] biomesPresent, string[] pointsOfInterest)
    {
        NpcBiomeInfos = npcBiomeInfos;
        BiomesPresent = biomesPresent;
        PointsOfInterest = pointsOfInterest;
    }

    public void Deserialize(NetDataReader reader)
    {
        NpcBiomeInfos = reader.GetArray<NpcBiomeInfo>();
        BiomesPresent = reader.GetStringArray();
        PointsOfInterest = reader.GetStringArray();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.PutArray(NpcBiomeInfos);
        writer.PutArray(BiomesPresent);
        writer.PutArray(PointsOfInterest);
    }
}
#nullable restore
