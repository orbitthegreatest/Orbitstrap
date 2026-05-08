using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Orbitstrap.Resources;

namespace Orbitstrap;

internal static class Locale
{
	private static readonly List<string> _rtlLocales = new List<string> { "ar", "he", "fa" };

	public static readonly Dictionary<string, string> SupportedLocales = new Dictionary<string, string>
	{
		{
			"nil",
			Strings.Common_SystemDefault
		},
		{ "en", "English" },
		{ "en-US", "English (United States)" },
		{ "ar", "العربية" },
		{ "bg", "Български" },
		{ "bs", "Bosanski" },
		{ "cs", "Čeština" },
		{ "de", "Deutsch" },
		{ "da", "Dansk" },
		{ "es-ES", "Español" },
		{ "fa", "فارسی" },
		{ "fi", "Suomi" },
		{ "fil", "Filipino" },
		{ "fr", "Français" },
		{ "hr", "Hrvatski" },
		{ "hu", "Magyar" },
		{ "id", "Bahasa Indonesia" },
		{ "it", "Italiano" },
		{ "ja", "日本語" },
		{ "ko", "한국어" },
		{ "lv", "Latviešu" },
		{ "lt", "Lietuvių" },
		{ "ms", "Malay" },
		{ "nl", "Nederlands" },
		{ "pl", "Polski" },
		{ "pt-BR", "Português (Brasil)" },
		{ "ro", "Română" },
		{ "ru", "Русский" },
		{ "sv-SE", "Svenska" },
		{ "th", "ภาษาไทย" },
		{ "tr", "Türkçe" },
		{ "uk", "Українська" },
		{ "vi", "Tiếng Việt" },
		{ "zh-CN", "中文 (简体)" },
		{ "zh-HK", "中文 (香港)" },
		{ "zh-TW", "中文 (繁體)" }
	};

	public static CultureInfo CurrentCulture { get; private set; } = CultureInfo.InvariantCulture;

	public static bool RightToLeft { get; private set; } = false;

	public static string GetIdentifierFromName(string language)
	{
		return SupportedLocales.FirstOrDefault<KeyValuePair<string, string>>((KeyValuePair<string, string> x) => x.Value == language).Key ?? "nil";
	}

	public static List<string> GetLanguages()
	{
		List<string> languages = new List<string>();
		languages.AddRange(SupportedLocales.Values.Take(3));
		languages.AddRange(from x in SupportedLocales.Values
			where !languages.Contains(x)
			orderby x
			select x);
		languages[0] = Strings.Common_SystemDefault;
		return languages;
	}

	public static void Set(string identifier)
	{
		if (!SupportedLocales.ContainsKey(identifier))
		{
			identifier = "nil";
		}
		if (identifier == "nil")
		{
			CurrentCulture = Thread.CurrentThread.CurrentUICulture;
		}
		else
		{
			CurrentCulture = new CultureInfo(identifier);
			CultureInfo.DefaultThreadCurrentUICulture = CurrentCulture;
			Thread.CurrentThread.CurrentUICulture = CurrentCulture;
		}
		RightToLeft = _rtlLocales.Any(CurrentCulture.Name.StartsWith);
	}

	public static void Initialize()
	{
		Set("nil");
		EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, (RoutedEventHandler)delegate(object sender, RoutedEventArgs _)
		{
			Window window = (Window)sender;
			if (RightToLeft)
			{
				window.FlowDirection = FlowDirection.RightToLeft;
				if (window.ContextMenu != null)
				{
					window.ContextMenu.FlowDirection = FlowDirection.RightToLeft;
				}
			}
			else if (CurrentCulture.Name.StartsWith("th"))
			{
				window.FontFamily = new System.Windows.Media.FontFamily(new Uri("pack://application:,,,/Resources/Fonts/"), "./#Noto Sans Thai");
			}
		});
	}
}
