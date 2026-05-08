using System;
using System.Collections.Generic;
using System.Reflection;
using Orbitstrap.Enums;
using Orbitstrap.Models;

namespace Orbitstrap;

public class LaunchSettings
{
	public LaunchFlag MenuFlag { get; } = new LaunchFlag("preferences,menu,settings");

	public LaunchFlag WatcherFlag { get; } = new LaunchFlag("watcher");

	public LaunchFlag MultiInstanceWatcherFlag { get; } = new LaunchFlag("multiinstancewatcher");

	public LaunchFlag BackgroundUpdaterFlag { get; } = new LaunchFlag("backgroundupdater");

	public LaunchFlag QuietFlag { get; } = new LaunchFlag("quiet");

	public LaunchFlag UninstallFlag { get; } = new LaunchFlag("uninstall");

	public LaunchFlag NoLaunchFlag { get; } = new LaunchFlag("nolaunch");

	public LaunchFlag TestModeFlag { get; } = new LaunchFlag("testmode");

	public LaunchFlag NoGPUFlag { get; } = new LaunchFlag("nogpu");

	public LaunchFlag UpgradeFlag { get; } = new LaunchFlag("upgrade");

	public LaunchFlag PlayerFlag { get; } = new LaunchFlag("player");

	public LaunchFlag StudioFlag { get; } = new LaunchFlag("studio");

	public LaunchFlag VersionFlag { get; } = new LaunchFlag("version");

	public LaunchFlag ChannelFlag { get; } = new LaunchFlag("channel");

	public LaunchFlag ForceFlag { get; } = new LaunchFlag("force");

	public LaunchFlag BloxshadeFlag { get; } = new LaunchFlag("bloxshade");

	public bool BypassUpdateCheck
	{
		get
		{
			if (!UninstallFlag.Active)
			{
				return WatcherFlag.Active;
			}
			return true;
		}
	}

	public LaunchMode RobloxLaunchMode { get; set; }

	public string RobloxLaunchArgs { get; set; } = "";

	public string[] Args { get; private set; }

	public LaunchSettings(string[] args)
	{
		Args = args;
		Dictionary<string, LaunchFlag> dictionary = new Dictionary<string, LaunchFlag>();
		PropertyInfo[] properties = GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (!(propertyInfo.PropertyType != typeof(LaunchFlag)) && propertyInfo.GetValue(this) is LaunchFlag launchFlag)
			{
				string[] array = launchFlag.Identifiers.Split(',');
				foreach (string key in array)
				{
					dictionary.Add(key, launchFlag);
				}
			}
		}
		int num = 0;
		if (Args.Length >= 1)
		{
			string text = Args[0];
			if (text.StartsWith("roblox:", StringComparison.OrdinalIgnoreCase) || text.StartsWith("roblox-player:", StringComparison.OrdinalIgnoreCase))
			{
				App.Logger.WriteLine("LaunchSettings::LaunchSettings", "Got Roblox player argument");
				RobloxLaunchMode = LaunchMode.Player;
				RobloxLaunchArgs = text;
				num = 1;
			}
			else if (text.StartsWith("version-"))
			{
				App.Logger.WriteLine("LaunchSettings::LaunchSettings", "Got version argument");
				VersionFlag.Active = true;
				VersionFlag.Data = text;
				num = 1;
			}
		}
		for (int k = num; k < Args.Length; k++)
		{
			string text2 = Args[k];
			if (!text2.StartsWith('-'))
			{
				App.Logger.WriteLine("LaunchSettings::LaunchSettings", "Invalid argument: " + text2);
				continue;
			}
			string text3 = text2;
			string text4 = text3.Substring(1, text3.Length - 1);
			if (!dictionary.TryGetValue(text4, out var value) || value == null)
			{
				App.Logger.WriteLine("LaunchSettings::LaunchSettings", "Unknown argument: " + text4);
				continue;
			}
			if (value.Active)
			{
				App.Logger.WriteLine("LaunchSettings::LaunchSettings", "Tried to set " + text4 + " flag twice");
				continue;
			}
			value.Active = true;
			if (k < Args.Length - 1)
			{
				string text5 = Args[k + 1];
				if (text5 != null && !text5.StartsWith('-'))
				{
					value.Data = text5;
					k++;
					App.Logger.WriteLine("LaunchSettings::LaunchSettings", "Identifier '" + text4 + "' is active with data");
					continue;
				}
			}
			App.Logger.WriteLine("LaunchSettings::LaunchSettings", "Identifier '" + text4 + "' is active");
		}
		if (VersionFlag.Active)
		{
			RobloxLaunchMode = LaunchMode.Unknown;
		}
		if (PlayerFlag.Active)
		{
			ParsePlayer(PlayerFlag.Data);
		}
		else if (StudioFlag.Active)
		{
			ParseStudio(StudioFlag.Data);
		}
	}

	private void ParsePlayer(string? data)
	{
		RobloxLaunchMode = LaunchMode.Player;
		if (!string.IsNullOrEmpty(data))
		{
			App.Logger.WriteLine("LaunchSettings::ParsePlayer", "Got Roblox launch arguments");
			RobloxLaunchArgs = data;
		}
		else
		{
			App.Logger.WriteLine("LaunchSettings::ParsePlayer", "No Roblox launch arguments were provided");
		}
	}

	private void ParseStudio(string? data)
	{
		RobloxLaunchMode = LaunchMode.Studio;
		if (string.IsNullOrEmpty(data))
		{
			App.Logger.WriteLine("LaunchSettings::ParseStudio", "No Roblox launch arguments were provided");
		}
		else if (data.StartsWith("roblox-studio:"))
		{
			App.Logger.WriteLine("LaunchSettings::ParseStudio", "Got Roblox Studio launch arguments");
			RobloxLaunchArgs = data;
		}
		else if (data.StartsWith("roblox-studio-auth:"))
		{
			App.Logger.WriteLine("LaunchSettings::ParseStudio", "Got Roblox Studio Auth launch arguments");
			RobloxLaunchMode = LaunchMode.StudioAuth;
			RobloxLaunchArgs = data;
		}
		else
		{
			App.Logger.WriteLine("LaunchSettings::ParseStudio", "Got Roblox Studio local place file");
			RobloxLaunchArgs = "-task EditFile -localPlaceFile \"" + data + "\"";
		}
	}
}
