using LiteNetLib.Utils;

namespace Backend.Messages;

public sealed class AnswerTokenInfo : INetSerializable
{
    public bool IsStreamed { get; set; }
    public string Sender { get; set; }
    public string Token { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    //Serialization only
    public AnswerTokenInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public AnswerTokenInfo(bool isStreamed, string sender, string token)
    {
        IsStreamed = isStreamed;
        Sender = sender;
        Token = token;
    }


    public void Deserialize(NetDataReader reader)
    {
        IsStreamed = reader.GetBool();
        Sender = reader.GetString();
        Token = reader.GetString();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(IsStreamed);
        writer.Put(Sender);
        writer.Put(Token);
    }
}
