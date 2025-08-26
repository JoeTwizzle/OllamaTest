using LiteNetLib.Utils;

namespace Backend.Messages;

sealed class NPCCharacterInfo : INetSerializable, IEquatable<NPCCharacterInfo>
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    //For deserializing ONLY
    public NPCCharacterInfo() { }

    public NPCCharacterInfo(string name, string prompt, string[] availableTools, string[] memories, string[] warmUpDialogue)
    {
        Name = name;
        Prompt = prompt;
        AvailableTools = availableTools;
        Memories = memories;
        WarmUpDialogue = warmUpDialogue;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public string Name { get; set; }
    public string Prompt { get; set; }
    public string[] AvailableTools { get; set; }
    public string[] Memories { get; set; }

    public string[] WarmUpDialogue { get; set; }

    public void Deserialize(NetDataReader reader)
    {
        Name = reader.GetString();
        Prompt = reader.GetString();
        AvailableTools = reader.GetStringArray();
        Memories = reader.GetStringArray();
        WarmUpDialogue = reader.GetStringArray();
    }

    public bool Equals(NPCCharacterInfo? other)
    {
        return Name == other?.Name;
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Name);
        writer.Put(Prompt);
        writer.PutArray(AvailableTools);
        writer.PutArray(Memories);
        writer.PutArray(WarmUpDialogue);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as NPCCharacterInfo);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override string? ToString()
    {
        return Name + Environment.NewLine + Prompt + Environment.NewLine;
    }
}
