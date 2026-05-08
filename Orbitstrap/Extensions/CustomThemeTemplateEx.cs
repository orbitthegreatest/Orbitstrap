using System.Text;
using Orbitstrap.Enums;
using Orbitstrap.Resources;

namespace Orbitstrap.Extensions;

internal static class CustomThemeTemplateEx
{
	private const string EXAMPLES_URL = "https://github.com/Orbitstrap/custom-bootstrapper-examples";

	public static string GetFileName(this CustomThemeTemplate template)
	{
		return $"CustomBootstrapperTemplate_{template}.xml";
	}

	public static string GetFileContents(this CustomThemeTemplate template)
	{
		string text = Encoding.UTF8.GetString(Resource.Get(template.GetFileName()).Result);
		switch (template)
		{
		case CustomThemeTemplate.Blank:
		{
			string newValue2 = string.Format(Strings.CustomTheme_Templates_Blank_MoreExamples, "https://github.com/Orbitstrap/custom-bootstrapper-examples");
			return text.Replace("{0}", Strings.CustomTheme_Templates_Blank_UIElements).Replace("{1}", newValue2);
		}
		case CustomThemeTemplate.Simple:
		{
			string newValue = string.Format(Strings.CustomTheme_Templates_Simple_MoreExamples, "https://github.com/Orbitstrap/custom-bootstrapper-examples");
			return text.Replace("{0}", newValue);
		}
		default:
			return text;
		}
	}
}
