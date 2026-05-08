using System;
using System.Collections.Generic;

namespace Orbitstrap;

public static class GlobalCache
{
	public static readonly Dictionary<string, string?> ServerLocation = new Dictionary<string, string>();

	public static readonly Dictionary<string, DateTime?> ServerTime = new Dictionary<string, DateTime?>();
}
