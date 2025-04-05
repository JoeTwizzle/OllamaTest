using LiteNetLib.Utils;

namespace Backend.Messages;

sealed class AnswerTokenInfo : INetSerializable
{
    public string Token { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    //Serialization only
    public AnswerTokenInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public AnswerTokenInfo(string token)
    {
        Token = token;
    }

    public void Deserialize(NetDataReader reader)
    {
        Token = reader.GetString();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Token);
    }
}
