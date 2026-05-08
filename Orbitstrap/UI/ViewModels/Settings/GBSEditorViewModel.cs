using System.Collections.Generic;
using System.Linq;
using Orbitstrap.Enums.GBSPresets;

namespace Orbitstrap.UI.ViewModels.Settings;

public class GBSEditorViewModel : NotifyPropertyChangedViewModel
{
	public bool ReadOnly
	{
		get
		{
			return App.GlobalSettings.GetReadOnly();
		}
		set
		{
			App.GlobalSettings.SetReadOnly(value);
		}
	}

	public string FramerateCap
	{
		get
		{
			return App.GlobalSettings.GetPreset("Rendering.FramerateCap");
		}
		set
		{
			App.GlobalSettings.SetPreset("Rendering.FramerateCap", value);
		}
	}

	public string UITransparency
	{
		get
		{
			return App.GlobalSettings.GetPreset("UI.Transparency");
		}
		set
		{
			App.GlobalSettings.SetPreset("UI.Transparency", (value.Length >= 3) ? value.Substring(0, 3) : value);
			OnPropertyChanged("UITransparency");
		}
	}

	public string GraphicsQuality
	{
		get
		{
			return App.GlobalSettings.GetPreset("Rendering.SavedQualityLevel");
		}
		set
		{
			App.GlobalSettings.SetPreset("Rendering.SavedQualityLevel", value);
			OnPropertyChanged("GraphicsQuality");
		}
	}

	public bool ReducedMotion
	{
		get
		{
			return App.GlobalSettings.GetPreset("UI.ReducedMotion")?.ToLower() == "true";
		}
		set
		{
			App.GlobalSettings.SetPreset("UI.ReducedMotion", value);
		}
	}

	public IReadOnlyDictionary<FontSize, string?> FontSizes => GBSEditor.FontSizes;

	public FontSize SelectedFontSize
	{
		get
		{
			return FontSizes.FirstOrDefault<KeyValuePair<FontSize, string>>((KeyValuePair<FontSize, string> x) => x.Value == App.GlobalSettings.GetPreset("UI.FontSize")).Key;
		}
		set
		{
			App.GlobalSettings.SetPreset("UI.FontSize", FontSizes[value]);
		}
	}

	public string MouseSensitivity
	{
		get
		{
			return App.GlobalSettings.GetPreset("User.MouseSensitivity");
		}
		set
		{
			App.GlobalSettings.SetPreset("User.MouseSensitivity", value);
		}
	}

	public string VREnabled
	{
		get
		{
			return App.GlobalSettings.GetPreset("User.VREnabled");
		}
		set
		{
			App.GlobalSettings.SetPreset("User.VREnabled", value);
		}
	}
}
