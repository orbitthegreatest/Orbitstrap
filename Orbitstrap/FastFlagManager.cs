using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orbitstrap.Enums.FlagPresets;

namespace Orbitstrap;

public class FastFlagManager : JsonManager<Dictionary<string, object>>
{
	public static IReadOnlyDictionary<string, string> PresetFlags = new Dictionary<string, string>
	{
		{ "Rendering.ManualFullscreen", "FFlagHandleAltEnterFullscreenManually" },
		{ "Rendering.DisableScaling", "DFFlagDisableDPIScale" },
		{ "Rendering.MSAA", "FIntDebugForceMSAASamples" },
		{ "Rendering.FRMQualityOverride", "DFIntDebugFRMQualityLevelOverride" },
		{ "Rendering.Mode.DisableD3D11", "FFlagDebugGraphicsDisableDirect3D11" },
		{ "Rendering.Mode.D3D11", "FFlagDebugGraphicsPreferD3D11" },
		{ "Rendering.Mode.Vulkan", "FFlagDebugGraphicsPreferVulkan" },
		{ "Rendering.Mode.OpenGL", "FFlagDebugGraphicsPreferOpenGL" },
		{ "Geometry.MeshLOD.Static", "DFIntCSGLevelOfDetailSwitchingDistanceStatic" },
		{ "Geometry.MeshLOD.L0", "DFIntCSGLevelOfDetailSwitchingDistance" },
		{ "Geometry.MeshLOD.L12", "DFIntCSGLevelOfDetailSwitchingDistanceL12" },
		{ "Geometry.MeshLOD.L23", "DFIntCSGLevelOfDetailSwitchingDistanceL23" },
		{ "Geometry.MeshLOD.L34", "DFIntCSGLevelOfDetailSwitchingDistanceL34" },
		{ "Rendering.TextureQuality.OverrideEnabled", "DFFlagTextureQualityOverrideEnabled" },
		{ "Rendering.TextureQuality.Level", "DFIntTextureQualityOverride" }
	};

	public override string ClassName => "FastFlagManager";

	public override string LOG_IDENT_CLASS => ClassName;

	public override string ProfilesLocation => Path.Combine(Paths.Base, "Profiles");

	public override string FileLocation => Path.Combine(Paths.Modifications, "ClientSettings\\ClientAppSettings.json");

	public bool Changed => !base.OriginalProp.SequenceEqual(base.Prop);

	public static IReadOnlyDictionary<RenderingMode, string> RenderingModes => new Dictionary<RenderingMode, string>
	{
		{
			RenderingMode.Default,
			"None"
		},
		{
			RenderingMode.Vulkan,
			"Vulkan"
		},
		{
			RenderingMode.OpenGL,
			"OpenGL"
		},
		{
			RenderingMode.D3D11,
			"D3D11"
		}
	};

	public static IReadOnlyDictionary<MSAAMode, string?> MSAAModes => new Dictionary<MSAAMode, string>
	{
		{
			MSAAMode.Default,
			null
		},
		{
			MSAAMode.x1,
			"1"
		},
		{
			MSAAMode.x2,
			"2"
		},
		{
			MSAAMode.x4,
			"4"
		}
	};

	public static IReadOnlyDictionary<TextureQuality, string?> TextureQualityLevels => new Dictionary<TextureQuality, string>
	{
		{
			TextureQuality.Default,
			null
		},
		{
			TextureQuality.Level0,
			"0"
		},
		{
			TextureQuality.Level1,
			"1"
		},
		{
			TextureQuality.Level2,
			"2"
		},
		{
			TextureQuality.Level3,
			"3"
		}
	};

	public void SetValue(string key, object? value)
	{
		if (value == null)
		{
			if (base.Prop.ContainsKey(key))
			{
				App.Logger.WriteLine("FastFlagManager::SetValue", "Deletion of '" + key + "' is pending");
			}
			base.Prop.Remove(key);
			return;
		}
		if (base.Prop.ContainsKey(key))
		{
			if (key == base.Prop[key].ToString())
			{
				return;
			}
			App.Logger.WriteLine("FastFlagManager::SetValue", $"Changing of '{key}' from '{base.Prop[key]}' to '{value}' is pending");
		}
		else
		{
			App.Logger.WriteLine("FastFlagManager::SetValue", $"Setting of '{key}' to '{value}' is pending");
		}
		base.Prop[key] = value.ToString();
	}

	public string? GetValue(string key)
	{
		if (base.Prop.TryGetValue(key, out object value) && value != null)
		{
			return value.ToString();
		}
		return null;
	}

	public void SetPreset(string prefix, object? value)
	{
		foreach (KeyValuePair<string, string> item in PresetFlags.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> x) => x.Key.StartsWith(prefix)))
		{
			SetValue(item.Value, value);
		}
	}

	public void SetPresetEnum(string prefix, string target, object? value)
	{
		foreach (KeyValuePair<string, string> item in PresetFlags.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> x) => x.Key.StartsWith(prefix)))
		{
			if (item.Key.StartsWith(prefix + "." + target))
			{
				SetValue(item.Value, value);
			}
			else
			{
				SetValue(item.Value, null);
			}
		}
	}

	public string? GetPreset(string name)
	{
		if (!PresetFlags.ContainsKey(name))
		{
			App.Logger.WriteLine("FastFlagManager::GetPreset", "Could not find preset " + name);
			return null;
		}
		return GetValue(PresetFlags[name]);
	}

	public T GetPresetEnum<T>(IReadOnlyDictionary<T, string> mapping, string prefix, string value) where T : Enum
	{
		foreach (KeyValuePair<T, string> item in mapping)
		{
			if (!(item.Value == "None") && GetPreset(prefix + "." + item.Value) == value)
			{
				return item.Key;
			}
		}
		return mapping.First().Key;
	}

	public bool IsPreset(string Flag)
	{
		return PresetFlags.Values.Any((string v) => v.ToLower() == Flag.ToLower());
	}

	public override void Save()
	{
		foreach (KeyValuePair<string, object> item in base.Prop)
		{
			base.Prop[item.Key] = item.Value.ToString();
		}
		base.Save();
		base.OriginalProp = new Dictionary<string, object>(base.Prop);
	}

	public override void Load(bool alertFailure = true)
	{
		base.Load(alertFailure);
		base.OriginalProp = new Dictionary<string, object>(base.Prop);
		if (GetPreset("Rendering.ManualFullscreen") != "False")
		{
			SetPreset("Rendering.ManualFullscreen", "False");
		}
	}
}
