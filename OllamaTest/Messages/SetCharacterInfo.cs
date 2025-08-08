using LiteNetLib.Utils;

namespace Backend.Messages
{
    class SetCharacterInfo : INetSerializable, IEquatable<SetCharacterInfo>
    {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public SetCharacterInfo()
        {
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public SetCharacterInfo(bool forceReload, NPCCharacterInfo nPCCharacterInfo)
        {
            ForceReload = forceReload;
            NPCCharacterInfo = nPCCharacterInfo;
        }

        public bool ForceReload { get; set; }
        public NPCCharacterInfo NPCCharacterInfo { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            ForceReload = reader.GetBool();
            reader.Get<NPCCharacterInfo>(out var info, () => new());
            NPCCharacterInfo = info;
        }

        public bool Equals(SetCharacterInfo? other)
        {
            return other != null && ForceReload == other.ForceReload && NPCCharacterInfo == other.NPCCharacterInfo;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ForceReload);
            writer.Put(NPCCharacterInfo);
        }

        public override string? ToString()
        {
            return base.ToString();
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as SetCharacterInfo);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ForceReload, NPCCharacterInfo.GetHashCode());
        }
    }
}
