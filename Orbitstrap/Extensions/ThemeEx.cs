using Microsoft.Win32;
using Orbitstrap.Enums;

namespace Orbitstrap.Extensions;

public static class ThemeEx
{
	public static Theme GetFinal(this Theme dialogTheme)
	{
		if (dialogTheme != Theme.Default)
		{
			return dialogTheme;
		}
		using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
		object obj = registryKey?.GetValue("AppsUseLightTheme");
		if (obj is int && (int)obj == 0)
		{
			return Theme.Dark;
		}
		return Theme.Light;
	}
}
