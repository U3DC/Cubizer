﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Cubizer.Net.Protocol.Extensions;
using Cubizer.Net.Protocol.Serialization;

namespace Cubizer.Net.Protocol
{
	public sealed class UncompressedPacket
	{
		public uint packetId;
		public ArraySegment<byte> data;

		public UncompressedPacket()
		{
			this.packetId = 0;
		}

		public UncompressedPacket(uint id, ArraySegment<byte> data)
		{
			this.packetId = id;
			this.data = data;
		}

		public void Serialize(Stream stream)
		{
			var length = (uint)data.Count + packetId.SizeofBytes();

			using (var bw = new NetworkWrite(stream, Encoding.UTF8, true))
			{
				bw.WriteVarInt(length);
				bw.WriteVarInt(packetId);
				bw.Flush();
			}

			stream.Write(data.Array, data.Offset, data.Count);
		}

		public async void SerializeAsync(Stream stream)
		{
			var length = (uint)data.Count + packetId.SizeofBytes();

			using (var bw = new NetworkWrite(stream, Encoding.UTF8, true))
			{
				bw.WriteVarInt(length);
				bw.WriteVarInt(packetId);
				bw.Flush();
			}

			await stream.WriteAsync(data.Array, data.Offset, data.Count);
		}

		public int Deserialize(Stream stream, int maxLength = ushort.MaxValue)
		{
			using (var br = new NetworkReader(stream, Encoding.UTF8, true))
			{
				uint length;
				br.ReadVarInt(out length);

				if (length > 0 && length < maxLength)
				{
					byte packLength = br.ReadVarInt(out packetId);

					data = new ArraySegment<byte>(new byte[length - packLength]);
					return stream.Read(data.Array, data.Offset, data.Count);
				}

				return (int)length;
			}
		}

		public async Task<int> DeserializeAsync(Stream stream, int maxLength = ushort.MaxValue)
		{
			using (var br = new NetworkReader(stream, Encoding.UTF8, true))
			{
				uint length;
				br.ReadVarInt(out length);

				if (length > 0 && length < maxLength)
				{
					byte packLength = br.ReadVarInt(out packetId);

					data = new ArraySegment<byte>(new byte[length - packLength]);
					return await stream.ReadAsync(data.Array, data.Offset, data.Count);
				}

				return (int)length;
			}
		}
	}
}