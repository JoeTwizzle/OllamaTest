using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Messages
{
    internal sealed class TimeOfDayInfo : INetSerializable
    {
        public string TimeOfDay {  get; set; }

        public TimeOfDayInfo(string timeOfDay)
        {
            TimeOfDay = timeOfDay;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public TimeOfDayInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public void Deserialize(NetDataReader reader)
        {
            TimeOfDay = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(TimeOfDay);
        }
    }
}
