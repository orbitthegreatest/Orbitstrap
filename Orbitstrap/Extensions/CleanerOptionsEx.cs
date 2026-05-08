using System.Collections.Generic;
using Orbitstrap.Enums;

namespace Orbitstrap.Extensions;

internal static class CleanerOptionsEx
{
	public static IReadOnlyCollection<CleanerOptions> Selections => new CleanerOptions[5]
	{
		CleanerOptions.Never,
		CleanerOptions.OneDay,
		CleanerOptions.OneWeek,
		CleanerOptions.OneMonth,
		CleanerOptions.TwoMonths
	};
}
