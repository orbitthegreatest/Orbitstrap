using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using System.Windows.Threading;
using Microsoft.Win32;
using Orbitstrap.Enums;
using Orbitstrap.Extensions;
using Orbitstrap.Models.APIs.GitHub;
using Orbitstrap.Models.Attributes;
using Orbitstrap.Models.Persistable;
using Orbitstrap.Models.SettingTasks.Base;
using Orbitstrap.Resources;
using Orbitstrap.UI;
using Orbitstrap.Utility;

namespace Orbitstrap;

public partial class App : Application
{
	public const string ProjectName = "Orbitstrap";

	public const string ProjectOwner = "Orbitstrap";

	public const string ProjectRepository = "Orbitstrap";

	public const string ProjectDownloadLink = "https://github.com/orbitthegreatest/Orbitstrap/releases";

	public const string ProjectHelpLink = "https://github.com/orbitthegreatest/Orbitstrap/wiki";

	public const string ProjectSupportLink = "https://github.com/orbitthegreatest/Orbitstrap/issues/new";

	public const string ProjectRemoteDataLink = "https://raw.githubusercontent.com/orbitthegreatest/Orbitstrap/refs/heads/main/Data.json";

	public const string RobloxPlayerAppName = "RobloxPlayerBeta.exe";

	public const string RobloxStudioAppName = "RobloxStudioBeta.exe";

	public const string RobloxAnselAppName = "eurotrucks2.exe";

	public const string UninstallKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Orbitstrap";

	public const string ApisKey = "Software\\Orbitstrap";

	public static BuildMetadataAttribute BuildMetadata = Assembly.GetExecutingAssembly().GetCustomAttribute<BuildMetadataAttribute>();

	public static string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

	public static readonly MD5 MD5Provider = MD5.Create();

	public static readonly Logger Logger = new Logger();

	public static readonly Dictionary<string, BaseTask> PendingSettingTasks = new Dictionary<string, BaseTask>();

	public static readonly JsonManager<Settings> Settings = new JsonManager<Settings>();

	public static readonly JsonManager<State> State = new JsonManager<State>();

	public static readonly JsonManager<RobloxState> RobloxState = new JsonManager<RobloxState>();

	public static readonly RemoteDataManager RemoteData = new RemoteDataManager();

	public static readonly FastFlagManager FastFlags = new FastFlagManager();

	public static readonly GBSEditor GlobalSettings = new GBSEditor();

	public static readonly ModInjector Injector = new ModInjector();

	public static readonly HttpClient HttpClient = new HttpClient(new HttpClientLoggingHandler(new HttpClientHandler
	{
		AutomaticDecompression = DecompressionMethods.All
	}));

	private static bool _showingExceptionDialog = false;

	public static LaunchSettings LaunchSettings { get; private set; } = null;

	public static Bootstrapper? Bootstrapper { get; set; } = null;

	public static bool IsActionBuild => BuildMetadata != null && !string.IsNullOrEmpty(BuildMetadata.CommitRef);

	public static bool IsProductionBuild
	{
		get
		{
			if (IsActionBuild)
			{
				return BuildMetadata.CommitRef.StartsWith("tag", StringComparison.Ordinal);
			}
			return false;
		}
	}

	public static bool IsStudioVisible => !string.IsNullOrEmpty(RobloxState.Prop.Studio.VersionGuid);

	public static void Terminate(ErrorCode exitCode = ErrorCode.ERROR_SUCCESS)
	{
		Logger.WriteLine("App::Terminate", $"Terminating with exit code {(int)exitCode} ({exitCode})");
		Environment.Exit((int)exitCode);
	}

	public static void SoftTerminate(ErrorCode exitCode = ErrorCode.ERROR_SUCCESS)
	{
		int exitCodeNum = (int)exitCode;
		Logger.WriteLine("App::SoftTerminate", $"Terminating with exit code {exitCodeNum} ({exitCode})");
		Application.Current.Dispatcher.Invoke(delegate
		{
			Application.Current.Shutdown(exitCodeNum);
		});
	}

	private void GlobalExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		e.Handled = true;
		try
		{
			Logger.WriteLine("App::GlobalExceptionHandler", "An exception occurred");
			FinalizeExceptionHandling(e.Exception);
		}
		catch
		{
			// Last resort — show raw message box so the error is never silent
			System.Windows.MessageBox.Show(
				"Orbitstrap encountered a fatal error and needs to close:\n\n" + e.Exception?.ToString(),
				"Orbitstrap - Fatal Error",
				System.Windows.MessageBoxButton.OK,
				System.Windows.MessageBoxImage.Error);
			Environment.Exit(1);
		}
	}

	public static void FinalizeExceptionHandling(AggregateException ex)
	{
		foreach (Exception innerException in ex.InnerExceptions)
		{
			Logger.WriteException("App::FinalizeExceptionHandling", innerException);
		}
		FinalizeExceptionHandling(ex.GetBaseException(), log: false);
	}

	public static void FinalizeExceptionHandling(Exception ex, bool log = true)
	{
		if (log)
		{
			Logger.WriteException("App::FinalizeExceptionHandling", ex);
		}
		if (_showingExceptionDialog)
		{
			return;
		}
		_showingExceptionDialog = true;
		SendLog();
		if (Bootstrapper?.Dialog != null)
		{
			if (Bootstrapper.Dialog.TaskbarProgressValue == 0.0)
			{
				Bootstrapper.Dialog.TaskbarProgressValue = 1.0;
			}
			Bootstrapper.Dialog.TaskbarProgressState = TaskbarItemProgressState.Error;
		}
		Frontend.ShowExceptionDialog(ex);
		Terminate(ErrorCode.ERROR_INSTALL_FAILURE);
	}

	public static async Task<GithubRelease?> GetLatestRelease()
	{
		try
		{
			GithubRelease githubRelease = await Http.GetJson<GithubRelease>("https://api.github.com/repos/orbitthegreatest/Orbitstrap/releases/latest");
			if (githubRelease == null || githubRelease.Assets == null)
			{
				Logger.WriteLine("App::GetLatestRelease", "Encountered invalid data");
				return null;
			}
			return githubRelease;
		}
		catch (Exception ex)
		{
			Logger.WriteException("App::GetLatestRelease", ex);
		}
		return null;
	}

	public static void SendLog()
	{
	}

	public static void AssertWindowsOSVersion()
	{
		if (Environment.OSVersion.Version.Major < 10)
		{
			Logger.WriteLine("App::AssertWindowsOSVersion", $"Detected unsupported Windows version ({Environment.OSVersion.Version}).");
			if (!LaunchSettings.QuietFlag.Active)
			{
				Frontend.ShowMessageBox(Strings.App_OSDeprecation_Win7_81, MessageBoxImage.Hand);
			}
			Terminate(ErrorCode.ERROR_INVALID_FUNCTION);
		}
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		// Register crash handlers FIRST, before anything else can throw
		AppDomain.CurrentDomain.UnhandledException += (s, ev) =>
		{
			try
			{
				if (ev.ExceptionObject is Exception ex)
					FinalizeExceptionHandling(ex);
			}
			catch
			{
				System.Windows.MessageBox.Show(
					"Orbitstrap crashed during startup:\n\n" + ev.ExceptionObject?.ToString(),
					"Orbitstrap - Startup Crash",
					System.Windows.MessageBoxButton.OK,
					System.Windows.MessageBoxImage.Error);
				Environment.Exit(1);
			}
		};

		Locale.Initialize();
		base.OnStartup(e);
		Logger.WriteLine("App::OnStartup", "Starting Orbitstrap v" + Version);
		string text = "Orbitstrap/" + Version;
		if (BuildMetadata != null)
		{
			if (IsActionBuild)
			{
				Logger.WriteLine("App::OnStartup", $"Compiled {BuildMetadata.Timestamp.ToFriendlyString()} from commit {BuildMetadata.CommitHash} ({BuildMetadata.CommitRef})");
				text = ((!IsProductionBuild) ? (text + $" (Artifact {BuildMetadata.CommitHash}, {BuildMetadata.CommitRef})") : (text + " (Production)"));
			}
			else
			{
				Logger.WriteLine("App::OnStartup", "Compiled " + BuildMetadata.Timestamp.ToFriendlyString() + " from " + BuildMetadata.Machine);
				text = text + " (Build " + Convert.ToBase64String(Encoding.UTF8.GetBytes(BuildMetadata.Machine ?? "Unknown")) + ")";
			}
		}
		else
		{
			Logger.WriteLine("App::OnStartup", "BuildMetadata not available (attribute not stamped).");
			text = text + " (Dev Build)";
		}
		Logger.WriteLine("App::OnStartup", $"OSVersion: {Environment.OSVersion}");
		Logger.WriteLine("App::OnStartup", "Loaded from " + Paths.Process);
		Logger.WriteLine("App::OnStartup", "Temp path is " + Paths.Temp);
		Logger.WriteLine("App::OnStartup", "WindowsStartMenu path is " + Paths.WindowsStartMenu);
		ApplicationConfiguration.Initialize();
		HttpClient.Timeout = TimeSpan.FromSeconds(30.0);
		HttpClient.DefaultRequestHeaders.Add("User-Agent", text);
		LaunchSettings = new LaunchSettings(e.Args);
		using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Orbitstrap");
		string text2 = null;
		bool flag = false;
		if (registryKey?.GetValue("InstallLocation") is string text3)
		{
			if (Directory.Exists(text3))
			{
				text2 = text3;
			}
			else
			{
				Match match = Regex.Match(text3, "^[a-zA-Z]:\\\\Users\\\\([^\\\\]+)", RegexOptions.IgnoreCase);
				if (match.Success)
				{
					string text4 = text3.Replace(match.Value, Paths.UserProfile, StringComparison.InvariantCultureIgnoreCase);
					if (Directory.Exists(text4))
					{
						text2 = text4;
						flag = true;
					}
				}
			}
		}
		if (text2 == null)
		{
			string text5 = Directory.GetParent(Paths.Process)?.FullName;
			if (text5 != null)
			{
				string[] array = (from x in Directory.GetFiles(text5)
					select Path.GetFileName(x)).ToArray();
				if (array.Length <= 3 && Enumerable.Contains(array, "Settings.json") && Enumerable.Contains(array, "State.json"))
				{
					text2 = text5;
					flag = true;
				}
			}
		}
		if (flag && text2 != null)
		{
			Installer installer = new Installer
			{
				InstallLocation = text2,
				IsImplicitInstall = true
			};
			if (installer.CheckInstallLocation())
			{
				Logger.WriteLine("App::OnStartup", "Changing install location to '" + text2 + "'");
				installer.DoInstall();
			}
			else
			{
				text2 = null;
			}
		}
		if (text2 == null)
		{
			Logger.Initialize(useTempDir: true);
			AssertWindowsOSVersion();
			Logger.WriteLine("App::OnStartup", "Not installed, launching the installer");
			AssertWindowsOSVersion();
			LaunchHandler.LaunchInstaller();
			return;
		}
		Paths.Initialize(text2);
		if (Paths.Process != Paths.Application && !File.Exists(Paths.Application))
		{
			File.Copy(Paths.Process, Paths.Application);
		}
		Logger.Initialize(LaunchSettings.UninstallFlag.Active);
		if (!Logger.Initialized && !Logger.NoWriteMode)
		{
			Logger.WriteLine("App::OnStartup", "Possible duplicate launch detected, terminating.");
			Terminate();
		}
		Settings.Load();
		State.Load();
		RobloxState.Load();
		FastFlags.Load();
		GlobalSettings.Load();
		Injector.Initialize();
		if (!Locale.SupportedLocales.ContainsKey(Settings.Prop.Locale))
		{
			Settings.Prop.Locale = "nil";
			Settings.Save();
		}
		Locale.Set(Settings.Prop.Locale);
		if (!LaunchSettings.BypassUpdateCheck)
		{
			Installer.HandleUpgrade();
		}
		Task.Run((Func<Task?>)RemoteData.LoadData);
		WindowsRegistry.RegisterApis();
		LaunchHandler.ProcessLaunchArgs();
	}
}
