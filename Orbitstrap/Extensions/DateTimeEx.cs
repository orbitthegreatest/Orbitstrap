using System;
using System.Globalization;

namespace Orbitstrap.Extensions;

internal static class DateTimeEx
{
	public static string ToFriendlyString(this DateTime dateTime)
	{
		return dateTime.ToString("dddd, d MMMM yyyy 'at' h:mm:ss tt", CultureInfo.InvariantCulture);
	}
}
