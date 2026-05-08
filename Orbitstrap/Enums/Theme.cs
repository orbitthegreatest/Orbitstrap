using Orbitstrap.Models.Attributes;

namespace Orbitstrap.Enums;

public enum Theme
{
	[EnumName(FromTranslation = "Common.SystemDefault")]
	Default,
	Light,
	Dark
}
