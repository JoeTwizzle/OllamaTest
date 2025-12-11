using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Messages
{
    internal class ItemCollectOrDropEventInfo : INetSerializable
    {
        public ItemCollectOrDropEventInfo() { }

        public ItemCollectOrDropEventInfo(string itemName, bool itemCollected)
        {
            ItemName = itemName;
            ItemCollected = itemCollected;
        }

        public string ItemName { get; set; }
        public bool ItemCollected { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            ItemName = reader.GetString();
            ItemCollected = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ItemName);
            writer.Put(ItemCollected);
        }
    }
}
