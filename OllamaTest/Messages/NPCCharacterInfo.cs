using LiteNetLib.Utils;

namespace Backend.Messages;

sealed class NPCCharacterInfo : INetSerializable
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    //For deserializing ONLY
    public NPCCharacterInfo() { }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public NPCCharacterInfo(string prompt, string[] availableTools, string[] warmUpDialogue)
    {
        Prompt = prompt;
        AvailableTools = availableTools;
        WarmUpDialogue = warmUpDialogue;
    }



    public string Prompt { get; set; }
    public string[] AvailableTools { get; set; }

    public string[] WarmUpDialogue { get; set; }

    public void Deserialize(NetDataReader reader)
    {
        Prompt = reader.GetString();
        AvailableTools = reader.GetStringArray();
        reader.GetStringArray();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Prompt);
        writer.PutArray(AvailableTools);
        writer.PutArray(WarmUpDialogue);
    }
}
