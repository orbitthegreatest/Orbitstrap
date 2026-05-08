using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Orbitstrap.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Orbitstrap.Properties.Resources", typeof(Resources).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Bitmap CancelButton => (Bitmap)ResourceManager.GetObject("CancelButton", resourceCulture);

	internal static Bitmap CancelButtonHover => (Bitmap)ResourceManager.GetObject("CancelButtonHover", resourceCulture);

	internal static Bitmap DarkCancelButton => (Bitmap)ResourceManager.GetObject("DarkCancelButton", resourceCulture);

	internal static Bitmap DarkCancelButtonHover => (Bitmap)ResourceManager.GetObject("DarkCancelButtonHover", resourceCulture);

	internal static Icon IconOrbitstrap => (Icon)ResourceManager.GetObject("IconOrbitstrap", resourceCulture);

	internal static Icon Orbitstrapicon => (Icon)ResourceManager.GetObject("IconOrbitstrap", resourceCulture);

	internal static Icon Icon2008 => (Icon)ResourceManager.GetObject("Icon2008", resourceCulture);

	internal static Icon Icon2011 => (Icon)ResourceManager.GetObject("Icon2011", resourceCulture);

	internal static Icon IconEarly2015 => (Icon)ResourceManager.GetObject("IconEarly2015", resourceCulture);

	internal static Icon IconLate2015 => (Icon)ResourceManager.GetObject("IconLate2015", resourceCulture);

	internal static Icon Icon2017 => (Icon)ResourceManager.GetObject("Icon2017", resourceCulture);

	internal static Icon Icon2019 => (Icon)ResourceManager.GetObject("Icon2019", resourceCulture);

	internal static Icon Icon2022 => (Icon)ResourceManager.GetObject("Icon2022", resourceCulture);

	internal static Icon IconOrbitstrapClassic => (Icon)ResourceManager.GetObject("IconOrbitstrapClassic", resourceCulture);

	internal Resources()
	{
	}
}
