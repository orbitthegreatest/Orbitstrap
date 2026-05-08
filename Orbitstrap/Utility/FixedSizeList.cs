using System.Collections.Generic;

namespace Orbitstrap.Utility;

internal class FixedSizeList<T> : List<T>
{
	public int MaxSize { get; }

	public FixedSizeList(int size)
	{
		MaxSize = size;
	}

	public new void Add(T item)
	{
		if (base.Count >= MaxSize)
		{
			RemoveAt(base.Count - 1);
		}
		base.Add(item);
	}
}
