using System;
using System.Collections.Generic;
using Microsoft.Win32;
using Orbitstrap.Extensions;

namespace Orbitstrap.Utility;

internal static class WindowsRegistry
{
	private const string RobloxPlaceKey = "Roblox.Place";

	public static readonly List<RegistryKey> Roots = new List<RegistryKey>
	{
		Registry.CurrentUser,
		Registry.LocalMachine
	};

	public static void RegisterProtocol(string key, string name, string handler, string handlerParam = "%1")
	{
		string text = "\"" + handler + "\" " + handlerParam;
		using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + key);
		using RegistryKey registryKey2 = registryKey.CreateSubKey("DefaultIcon");
		using RegistryKey registryKey3 = registryKey.CreateSubKey("shell\\open\\command");
		if (registryKey.GetValue("") == null)
		{
			registryKey.SetValueSafe("", "URL: " + name + " Protocol");
			registryKey.SetValueSafe("URL Protocol", "");
		}
		if (registryKey3.GetValue("") as string != text)
		{
			registryKey2.SetValueSafe("", handler);
			registryKey3.SetValueSafe("", text);
		}
	}

	public static void RegisterPlayer()
	{
		RegisterPlayer(Paths.Application, "-player \"%1\"");
	}

	public static void RegisterPlayer(string handler, string handlerParam)
	{
		RegisterProtocol("roblox", "Roblox", handler, handlerParam);
		RegisterProtocol("roblox-player", "Roblox", handler, handlerParam);
	}

	public static void RegisterStudio()
	{
		RegisterStudioProtocol(Paths.Application, "-studio \"%1\"");
		RegisterStudioFileClass(Paths.Application, "-studio \"%1\"");
		RegisterStudioFileTypes();
	}

	public static void RegisterStudioProtocol(string handler, string handlerParam)
	{
		RegisterProtocol("roblox-studio", "Roblox", handler, handlerParam);
		RegisterProtocol("roblox-studio-auth", "Roblox", handler, handlerParam);
	}

	public static void RegisterStudioFileTypes()
	{
		RegisterStudioFileType(".rbxl");
		RegisterStudioFileType(".rbxlx");
	}

	public static void RegisterStudioFileClass(string handler, string handlerParam)
	{
		string text = "\"" + handler + "\" " + handlerParam;
		string text2 = handler + ",0";
		using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\Roblox.Place");
		using RegistryKey registryKey2 = registryKey.CreateSubKey("DefaultIcon");
		using RegistryKey registryKey3 = registryKey.CreateSubKey("shell\\Open");
		using RegistryKey registryKey4 = registryKey3.CreateSubKey("command");
		if (registryKey.GetValue("") as string != "Roblox Place")
		{
			registryKey.SetValueSafe("", "Roblox Place");
		}
		if (registryKey4.GetValue("") as string != text)
		{
			registryKey4.SetValueSafe("", text);
		}
		if (registryKey3.GetValue("") as string != "Open")
		{
			registryKey3.SetValueSafe("", "Open");
		}
		if (registryKey2.GetValue("") as string != text2)
		{
			registryKey2.SetValueSafe("", text2);
		}
	}

	public static void RegisterStudioFileType(string key)
	{
		using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + key);
		registryKey.CreateSubKey("Roblox.Place\\ShellNew");
		if (registryKey.GetValue("") as string != "Roblox.Place")
		{
			registryKey.SetValueSafe("", "Roblox.Place");
		}
	}

	public static void RegisterApis()
	{
		RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey("Software\\Orbitstrap", writable: false);
		if (registryKey == null)
		{
			Register();
		}
		registryKey?.Dispose();
		static void Register()
		{
			using RegistryKey registryKey2 = Registry.CurrentUser.CreateSubKey("Software\\Orbitstrap");
			registryKey2.SetValueSafe("ApplicationPath", Paths.Application);
			registryKey2.SetValueSafe("InstallationPath", Paths.Base);
		}
	}

	public static void RegisterClientLocation(bool isStudio, string? clientPath)
	{
		string name = (isStudio ? "StudioPath" : "PlayerPath");
		if (clientPath == null)
		{
			clientPath = "";
		}
		using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Orbitstrap");
		registryKey.SetValueSafe(name, clientPath);
	}

	public static void Unregister(string key)
	{
		try
		{
			Registry.CurrentUser.DeleteSubKeyTree("Software\\Classes\\" + key);
		}
		catch (Exception value)
		{
			App.Logger.WriteLine("Protocol::Unregister", $"Failed to unregister {key}: {value}");
		}
	}
}
