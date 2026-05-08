using System;
using System.IO;
using Orbitstrap.Models.SettingTasks.Base;
using Orbitstrap.Utility;

namespace Orbitstrap.Models.SettingTasks;

public class FontModPresetTask : StringBaseTask
{
	public string? GetFileHash()
	{
		if (!File.Exists(Paths.CustomFont))
		{
			return null;
		}
		using FileStream inputStream = File.OpenRead(Paths.CustomFont);
		return MD5Hash.Stringify(App.MD5Provider.ComputeHash(inputStream));
	}

	public FontModPresetTask()
		: base("ModPreset", "TextFont")
	{
		if (File.Exists(Paths.CustomFont))
		{
			OriginalState = Paths.CustomFont;
		}
	}

	public override void Execute()
	{
		if (!string.IsNullOrEmpty(NewState))
		{
			if (string.Compare(NewState, Paths.CustomFont, StringComparison.InvariantCultureIgnoreCase) != 0 && File.Exists(NewState))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(Paths.CustomFont));
				Filesystem.AssertReadOnly(Paths.CustomFont);
				File.Copy(NewState, Paths.CustomFont, overwrite: true);
			}
		}
		else if (File.Exists(Paths.CustomFont))
		{
			Filesystem.AssertReadOnly(Paths.CustomFont);
			File.Delete(Paths.CustomFont);
		}
		OriginalState = NewState;
	}
}
