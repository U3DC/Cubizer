﻿using System.Collections;

namespace Cubizer
{
	public sealed class VoxelDataNodeEnum<_Tx, _Ty> : IEnumerator
		where _Tx : struct
		where _Ty : class
	{
		private int position = -1;
		private readonly VoxelDataNode<_Tx, _Ty>[] _array;

		public VoxelDataNodeEnum(VoxelDataNode<_Tx, _Ty>[] list)
		{
			_array = list;
		}

		public bool MoveNext()
		{
			var length = _array.Length;
			for (position++; position < length; position++)
			{
				if (_array[position] == null)
					continue;
				if (_array[position].is_empty())
					continue;
				break;
			}

			return position < _array.Length;
		}

		public void Reset()
		{
			position = -1;
		}

		object IEnumerator.Current
		{
			get
			{
				return Current;
			}
		}

		public VoxelDataNode<_Tx, _Ty> Current
		{
			get
			{
				return _array[position];
			}
		}
	}
}