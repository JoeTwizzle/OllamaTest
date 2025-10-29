using LiteNetLib.Utils;

namespace Backend.Messages
{
    sealed class ResultInfo : INetSerializable
    {
        public string Message { get; set; }
        public bool Success { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ResultInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public ResultInfo(string message, bool success)
        {
            Message = message;
            Success = success;
        }

        public void Deserialize(NetDataReader reader)
        {
            Message = reader.GetString();
            Success = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Message);
            writer.Put(Success);
        }
    }
}