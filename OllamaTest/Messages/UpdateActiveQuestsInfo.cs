using LiteNetLib.Utils;
#nullable enable
namespace Backend.Messages;


internal class UpdateActiveQuestsInfo : INetSerializable
{
    public string Name { get; set; }
    public string Quest { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public UpdateActiveQuestsInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public UpdateActiveQuestsInfo(string name, string quest)
    {
        Name = name;
        Quest = quest;
    }

    public void Deserialize(NetDataReader reader)
    {
        Name = reader.GetString();
        Quest = reader.GetString();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Name);
        writer.Put(Quest);
    }
}
#nullable restore