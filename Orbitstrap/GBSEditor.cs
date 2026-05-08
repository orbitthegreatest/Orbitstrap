using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Orbitstrap.Enums.GBSPresets;

namespace Orbitstrap;

public class GBSEditor
{
	public Dictionary<string, string> PresetPaths = new Dictionary<string, string>
	{
		{ "Rendering.FramerateCap", "{UserSettings}/int[@name='FramerateCap']" },
		{ "Rendering.SavedQualityLevel", "{UserSettings}/token[@name='SavedQualityLevel']" },
		{ "User.MouseSensitivity", "{UserSettings}/float[@name='MouseSensitivity']" },
		{ "User.VREnabled", "{UserSettings}/bool[@name='VREnabled']" },
		{ "UI.Transparency", "{UserSettings}/float[@name='PreferredTransparency']" },
		{ "UI.ReducedMotion", "{UserSettings}/bool[@name='ReducedMotion']" },
		{ "UI.FontSize", "{UserSettings}/token[@name='PreferredTextSize']" }
	};

	public Dictionary<string, string> RootPaths = new Dictionary<string, string> { { "UserSettings", "//Item[@class='UserGameSettings']/Properties" } };

	public bool previousReadOnlyState;

	public XDocument? Document { get; set; }

	public static IReadOnlyDictionary<FontSize, string?> FontSizes => new Dictionary<FontSize, string>
	{
		{
			FontSize.x1,
			"1"
		},
		{
			FontSize.x2,
			"2"
		},
		{
			FontSize.x3,
			"3"
		},
		{
			FontSize.x4,
			"4"
		}
	};

	public bool Loaded { get; set; }

	public string FileLocation => Path.Combine(Paths.Roblox, "GlobalBasicSettings_13.xml");

	public void SetPreset(string prefix, object? value)
	{
		foreach (KeyValuePair<string, string> item in PresetPaths.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> x) => x.Key.StartsWith(prefix)))
		{
			SetValue(item.Value, value);
		}
	}

	public string? GetPreset(string prefix)
	{
		if (!PresetPaths.ContainsKey(prefix))
		{
			return null;
		}
		return GetValue(PresetPaths[prefix]);
	}

	public void SetValue(string path, object? value)
	{
		path = ResolvePath(path);
		XElement xElement = Document?.XPathSelectElement(path);
		if (xElement != null)
		{
			xElement.Value = value?.ToString();
		}
	}

	public string? GetValue(string path)
	{
		path = ResolvePath(path);
		return Document?.XPathSelectElement(path)?.Value;
	}

	public void SetReadOnly(bool readOnly, bool preserveState = false)
	{
		if (!File.Exists(FileLocation))
		{
			return;
		}
		try
		{
			FileAttributes attributes = File.GetAttributes(FileLocation);
			attributes = ((!readOnly) ? (attributes & ~FileAttributes.ReadOnly) : (attributes | FileAttributes.ReadOnly));
			File.SetAttributes(FileLocation, attributes);
			if (!preserveState)
			{
				previousReadOnlyState = readOnly;
			}
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("GBSEditor::SetReadOnly", "Failed to set read-only on " + FileLocation);
			App.Logger.WriteException("GBSEditor::SetReadOnly", ex);
		}
	}

	public bool GetReadOnly()
	{
		if (!File.Exists(FileLocation))
		{
			return false;
		}
		return File.GetAttributes(FileLocation).HasFlag(FileAttributes.ReadOnly);
	}

	public void Load()
	{
		string identifier = "GBSEditor::Load";
		App.Logger.WriteLine(identifier, "Loading from " + FileLocation + "...");
		if (!File.Exists(FileLocation))
		{
			return;
		}
		try
		{
			Document = XDocument.Load(FileLocation);
			Loaded = true;
			previousReadOnlyState = GetReadOnly();
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine(identifier, "Failed to load!");
			App.Logger.WriteException(identifier, ex);
		}
	}

	public virtual void Save()
	{
		string identifier = "GBSEditor::Save";
		App.Logger.WriteLine(identifier, "Saving to " + FileLocation + "...");
		try
		{
			SetReadOnly(readOnly: false, preserveState: true);
			Document?.Save(FileLocation);
			SetReadOnly(previousReadOnlyState);
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine(identifier, "Failed to save");
			App.Logger.WriteException(identifier, ex);
			return;
		}
		App.Logger.WriteLine(identifier, "Save complete!");
	}

	private string ResolvePath(string rawPath)
	{
		return Regex.Replace(rawPath, "\\{(.+?)\\}", delegate(Match match)
		{
			string value = match.Groups[1].Value;
			string value2;
			return (!RootPaths.TryGetValue(value, out value2)) ? match.Value : value2;
		});
	}
}
