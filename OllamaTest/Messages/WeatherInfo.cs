using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Messages
{
    internal sealed class WeatherInfo : INetSerializable
    {
        public string Weather {  get; set; }

        public WeatherInfo(string weather)
        {
            Weather = weather;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public WeatherInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public void Deserialize(NetDataReader reader)
        {
            Weather = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Weather);
        }
    }
}
