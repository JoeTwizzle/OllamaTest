using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Messages
{
    sealed class QuestEventMessageInfo : INetSerializable
    {
        public QuestEventMessageInfo() { }

        public QuestEventMessageInfo(bool completed, string questName)
        {
            Completed = completed;
            QuestName = questName;
        }

        //True if completed
        public bool Completed { get; set; }
        public string QuestName { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            Completed = reader.GetBool();
            QuestName = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Completed);
            writer.Put(QuestName);
        }
    }
}
