using System;
using System.Collections.Generic;
using System.Linq;

namespace Orbitstrap.Utility;

internal static class Time
{
	public static string FormatTimeSpan(TimeSpan timeSpan)
	{
		Func<Tuple<int, string>, string> func = (Tuple<int, string> t) => $"{t.Item1} {t.Item2}{((t.Item1 == 1) ? string.Empty : "s")}";
		List<Tuple<int, string>> list = new List<Tuple<int, string>>
		{
			Tuple.Create((int)timeSpan.TotalDays, "day"),
			Tuple.Create(timeSpan.Hours, "hour"),
			Tuple.Create(timeSpan.Minutes, "minute")
		};
		list.RemoveAll((Tuple<int, string> i) => i.Item1 == 0);
		string text = "";
		if (list.Count > 1)
		{
			Tuple<int, string> arg = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			text = " and " + func(arg);
		}
		return string.Join(", ", list.Select(func)) + text;
	}
}
