using LiteNetLib.Utils;
#nullable enable
namespace Backend.Messages;

internal class UpdateTasksInfo : INetSerializable
{
    public string Name { get; set; }
    public string Quests { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public UpdateTasksInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public UpdateTasksInfo(string name, string quests)
    {
        Name = name;
        Quests = quests;
    }



    public void Deserialize(NetDataReader reader)
    {
        Name = reader.GetString();
        Quests = reader.GetString();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Name);
        writer.Put(Quests);
    }
}
#nullable restore