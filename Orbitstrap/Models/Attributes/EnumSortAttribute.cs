using System;

namespace Orbitstrap.Models.Attributes;

internal class EnumSortAttribute : Attribute
{
	public int Order { get; set; }
}
