﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using Cubizer.Net.Protocol.Extensions;

namespace Cubizer.Net.Protocol.Serialization
{
	public class NetworkReader : IDisposable
	{
		private BinaryReader reader;

		public NetworkReader(Stream stream)
		{
			reader = new BinaryReader(stream);
		}

		public NetworkReader(Stream stream, Encoding encoding)
		{
			reader = new BinaryReader(stream, encoding);
		}

		public NetworkReader(Stream stream, Encoding encoding, bool leaveOpen)
		{
			reader = new BinaryReader(stream, encoding, leaveOpen);
		}

		public NetworkReader(ArraySegment<byte> segment)
		{
			reader = new BinaryReader(new MemoryStream(segment.Array, segment.Offset, segment.Count));
		}

		public void Read(out bool value) => value = reader.ReadBoolean();
		public void Read(out byte value) => value = reader.ReadByte();
		public void Read(out sbyte value) => value = reader.ReadSByte();
		public void Read(out short value) => value = reader.ReadInt16().ToBigEndian();
		public void Read(out int value) => value = reader.ReadInt32().ToBigEndian();
		public void Read(out long value) => value = reader.ReadInt64().ToBigEndian();
		public void Read(out ushort value) => value = reader.ReadUInt16().ToBigEndian();
		public void Read(out uint value) => value = reader.ReadUInt32().ToBigEndian();
		public void Read(out ulong value) => value = reader.ReadUInt64().ToBigEndian();
		public void Read(out string value) => value = reader.ReadString();
		public void Read(out float value) => value = BitConverter.ToSingle(BitConverter.GetBytes(reader.ReadUInt32().ToBigEndian()), 0);
		public void Read(out double value) => value = BitConverter.Int64BitsToDouble(reader.ReadInt64().ToBigEndian());
		public void Read(out byte[] value, int count) => value = reader.ReadBytes(count);

		public void Read(out Guid value)
		{
			if (!Guid.TryParse(reader.ReadString(), out value))
				throw new InvalidDataException("Invalid Guid");
		}

		public byte ReadVarInt(out uint varint)
		{
			byte numRead = 0;
			int result = 0;
			byte read;
			do
			{
				read = reader.ReadByte();
				int value = (read & 0x7F);
				result |= (value << (7 * numRead));

				numRead++;
				if (numRead > 5)
				{
					throw new InvalidDataException("VarInt is too big");
				}
			} while ((read & 0x80) != 0);

			varint = (uint)result;

			return numRead;
		}

		public byte ReadVarLong(out ulong varlong)
		{
			byte numRead = 0;
			ulong result = 0;
			uint read;
			do
			{
				read = reader.ReadByte();
				uint value = (read & 0x7F);
				result |= (value << (7 * numRead));

				numRead++;
				if (numRead > 10)
				{
					throw new InvalidDataException("VarLong is too big");
				}
			} while ((read & 0x80) != 0);

			varlong = result;

			return numRead;
		}

		public void ReadVarString(out string value, int maxLength = short.MaxValue)
		{
			uint length;
			this.ReadVarInt(out length);

			if (length <= maxLength)
			{
				var bytes = reader.ReadBytes((int)length);
				value = Encoding.UTF8.GetString(bytes);
			}
			else
			{
				throw new InvalidDataException($"String is too big .length:{length}.");
			}
		}

		public void ReadVarBytes(out byte[] value, int maxLength = short.MaxValue)
		{
			uint length;
			this.ReadVarInt(out length);

			if (length <= maxLength)
			{
				value = reader.ReadBytes((int)length);
			}
			else
			{
				throw new InvalidDataException($"Bytes is too big .length:{length}.");
			}
		}

		public void Read<T>(IReadOnlyList<T> array)
		where T : IPacketSerializable
		{
			foreach (var it in array)
				it.Deserialize(this);
		}

		public void Dispose()
		{
			reader.Close();
		}
	}
}