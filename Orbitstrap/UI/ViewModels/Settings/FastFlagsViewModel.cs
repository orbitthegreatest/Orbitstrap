using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Orbitstrap.Enums.FlagPresets;

namespace Orbitstrap.UI.ViewModels.Settings;

public class FastFlagsViewModel : NotifyPropertyChangedViewModel
{
	private Dictionary<string, object>? _preResetFlags;

	private static readonly string[] LODLevels = new string[4] { "L0", "L12", "L23", "L34" };

	public ICommand OpenFastFlagEditorCommand => new RelayCommand(OpenFastFlagEditor);

	public bool UseFastFlagManager
	{
		get
		{
			return App.Settings.Prop.UseFastFlagManager;
		}
		set
		{
			App.Settings.Prop.UseFastFlagManager = value;
		}
	}

	public bool ModInjectorEnabled
	{
		get
		{
			return App.Settings.Prop.ModInjectorEnabled;
		}
		set
		{
			App.Settings.Prop.ModInjectorEnabled = value;
			App.Injector.Toggle(value);
			OnPropertyChanged("ModInjectorEnabled");
		}
	}

	public IReadOnlyDictionary<MSAAMode, string?> MSAALevels => FastFlagManager.MSAAModes;

	public MSAAMode SelectedMSAALevel
	{
		get
		{
			return MSAALevels.FirstOrDefault<KeyValuePair<MSAAMode, string>>((KeyValuePair<MSAAMode, string> x) => x.Value == App.FastFlags.GetPreset("Rendering.MSAA")).Key;
		}
		set
		{
			App.FastFlags.SetPreset("Rendering.MSAA", MSAALevels[value]);
		}
	}

	public IReadOnlyDictionary<RenderingMode, string> RenderingModes => FastFlagManager.RenderingModes;

	public RenderingMode SelectedRenderingMode
	{
		get
		{
			return App.FastFlags.GetPresetEnum(RenderingModes, "Rendering.Mode", "True");
		}
		set
		{
			RenderingMode[] source = new RenderingMode[2]
			{
				RenderingMode.Vulkan,
				RenderingMode.OpenGL
			};
			App.FastFlags.SetPresetEnum("Rendering.Mode", value.ToString(), "True");
			App.FastFlags.SetPreset("Rendering.Mode.DisableD3D11", Enumerable.Contains(source, value) ? "True" : null);
		}
	}

	public bool FixDisplayScaling
	{
		get
		{
			return App.FastFlags.GetPreset("Rendering.DisableScaling") == "True";
		}
		set
		{
			App.FastFlags.SetPreset("Rendering.DisableScaling", value ? "True" : null);
		}
	}

	public IReadOnlyDictionary<TextureQuality, string?> TextureQualities => FastFlagManager.TextureQualityLevels;

	public TextureQuality SelectedTextureQuality
	{
		get
		{
			return TextureQualities.Where<KeyValuePair<TextureQuality, string>>((KeyValuePair<TextureQuality, string> x) => x.Value == App.FastFlags.GetPreset("Rendering.TextureQuality.Level")).FirstOrDefault().Key;
		}
		set
		{
			if (value == TextureQuality.Default)
			{
				App.FastFlags.SetPreset("Rendering.TextureQuality", null);
				return;
			}
			App.FastFlags.SetPreset("Rendering.TextureQuality.OverrideEnabled", "True");
			App.FastFlags.SetPreset("Rendering.TextureQuality.Level", TextureQualities[value]);
		}
	}

	public bool FRMQualityOverrideEnabled
	{
		get
		{
			return App.FastFlags.GetPreset("Rendering.FRMQualityOverride") != null;
		}
		set
		{
			if (value)
			{
				FRMQualityOverride = 21;
			}
			else
			{
				App.FastFlags.SetPreset("Rendering.FRMQualityOverride", null);
			}
			OnPropertyChanged("FRMQualityOverride");
			OnPropertyChanged("FRMQualityOverrideEnabled");
		}
	}

	public int FRMQualityOverride
	{
		get
		{
			if (!int.TryParse(App.FastFlags.GetPreset("Rendering.FRMQualityOverride"), out var result))
			{
				return 21;
			}
			return result;
		}
		set
		{
			App.FastFlags.SetPreset("Rendering.FRMQualityOverride", value);
			OnPropertyChanged("FRMQualityOverride");
		}
	}

	public bool MeshQualityEnabled
	{
		get
		{
			return App.FastFlags.GetPreset("Geometry.MeshLOD.Static") != null;
		}
		set
		{
			if (value)
			{
				MeshQuality = 3;
			}
			else
			{
				string[] lODLevels = LODLevels;
				foreach (string text in lODLevels)
				{
					App.FastFlags.SetPreset("Geometry.MeshLOD." + text, null);
				}
				App.FastFlags.SetPreset("Geometry.MeshLOD.Static", null);
			}
			OnPropertyChanged("MeshQualityEnabled");
		}
	}

	public int MeshQuality
	{
		get
		{
			if (!int.TryParse(App.FastFlags.GetPreset("Geometry.MeshLOD.Static"), out var result))
			{
				return 0;
			}
			return result;
		}
		set
		{
			int num = Math.Clamp(value, 0, LODLevels.Length - 1);
			for (int i = 0; i < LODLevels.Length; i++)
			{
				int num2 = Math.Clamp(num - i, 0, 3);
				string text = LODLevels[i];
				App.FastFlags.SetPreset("Geometry.MeshLOD." + text, num2);
			}
			App.FastFlags.SetPreset("Geometry.MeshLOD.Static", num);
			OnPropertyChanged("MeshQuality");
			OnPropertyChanged("MeshQualityEnabled");
		}
	}

	public bool ResetConfiguration
	{
		get
		{
			return _preResetFlags != null;
		}
		set
		{
			if (value)
			{
				_preResetFlags = new Dictionary<string, object>(App.FastFlags.Prop);
				App.FastFlags.Prop.Clear();
			}
			else
			{
				App.FastFlags.Prop = _preResetFlags;
				_preResetFlags = null;
			}
			this.RequestPageReloadEvent?.Invoke(this, EventArgs.Empty);
		}
	}

	public event EventHandler? RequestPageReloadEvent;

	public event EventHandler? OpenFlagEditorEvent;

	private void OpenFastFlagEditor()
	{
		this.OpenFlagEditorEvent?.Invoke(this, EventArgs.Empty);
	}
}
