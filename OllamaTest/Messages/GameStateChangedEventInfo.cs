using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Messages
{
    internal class GameStateChangedEventInfo : INetSerializable
    {
        public GameStateChangedEventInfo() { }

        public GameStateChangedEventInfo(string eventName)
        {
            EventName = eventName;
        }

        public string EventName { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            EventName = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(EventName);
        }
    }
}
