using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Messages
{
    internal sealed class PlayerAnimalInfo : INetSerializable
    {
        public string AnimalName {  get; set; }
        public string AnimalSpecies {  get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public PlayerAnimalInfo() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public PlayerAnimalInfo(string animalName, string animalSpecies)
        {
            AnimalName = animalName;
            AnimalSpecies = animalSpecies;
        }

        public void Deserialize(NetDataReader reader)
        {
            AnimalName = reader.GetString();
            AnimalSpecies = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(AnimalName);
            writer.Put(AnimalSpecies);
        }
    }
}
