using Fika.Core.Networking;
using LiteNetLib.Utils;
using UnityEngine;

namespace ScavBirthdayParty
{
    public class BirthdayPartyPacket : INetSerializable
    {
        public Vector3 position;

        public void Deserialize(NetDataReader reader)
        {
            position = reader.GetVector3();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(position);
        }
    }
}
