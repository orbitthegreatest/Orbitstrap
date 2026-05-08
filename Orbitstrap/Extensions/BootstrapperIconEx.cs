using System;
using System.Collections.Generic;
using System.Drawing;
using Orbitstrap.Enums;
using Orbitstrap.Properties;

namespace Orbitstrap.Extensions;

internal static class BootstrapperIconEx
{
	public static IReadOnlyCollection<BootstrapperIcon> Selections => new BootstrapperIcon[10]
	{
		BootstrapperIcon.IconOrbitstrap,
		BootstrapperIcon.Icon2022,
		BootstrapperIcon.Icon2019,
		BootstrapperIcon.Icon2017,
		BootstrapperIcon.IconLate2015,
		BootstrapperIcon.IconEarly2015,
		BootstrapperIcon.Icon2011,
		BootstrapperIcon.Icon2008,
		BootstrapperIcon.IconOrbitstrapClassic,
		BootstrapperIcon.IconCustom
	};

	public static Icon GetIcon(this BootstrapperIcon icon)
	{
		switch (icon)
		{
		case BootstrapperIcon.IconCustom:
		{
			Icon icon2 = null;
			string bootstrapperIconCustomLocation = App.Settings.Prop.BootstrapperIconCustomLocation;
			if (string.IsNullOrEmpty(bootstrapperIconCustomLocation))
			{
				App.Logger.WriteLine("BootstrapperIconEx::GetIcon", "Warning: custom icon is not set.");
			}
			else
			{
				try
				{
					icon2 = new Icon(bootstrapperIconCustomLocation);
				}
				catch (Exception ex)
				{
					App.Logger.WriteLine("BootstrapperIconEx::GetIcon", "Failed to load custom icon!");
					App.Logger.WriteException("BootstrapperIconEx::GetIcon", ex);
				}
			}
			return icon2 ?? Orbitstrap.Properties.Resources.IconOrbitstrap;
		}
		case BootstrapperIcon.IconOrbitstrap:
			return Orbitstrap.Properties.Resources.IconOrbitstrap;
		case BootstrapperIcon.Icon2008:
			return Orbitstrap.Properties.Resources.Icon2008;
		case BootstrapperIcon.Icon2011:
			return Orbitstrap.Properties.Resources.Icon2011;
		case BootstrapperIcon.IconEarly2015:
			return Orbitstrap.Properties.Resources.IconEarly2015;
		case BootstrapperIcon.IconLate2015:
			return Orbitstrap.Properties.Resources.IconLate2015;
		case BootstrapperIcon.Icon2017:
			return Orbitstrap.Properties.Resources.Icon2017;
		case BootstrapperIcon.Icon2019:
			return Orbitstrap.Properties.Resources.Icon2019;
		case BootstrapperIcon.Icon2022:
			return Orbitstrap.Properties.Resources.Icon2022;
		case BootstrapperIcon.IconOrbitstrapClassic:
			return Orbitstrap.Properties.Resources.IconOrbitstrapClassic;
		default:
			return Orbitstrap.Properties.Resources.IconOrbitstrap;
		}
	}
}
