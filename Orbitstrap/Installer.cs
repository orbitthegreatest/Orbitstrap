using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using ShellLink;
using Orbitstrap.Enums;
using Orbitstrap.Extensions;
using Orbitstrap.Resources;
using Orbitstrap.UI;
using Orbitstrap.Utility;

namespace Orbitstrap;

internal class Installer
{
	private const bool OpenReleaseNotes = false;

	public string OrbitstrapInstallDirectory = Path.Combine(Paths.LocalAppData, "Orbitstrap");

	public string InstallLocation = Path.Combine(Paths.LocalAppData, "Orbitstrap");

	public bool CreateDesktopShortcuts = true;

	public bool CreateStartMenuShortcuts = true;

	public bool ImportSettings = Directory.Exists(Path.Combine(Paths.LocalAppData, "Orbitstrap"));

	public bool IsImplicitInstall;

	public string[] FilesForImporting = new string[3] { "CustomThemes", "Modifications", "Settings.json" };

	private static string DesktopShortcut => Path.Combine(Paths.Desktop, "Orbitstrap.lnk");

	private static string StartMenuShortcut => Path.Combine(Paths.WindowsStartMenu, "Orbitstrap.lnk");

	public bool ExistingDataPresent => File.Exists(Path.Combine(InstallLocation, "Settings.json"));

	public string InstallLocationError { get; set; } = "";

	public void DoInstall()
	{
		App.Logger.WriteLine("Installer::DoInstall", "Beginning installation");
		Directory.CreateDirectory(InstallLocation);
		string path = Path.Combine(InstallLocation, "Settings.json");
		if (!File.Exists(path))
		{
			try
			{
				string contents = "{\n  \"BootstrapperStyle\": 7,\n  \"BootstrapperIcon\": 0,\n  \"BootstrapperTitle\": \"Orbitstrap\",\n  \"BootstrapperIconCustomLocation\": \"\",\n  \"Theme\": 0,\n  \"ForceLocalData\": false,\n  \"CheckForUpdates\": true,\n  \"MultiInstanceLaunching\": false,\n  \"ConfirmLaunches\": true,\n  \"Locale\": \"nil\",\n  \"ForceRobloxLanguage\": false,\n  \"UseFastFlagManager\": true,\n  \"ModInjectorEnabled\": true,\n  \"WPFSoftwareRender\": false,\n  \"EnableAnalytics\": false,\n  \"UpdateRoblox\": true,\n  \"Channel\": \"production\",\n  \"ChannelChangeMode\": 0,\n  \"ChannelHash\": \"\",\n  \"DownloadingStringFormat\": \"Downloading {0} - {1}MB / {2}MB\",\n  \"SelectedCustomTheme\": null,\n  \"BackgroundUpdatesEnabled\": false,\n  \"DebugDisableVersionPackageCleanup\": false,\n  \"WebEnvironment\": \"Production\",\n  \"CleanerOptions\": 0,\n  \"CleanerDirectories\": [],\n  \"EnableActivityTracking\": true,\n  \"UseDiscordRichPresence\": true,\n  \"HideRPCButtons\": true,\n  \"ShowAccountOnRichPresence\": false,\n  \"ShowServerDetails\": false,\n  \"ShowServerUptime\": false,\n  \"CustomIntegrations\": [],\n  \"UseDisableAppPatch\": false\n}";
				File.WriteAllText(path, contents);
				App.Logger.WriteLine("Installer::DoInstall", "Created default Settings.json");
			}
			catch (Exception ex)
			{
				App.Logger.WriteLine("Installer::DoInstall", "Failed to create default Settings.json");
				App.Logger.WriteException("Installer::DoInstall", ex);
			}
		}
		string path2 = Path.Combine(InstallLocation, "Data.json");
		if (!File.Exists(path2))
		{
			try
			{
				string contents2 = "{\n  \"alertEnabled\": false,\n  \"alertContent\": \"\",\n  \"alertSeverity\": 0,\n  \"killFlags\": false,\n  \"deeplinkUrl\": \"roblox://experiences/start\",\n  \"packageMaps\": {\n    \"common\": {\n      \"Libraries.zip\": \"\",\n      \"redist.zip\": \"\",\n      \"shaders.zip\": \"shaders\\\\\",\n      \"ssl.zip\": \"ssl\\\\\",\n      \"WebView2.zip\": \"\",\n      \"WebView2RuntimeInstaller.zip\": \"WebView2RuntimeInstaller\\\\\",\n      \"content-avatar.zip\": \"content\\\\avatar\\\\\",\n      \"content-configs.zip\": \"content\\\\configs\\\\\",\n      \"content-fonts.zip\": \"content\\\\fonts\\\\\",\n      \"content-sky.zip\": \"content\\\\sky\\\\\",\n      \"content-sounds.zip\": \"content\\\\sounds\\\\\",\n      \"content-textures2.zip\": \"content\\\\textures\\\\\",\n      \"content-models.zip\": \"content\\\\models\\\\\",\n      \"content-textures3.zip\": \"PlatformContent\\\\pc\\\\textures\\\\\",\n      \"content-terrain.zip\": \"PlatformContent\\\\pc\\\\terrain\\\\\",\n      \"content-platform-fonts.zip\": \"PlatformContent\\\\pc\\\\fonts\\\\\",\n      \"content-platform-dictionaries.zip\": \"PlatformContent\\\\pc\\\\shared_compression_dictionaries\\\\\",\n      \"extracontent-luapackages.zip\": \"ExtraContent\\\\LuaPackages\\\\\",\n      \"extracontent-translations.zip\": \"ExtraContent\\\\translations\\\\\",\n      \"extracontent-models.zip\": \"ExtraContent\\\\models\\\\\",\n      \"extracontent-textures.zip\": \"ExtraContent\\\\textures\\\\\",\n      \"extracontent-places.zip\": \"ExtraContent\\\\places\\\\\"\n    },\n    \"player\": {\n      \"RobloxApp.zip\": \"\"\n    },\n    \"studio\": {\n      \"RobloxStudio.zip\": \"\",\n      \"LibrariesQt5.zip\": \"\",\n      \"content-studio_svg_textures.zip\": \"content\\\\studio_svg_textures\\\\\",\n      \"content-qt_translations.zip\": \"content\\\\qt_translations\\\\\",\n      \"content-api-docs.zip\": \"content\\\\api_docs\\\\\",\n      \"extracontent-scripts.zip\": \"ExtraContent\\\\scripts\\\\\",\n      \"studiocontent-models.zip\": \"StudioContent\\\\models\\\\\",\n      \"studiocontent-textures.zip\": \"StudioContent\\\\textures\\\\\",\n      \"BuiltInPlugins.zip\": \"BuiltInPlugins\\\\\",\n      \"BuiltInStandalonePlugins.zip\": \"BuiltInStandalonePlugins\\\\\",\n      \"ApplicationConfig.zip\": \"ApplicationConfig\\\\\",\n      \"Plugins.zip\": \"Plugins\\\\\",\n      \"Qml.zip\": \"Qml\\\\\",\n      \"StudioFonts.zip\": \"StudioFonts\\\\\",\n      \"RibbonConfig.zip\": \"RibbonConfig\\\\\"\n    }\n  }\n}";
				File.WriteAllText(path2, contents2);
				App.Logger.WriteLine("Installer::DoInstall", "Created default Data.json");
			}
			catch (Exception ex2)
			{
				App.Logger.WriteLine("Installer::DoInstall", "Failed to create default Data.json");
				App.Logger.WriteException("Installer::DoInstall", ex2);
			}
		}
		Paths.Initialize(InstallLocation);

		// ── Deploy bundled skybox packs to AppData ──────────────────────────
		// The skybox .tex files are shipped as Content items embedded in the
		// single-file exe (IncludeAllContentForSelfExtract = true). At runtime
		// .NET extracts them to AppDomain.CurrentDomain.BaseDirectory\Skyboxes\.
		// We copy them from there to Paths.Skyboxes (= AppData\Orbitstrap\Skyboxes\)
		// so they are permanently available even after the temp dir is cleaned up,
		// and so the rest of the skybox pipeline can use Paths.Skyboxes as its
		// single source of truth.
		try
		{
			string srcSkyboxRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Skyboxes");
			if (Directory.Exists(srcSkyboxRoot))
			{
				Directory.CreateDirectory(Paths.Skyboxes);
				foreach (string packDir in Directory.GetDirectories(srcSkyboxRoot))
				{
					string packName = Path.GetFileName(packDir);
					string destPackDir = Path.Combine(Paths.Skyboxes, packName);
					Directory.CreateDirectory(destPackDir);
					foreach (string texFile in Directory.GetFiles(packDir, "*.tex"))
					{
						string destFile = Path.Combine(destPackDir, Path.GetFileName(texFile));
						// Always overwrite — ensures updates ship fresh textures
						File.Copy(texFile, destFile, overwrite: true);
					}
				}
				App.Logger.WriteLine("Installer::DoInstall",
					$"Deployed skybox packs to {Paths.Skyboxes}");
			}
			else
			{
				App.Logger.WriteLine("Installer::DoInstall",
					"No bundled Skyboxes directory found — skipping skybox deployment");
			}
		}
		catch (Exception exSky)
		{
			// Non-fatal — log and continue; the skybox feature simply won't work
			App.Logger.WriteLine("Installer::DoInstall", "Failed to deploy skybox packs");
			App.Logger.WriteException("Installer::DoInstall", exSky);
		}
		// ── End skybox deployment ────────────────────────────────────────────
		if (!IsImplicitInstall)
		{
			Filesystem.AssertReadOnly(Paths.Application);
			try
			{
				// Always copy the main executable first.
				File.Copy(Paths.Process, Paths.Application, overwrite: true);

				// For debug (non-single-file) builds, the DLLs are separate files sitting
				// next to the exe. Copy them all so AppData has a complete runnable install.
				// For a single-file publish the loop finds only the exe itself — harmless.
				string srcDir = Path.GetDirectoryName(Paths.Process) ?? "";
				if (!string.IsNullOrEmpty(srcDir) &&
					!string.Equals(srcDir, Paths.Base, StringComparison.OrdinalIgnoreCase))
				{
					foreach (string srcFile in Directory.GetFiles(srcDir))
					{
						if (string.Equals(srcFile, Paths.Process, StringComparison.OrdinalIgnoreCase))
							continue;
						string destFile = Path.Combine(Paths.Base, Path.GetFileName(srcFile));
						try { File.Copy(srcFile, destFile, overwrite: true); }
						catch (Exception exFile)
						{
							App.Logger.WriteLine("Installer::DoInstall",
								$"Could not copy '{Path.GetFileName(srcFile)}': {exFile.Message}");
						}
					}
					App.Logger.WriteLine("Installer::DoInstall",
						$"Copied side-by-side files from '{srcDir}' to '{Paths.Base}'");
				}
			}
			catch (Exception ex3)
			{
				App.Logger.WriteLine("Installer::DoInstall", "Could not overwrite executable");
				App.Logger.WriteException("Installer::DoInstall", ex3);
				Frontend.ShowMessageBox(Strings.Installer_Install_CannotOverwrite, MessageBoxImage.Hand);
				App.Terminate(ErrorCode.ERROR_INSTALL_FAILURE);
			}
		}
		using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Orbitstrap"))
		{
			registryKey.SetValueSafe("DisplayIcon", Paths.Application + ",0");
			registryKey.SetValueSafe("DisplayName", "Orbitstrap");
			registryKey.SetValueSafe("DisplayVersion", App.Version);
			if (registryKey.GetValue("InstallDate") == null)
			{
				registryKey.SetValueSafe("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
			}
			registryKey.SetValueSafe("InstallLocation", Paths.Base);
			registryKey.SetValueSafe("NoRepair", 1);
			registryKey.SetValueSafe("Publisher", "Orbitstrap");
			registryKey.SetValueSafe("ModifyPath", "\"" + Paths.Application + "\" -settings");
			registryKey.SetValueSafe("QuietUninstallString", "\"" + Paths.Application + "\" -uninstall -quiet");
			registryKey.SetValueSafe("UninstallString", "\"" + Paths.Application + "\" -uninstall");
			registryKey.SetValueSafe("HelpLink", "https://github.com/4r6z/");
			registryKey.SetValueSafe("URLInfoAbout", "https://github.com/4r6z/");
			registryKey.SetValueSafe("URLUpdateInfo", "https://github.com/4r6z/");
		}
		WindowsRegistry.RegisterApis();
		WindowsRegistry.RegisterPlayer();
		if (CreateDesktopShortcuts)
		{
			Orbitstrap.Utility.Shortcut.Create(Paths.Application, "", DesktopShortcut);
		}
		if (CreateStartMenuShortcuts)
		{
			Orbitstrap.Utility.Shortcut.Create(Paths.Application, "", StartMenuShortcut);
		}
		if (ImportSettings)
		{
			try
			{
				ImportSettingsFromOrbitstrap();
			}
			catch (IOException)
			{
			}
			catch (Exception)
			{
			}
		}
		App.Settings.Load(alertFailure: false);
		App.State.Load(alertFailure: false);
		App.FastFlags.Load(alertFailure: false);
		if (App.IsStudioVisible)
		{
			WindowsRegistry.RegisterStudio();
		}
		App.Settings.Save();
		App.Logger.WriteLine("Installer::DoInstall", "Installation finished");
	}

	private bool ValidateLocation()
	{
		if (InstallLocation.Length <= 3)
		{
			return false;
		}
		if (InstallLocation.StartsWith("\\\\"))
		{
			return false;
		}
		if (InstallLocation.StartsWith(Path.GetTempPath(), StringComparison.InvariantCultureIgnoreCase) || InstallLocation.Contains("\\Temp\\", StringComparison.InvariantCultureIgnoreCase))
		{
			return false;
		}
		if (InstallLocation.Contains("OneDrive", StringComparison.InvariantCultureIgnoreCase))
		{
			return false;
		}
		if (string.Compare(Directory.GetParent(InstallLocation)?.FullName, Paths.UserProfile, StringComparison.InvariantCultureIgnoreCase) == 0)
		{
			return false;
		}
		if (InstallLocation.Contains("Program Files"))
		{
			return false;
		}
		if (InstallLocation.Contains("Local\\Orbitstrap"))
		{
			return false;
		}
		return true;
	}

	public bool CheckInstallLocation()
	{
		if (string.IsNullOrEmpty(InstallLocation))
		{
			InstallLocationError = Strings.Menu_InstallLocation_NotSet;
		}
		else if (ValidateLocation())
		{
			if (!IsImplicitInstall && !InstallLocation.EndsWith("Orbitstrap", StringComparison.InvariantCultureIgnoreCase) && Directory.Exists(InstallLocation) && Directory.EnumerateFileSystemEntries(InstallLocation).Any())
			{
				string text = Path.Combine(InstallLocation, "Orbitstrap");
				switch (Frontend.ShowMessageBox(string.Format(Strings.Menu_InstallLocation_NotEmpty, text), MessageBoxImage.Exclamation, MessageBoxButton.YesNoCancel, MessageBoxResult.Yes))
				{
				case MessageBoxResult.Yes:
					InstallLocation = text;
					break;
				case MessageBoxResult.None:
				case MessageBoxResult.Cancel:
					return false;
				}
			}
			try
			{
				string path = Path.Combine(InstallLocation, "OrbitstrapWriteTest.txt");
				Directory.CreateDirectory(InstallLocation);
				File.WriteAllText(path, "");
				File.Delete(path);
			}
			catch (UnauthorizedAccessException)
			{
				InstallLocationError = Strings.Menu_InstallLocation_NoWritePerms;
			}
			catch (Exception ex2)
			{
				InstallLocationError = ex2.Message;
			}
		}
		return string.IsNullOrEmpty(InstallLocationError);
	}

	public static void DoUninstall(bool keepData)
	{
		List<Process> list = new List<Process>();
		if (!string.IsNullOrEmpty(App.RobloxState.Prop.Player.VersionGuid))
		{
			list.AddRange(Process.GetProcessesByName("RobloxPlayerBeta.exe"));
		}
		if (App.IsStudioVisible)
		{
			list.AddRange(Process.GetProcessesByName("RobloxStudioBeta.exe"));
		}
		if (list.Any())
		{
			if (Frontend.ShowMessageBox(Strings.Bootstrapper_Uninstall_RobloxRunning, MessageBoxImage.Asterisk, MessageBoxButton.OKCancel, MessageBoxResult.OK) != MessageBoxResult.OK)
			{
				App.Terminate(ErrorCode.ERROR_CANCELLED);
				return;
			}
			try
			{
				foreach (Process item in list)
				{
					item.Kill();
					item.Close();
				}
			}
			catch (Exception value)
			{
				App.Logger.WriteLine("Installer::DoUninstall", $"Failed to close process! {value}");
			}
		}
		try
		{
			Process currentProcess = Process.GetCurrentProcess();
			foreach (Process item2 in from p in Process.GetProcessesByName(currentProcess.ProcessName)
				where p.Id != currentProcess.Id
				select p)
			{
				item2.Kill();
				item2.WaitForExit(1000);
				item2.Dispose();
			}
		}
		catch (Exception value2)
		{
			App.Logger.WriteLine("Installer::DoUninstall", $"Failed to close other Orbitstrap instances: {value2}");
		}
		string robloxFolder = Path.Combine(Paths.LocalAppData, "Roblox");
		bool flag = true;
		bool flag2 = true;
		using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\roblox-player");
		object obj = registryKey?.GetValue("InstallLocation");
		if (registryKey == null || !(obj is string))
		{
			flag = false;
			WindowsRegistry.Unregister("roblox");
			WindowsRegistry.Unregister("roblox-player");
		}
		else
		{
			bool flag3 = File.Exists(Path.Combine((string)obj, "eurotrucks2.exe"));
			WindowsRegistry.RegisterPlayer(Path.Combine((string)obj, flag3 ? "eurotrucks2.exe" : "RobloxPlayerBeta.exe"), "%1");
		}
		using RegistryKey registryKey2 = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\roblox-studio");
		object obj2 = registryKey2?.GetValue("InstallLocation");
		if (registryKey2 == null || !(obj2 is string))
		{
			flag2 = false;
			WindowsRegistry.Unregister("roblox-studio");
			WindowsRegistry.Unregister("roblox-studio-auth");
			WindowsRegistry.Unregister("Roblox.Place");
			WindowsRegistry.Unregister(".rbxl");
			WindowsRegistry.Unregister(".rbxlx");
		}
		else
		{
			string handler = Path.Combine((string)obj2, "RobloxStudioBeta.exe");
			Path.Combine((string)obj2, "RobloxStudioLauncherBeta.exe");
			WindowsRegistry.RegisterStudioProtocol(handler, "%1");
			WindowsRegistry.RegisterStudioFileClass(handler, "-ide \"%1\"");
		}
		Registry.CurrentUser.DeleteSubKey("Software\\Orbitstrap");
		List<Action> list2 = new List<Action>
		{
			delegate
			{
				foreach (string item3 in from x in Directory.GetFiles(Paths.Desktop)
					where x.EndsWith("lnk")
					select x)
				{
					if (ShellLink.Shortcut.ReadFromFile(item3).ExtraData.EnvironmentVariableDataBlock?.TargetUnicode == Paths.Application)
					{
						File.Delete(item3);
					}
				}
			},
			delegate
			{
				File.Delete(StartMenuShortcut);
			},
			delegate
			{
				Directory.Delete(Paths.Versions, recursive: true);
			},
			delegate
			{
				Directory.Delete(Paths.Downloads, recursive: true);
			},
			delegate
			{
				File.Delete(App.State.FileLocation);
			},
			delegate
			{
				if (Paths.Roblox == Path.Combine(Paths.Base, "Roblox"))
				{
					Directory.Delete(Paths.Roblox, recursive: true);
				}
			}
		};
		if (!keepData)
		{
			list2.AddRange(new List<Action>
			{
				delegate
				{
					Directory.Delete(Paths.Modifications, recursive: true);
				},
				delegate
				{
					Directory.Delete(Paths.Logs, recursive: true);
				},
				delegate
				{
					File.Delete(App.Settings.FileLocation);
				}
			});
		}
		bool flag4 = Directory.GetFiles(Paths.Base).Length <= 3;
		if (flag4)
		{
			list2.Add(delegate
			{
				Directory.Delete(Paths.Base, recursive: true);
			});
		}
		if (!flag && !flag2 && Directory.Exists(robloxFolder))
		{
			list2.Add(delegate
			{
				Directory.Delete(robloxFolder, recursive: true);
			});
		}
		list2.Add(delegate
		{
			Registry.CurrentUser.DeleteSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Orbitstrap");
		});
		foreach (Action item4 in list2)
		{
			try
			{
				item4();
			}
			catch (Exception ex)
			{
				App.Logger.WriteLine("Installer::DoUninstall", $"Encountered exception when running cleanup sequence (#{list2.IndexOf(item4)})");
				App.Logger.WriteException("Installer::DoUninstall", ex);
			}
		}
		if (Directory.Exists(Paths.Base))
		{
			string text = ((!flag4) ? ("del /Q \"" + Paths.Application + "\"") : $"del /Q \"{Paths.Base}\\*\" && rmdir \"{Paths.Base}\"");
			Process.Start(new ProcessStartInfo
			{
				FileName = "cmd.exe",
				Arguments = "/c timeout 5 && " + text,
				UseShellExecute = true,
				WindowStyle = ProcessWindowStyle.Hidden
			});
		}
	}

	public static void HandleUpgrade()
	{
		if (!File.Exists(Paths.Application) || Paths.Process == Paths.Application)
		{
			return;
		}
		bool flag = App.LaunchSettings.UpgradeFlag.Active || Paths.Process.StartsWith(Path.Combine(Paths.Base, "Updates")) || Paths.Process.StartsWith(Path.Combine(Paths.LocalAppData, "Temp")) || Paths.Process.StartsWith(Paths.TempUpdates);
		string productVersion = FileVersionInfo.GetVersionInfo(Paths.Application).ProductVersion;
		string productVersion2 = FileVersionInfo.GetVersionInfo(Paths.Process).ProductVersion;
		if (MD5Hash.FromFile(Paths.Process) == MD5Hash.FromFile(Paths.Application) || (productVersion2 != null && productVersion != null && Utilities.CompareVersions(productVersion2, productVersion) == VersionComparison.LessThan && Frontend.ShowMessageBox(Strings.InstallChecker_VersionLessThanInstalled, MessageBoxImage.Question, MessageBoxButton.YesNo) != MessageBoxResult.Yes) || (!flag && Frontend.ShowMessageBox(Strings.InstallChecker_VersionDifferentThanInstalled, MessageBoxImage.Question, MessageBoxButton.YesNo) != MessageBoxResult.Yes))
		{
			return;
		}
		App.Logger.WriteLine("Installer::HandleUpgrade", "Doing upgrade");
		Filesystem.AssertReadOnly(Paths.Application);
		using (InterProcessLock interProcessLock = new InterProcessLock("AutoUpdater", TimeSpan.FromSeconds(5.0)))
		{
			if (!interProcessLock.IsAcquired)
			{
				App.Logger.WriteLine("Installer::HandleUpgrade", "Failed to update! (Could not obtain singleton mutex)");
				return;
			}
		}
		for (int i = 1; i <= 10; i++)
		{
			try
			{
				File.Copy(Paths.Process, Paths.Application, overwrite: true);
			}
			catch (Exception ex)
			{
				switch (i)
				{
				case 1:
					App.Logger.WriteLine("Installer::HandleUpgrade", "Waiting for write permissions to update version");
					break;
				case 10:
					App.Logger.WriteLine("Installer::HandleUpgrade", "Failed to update! (Could not get write permissions after 10 tries/5 seconds)");
					App.Logger.WriteException("Installer::HandleUpgrade", ex);
					return;
				}
				Thread.Sleep(500);
				continue;
			}
			break;
		}
		using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Orbitstrap"))
		{
			registryKey.SetValueSafe("DisplayVersion", App.Version);
			registryKey.SetValueSafe("Publisher", "Orbitstrap");
			registryKey.SetValueSafe("HelpLink", "https://github.com/4r6z/");
			registryKey.SetValueSafe("URLInfoAbout", "https://github.com/4r6z/");
			registryKey.SetValueSafe("URLUpdateInfo", "https://github.com/4r6z/");
		}
		if (productVersion != null)
		{
			if (Utilities.CompareVersions(productVersion, "2.2.0") == VersionComparison.LessThan)
			{
				string path = Path.Combine(Paths.Integrations, "rbxfpsunlocker");
				try
				{
					if (Directory.Exists(path))
					{
						Directory.Delete(path, recursive: true);
					}
				}
				catch (Exception ex2)
				{
					App.Logger.WriteException("Installer::HandleUpgrade", ex2);
				}
			}
			if (Utilities.CompareVersions(productVersion, "2.3.0") == VersionComparison.LessThan)
			{
				string path2 = Path.Combine(Paths.Modifications, "dxgi.dll");
				string path3 = Path.Combine(Paths.Modifications, "ReShade.ini");
				if (File.Exists(path2))
				{
					File.Delete(path2);
				}
				if (File.Exists(path3))
				{
					File.Delete(path3);
				}
			}
			if (Utilities.CompareVersions(productVersion, "2.5.0") == VersionComparison.LessThan)
			{
				App.FastFlags.SetValue("DFFlagDisableDPIScale", null);
				App.FastFlags.SetValue("DFFlagVariableDPIScale2", null);
			}
			if (Utilities.CompareVersions(productVersion, "2.6.0") == VersionComparison.LessThan)
			{
				if (App.Settings.Prop.UseDisableAppPatch)
				{
					try
					{
						File.Delete(Path.Combine(Paths.Modifications, "ExtraContent\\places\\Mobile.rbxl"));
					}
					catch (Exception ex3)
					{
						App.Logger.WriteException("Installer::HandleUpgrade", ex3);
					}
					App.Settings.Prop.EnableActivityTracking = true;
				}
				if (App.Settings.Prop.BootstrapperStyle == BootstrapperStyle.ClassicFluentDialog)
				{
					App.Settings.Prop.BootstrapperStyle = BootstrapperStyle.FluentDialog;
				}
				int.TryParse(App.FastFlags.GetPreset("Rendering.Framerate"), out var result);
				if (result == 0)
				{
					App.FastFlags.SetPreset("Rendering.Framerate", null);
				}
			}
			if (Utilities.CompareVersions(productVersion, "2.8.0") == VersionComparison.LessThan)
			{
				if (flag)
				{
					if (App.LaunchSettings.Args.Length == 0)
					{
						App.LaunchSettings.RobloxLaunchMode = LaunchMode.Player;
					}
					string text = App.LaunchSettings.Args.FirstOrDefault((string x) => x.Contains("roblox"));
					if (text != null)
					{
						App.LaunchSettings.RobloxLaunchMode = LaunchMode.Player;
						App.LaunchSettings.RobloxLaunchArgs = text;
					}
				}
				string text2 = Path.Combine(Paths.Desktop, "Play Roblox.lnk");
				string path4 = Path.Combine(Paths.WindowsStartMenu, "Orbitstrap");
				if (File.Exists(text2))
				{
					File.Move(text2, DesktopShortcut, overwrite: true);
				}
				if (Directory.Exists(path4))
				{
					try
					{
						Directory.Delete(path4, recursive: true);
					}
					catch (Exception ex4)
					{
						App.Logger.WriteException("Installer::HandleUpgrade", ex4);
					}
					Orbitstrap.Utility.Shortcut.Create(Paths.Application, "", StartMenuShortcut);
				}
				Registry.CurrentUser.DeleteSubKeyTree("Software\\Orbitstrap", throwOnMissingSubKey: false);
				WindowsRegistry.RegisterPlayer();
				App.FastFlags.SetValue("FFlagDisableNewIGMinDUA", null);
				App.FastFlags.SetValue("FFlagFixGraphicsQuality", null);
			}
			if (Utilities.CompareVersions(productVersion, "1.0.0.0") == VersionComparison.LessThan)
			{
				App.FastFlags.SetValue("FIntNewInGameMenuPercentRollout3", null);
				App.FastFlags.SetValue("FFlagEnableInGameMenuControls", null);
				App.FastFlags.SetValue("FFlagEnableInGameMenuModernization", null);
				App.FastFlags.SetValue("FFlagEnableInGameMenuChrome", null);
				App.FastFlags.SetValue("FFlagFixReportButtonCutOff", null);
				App.FastFlags.SetValue("FFlagEnableMenuControlsABTest", null);
				App.FastFlags.SetValue("FFlagEnableV3MenuABTest3", null);
				App.FastFlags.SetValue("FFlagEnableInGameMenuChromeABTest3", null);
				App.FastFlags.SetValue("FFlagEnableInGameMenuChromeABTest4", null);
			}
			if (Utilities.CompareVersions(productVersion, "1.0.0.0") == VersionComparison.LessThan)
			{
				string path5 = Path.Combine(Paths.Base, "Roblox");
				if (Directory.Exists(path5))
				{
					try
					{
						Directory.Delete(path5, recursive: true);
					}
					catch (Exception ex5)
					{
						App.Logger.WriteLine("Installer::HandleUpgrade", "Failed to delete the Roblox directory");
						App.Logger.WriteException("Installer::HandleUpgrade", ex5);
					}
				}
			}
			if (Utilities.CompareVersions(productVersion, "1.0.0.0") == VersionComparison.LessThan)
			{
				if (App.State.Prop.GetDeprecatedPlayer() != null)
				{
					App.RobloxState.Prop.Player = App.State.Prop.GetDeprecatedPlayer();
				}
				if (App.State.Prop.GetDeprecatedStudio() != null)
				{
					App.RobloxState.Prop.Studio = App.State.Prop.GetDeprecatedStudio();
				}
				if (App.State.Prop.GetDeprecatedModManifest() != null)
				{
					App.RobloxState.Prop.ModManifest = App.State.Prop.GetDeprecatedModManifest();
				}
			}
			App.Settings.Save();
			App.FastFlags.Save();
			App.State.Save();
			App.RobloxState.Save();
		}
		if (productVersion2 != null && !flag)
		{
			Frontend.ShowMessageBox(string.Format(Strings.InstallChecker_Updated, productVersion2), MessageBoxImage.Asterisk);
		}
	}

	public void ImportSettingsFromOrbitstrap()
	{
		if (!Directory.Exists(OrbitstrapInstallDirectory))
		{
			Frontend.ShowMessageBox(Strings.Installer_InstallationNotFound, MessageBoxImage.Exclamation);
			return;
		}
		if (Path.GetFullPath(OrbitstrapInstallDirectory).Equals(Path.GetFullPath(InstallLocation), StringComparison.OrdinalIgnoreCase))
		{
			App.Logger.WriteLine("Installer::ImportSettings", "Install location is same as import location, skipping import.");
			return;
		}
		string[] filesForImporting = FilesForImporting;
		foreach (string text in filesForImporting)
		{
			string text2 = Path.Combine(OrbitstrapInstallDirectory, text);
			if (!Directory.Exists(text2) && !File.Exists(text2))
			{
				continue;
			}
			FileAttributes attributes = File.GetAttributes(text2);
			bool flag = attributes.HasFlag(FileAttributes.Directory);
			App.Logger.WriteLine("Installer::ImportSettings", $"Found file {text2}, IsDirectory: {flag}");
			if (flag)
			{
				string text3 = Path.Combine(InstallLocation, text);
				if (Directory.Exists(text3))
				{
					App.Logger.WriteLine("Installer::ImportSettings", "Deleting existing " + text + "...");
					Directory.Delete(text3, recursive: true);
				}
				Directory.CreateDirectory(text3);
				string[] directories = Directory.GetDirectories(text2, "*", SearchOption.AllDirectories);
				for (int j = 0; j < directories.Length; j++)
				{
					Directory.CreateDirectory(directories[j].Replace(text2, text3));
				}
				directories = Directory.GetFiles(text2, "*.*", SearchOption.AllDirectories);
				foreach (string obj in directories)
				{
					File.Copy(obj, obj.Replace(text2, text3), overwrite: true);
				}
			}
			else
			{
				string destFileName = Path.Combine(InstallLocation, text);
				File.Copy(text2, destFileName, overwrite: true);
				App.Logger.WriteLine("Installer::ImportSettings", "Overridding " + text + " in InstallLocation");
			}
		}
		App.Logger.WriteLine("Installer::ImportSettings", "Importing succeded");
	}
}
