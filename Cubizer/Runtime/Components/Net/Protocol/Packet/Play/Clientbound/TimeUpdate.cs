﻿using Cubizer.Net.Protocol.Serialization;

namespace Cubizer.Net.Protocol.Play.Clientbound
{
	[Packet(Packet)]
	public class TimeUpdate : IPacketSerializable
	{
		public const int Packet = 0x46;

		public long worldAge;
		public long timeOfDay;

		public uint packetId
		{
			get
			{
				return Packet;
			}
		}

		public object Clone()
		{
			return new TimeUpdate();
		}

		public void Deserialize(NetworkReader br)
		{
			br.Read(out worldAge);
			br.Read(out timeOfDay);
		}

		public void Serialize(NetworkWrite bw)
		{
			bw.Write(worldAge);
			bw.Write(timeOfDay);
		}
	}
}