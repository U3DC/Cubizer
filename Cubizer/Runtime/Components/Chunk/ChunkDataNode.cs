﻿using System;

namespace Cubizer.Chunk
{
	[Serializable]
	public sealed class ChunkDataNode<_Tx, _Ty>
		where _Tx : struct
		where _Ty : class
	{
		public readonly _Tx position;
		public _Ty value;

		public ChunkDataNode()
		{
			value = null;
		}

		public ChunkDataNode(_Tx x, _Ty value)
		{
			position = x;
			this.value = value;
		}

		public bool is_empty()
		{
			return value == null;
		}
	}
}