using LiteNetLib.Utils;

namespace Backend.Messages;

sealed class CharacterInfo : INetSerializable
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    //For deserializing ONLY
    public CharacterInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public CharacterInfo(string prompt, params string[] availableTools)
    {
        Prompt = prompt;
        AvailableTools = availableTools;
    }

    public string Prompt { get; set; }
    public string[] AvailableTools { get; set; }

    public void Deserialize(NetDataReader reader)
    {
        Prompt = reader.GetString();
        AvailableTools = reader.GetStringArray();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Prompt);
        writer.PutArray(AvailableTools);
    }
}
