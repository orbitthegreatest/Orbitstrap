using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Orbitstrap.AppData;
using Orbitstrap.Enums;

namespace Orbitstrap;

internal static class Utilities
{
	public static void ShellExecute(string website)
	{
		try
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = website,
				UseShellExecute = true
			});
		}
		catch (Win32Exception ex)
		{
			if (ex.NativeErrorCode != -2147221003)
			{
				throw;
			}
			Process.Start(new ProcessStartInfo
			{
				FileName = "rundll32.exe",
				Arguments = "shell32,OpenAs_RunDLL " + website
			});
		}
	}

	public static Version GetVersionFromString(string version)
	{
		if (version.StartsWith('v'))
		{
			string text = version;
			version = text.Substring(1, text.Length - 1);
		}
		int num = version.IndexOf('+');
		if (num != -1)
		{
			version = version.Substring(0, num);
		}
		return new Version(version);
	}

	public static VersionComparison CompareVersions(string versionStr1, string versionStr2)
	{
		try
		{
			Version versionFromString = GetVersionFromString(versionStr1);
			Version versionFromString2 = GetVersionFromString(versionStr2);
			return (VersionComparison)versionFromString.CompareTo(versionFromString2);
		}
		catch (Exception)
		{
			App.Logger.WriteLine("Utilities::CompareVersions", "An exception occurred when comparing versions");
			App.Logger.WriteLine("Utilities::CompareVersions", "versionStr1=" + versionStr1 + " versionStr2=" + versionStr2);
			throw;
		}
	}

	public static Version? ParseVersionSafe(string versionStr)
	{
		if (!Version.TryParse(versionStr, out Version result))
		{
			App.Logger.WriteLine("Utilities::ParseVersionSafe", "Failed to convert " + versionStr + " to a valid Version type.");
			return result;
		}
		return result;
	}

	public static string GetRobloxVersionStr(IAppData data)
	{
		string executablePath = data.ExecutablePath;
		if (!File.Exists(executablePath))
		{
			return "";
		}
		FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(executablePath);
		if (versionInfo.ProductVersion == null)
		{
			return "";
		}
		return versionInfo.ProductVersion.Replace(", ", ".");
	}

	public static string GetRobloxVersionStr(bool studio)
	{
		IAppData data;
		if (!studio)
		{
			IAppData appData = new RobloxPlayerData();
			data = appData;
		}
		else
		{
			IAppData appData = new RobloxStudioData();
			data = appData;
		}
		return GetRobloxVersionStr(data);
	}

	public static Version? GetRobloxVersion(IAppData data)
	{
		return ParseVersionSafe(GetRobloxVersionStr(data));
	}

	public static Process[] GetProcessesSafe()
	{
		try
		{
			return Process.GetProcesses();
		}
		catch (ArithmeticException ex)
		{
			App.Logger.WriteLine("Utilities::GetProcessesSafe", "Unable to fetch processes!");
			App.Logger.WriteException("Utilities::GetProcessesSafe", ex);
			return Array.Empty<Process>();
		}
	}

	public static bool DoesMutexExist(string name)
	{
		try
		{
			Mutex.OpenExisting(name).Close();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static void KillBackgroundUpdater()
	{
		using EventWaitHandle eventWaitHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, "Orbitstrap-BackgroundUpdaterKillEvent");
		eventWaitHandle.Set();
	}
}
