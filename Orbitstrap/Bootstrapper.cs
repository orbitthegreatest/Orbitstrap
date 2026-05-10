using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Shell;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using Orbitstrap.AppData;
using Orbitstrap.Enums;
using Orbitstrap.Exceptions;
using Orbitstrap.Extensions;
using Orbitstrap.Models;
using Orbitstrap.Models.APIs.GitHub;
using Orbitstrap.Models.APIs.Roblox;
using Orbitstrap.Models.Manifest;
using Orbitstrap.Resources;
using Orbitstrap.RobloxInterfaces;
using Orbitstrap.UI;
using Orbitstrap.UI.Elements.Bootstrapper.Base;
using Orbitstrap.Utility;
using OrbitstrapFontFamily = Orbitstrap.Models.FontFamily;

namespace Orbitstrap;

public class Bootstrapper
{
	private const int ProgressBarMaximum = 10000;

	private const double TaskbarProgressMaximumWpf = 1.0;

	private const int TaskbarProgressMaximumWinForms = 100;

	private const string AppSettings = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<Settings>\r\n\t<ContentFolder>content</ContentFolder>\r\n\t<BaseUrl>http://www.roblox.com</BaseUrl>\r\n</Settings>\r\n";

	private const string SkyboxZipUrl = "https://github.com/KloBraticc/SkyboxPackV2/archive/refs/heads/main.zip";
	private const string SkyboxCommitApiUrl = "https://api.github.com/repos/KloBraticc/SkyboxPackV2/commits/main";
	private const string SkyboxVersionFile = "skybox.commit";

	private static readonly string PackFolder = Path.Combine(Paths.Base, "SkyboxPack");

	private static readonly HttpClient SkyboxHttpClient = new HttpClient
	{
		Timeout = TimeSpan.FromMinutes(30)
	};

	private static readonly Dictionary<string, string> SkyboxPatchFolderMap = new Dictionary<string, string>
	{
		{ "a564ec8aeef3614e788d02f0090089d8", "a5" },
		{ "7328622d2d509b95dd4dd2c721d1ca8b", "73" },
		{ "a50f6563c50ca4d5dcb255ee5cfab097", "a5" },
		{ "6c94b9385e52d221f0538aadaceead2d", "6c" },
		{ "9244e00ff9fd6cee0bb40a262bb35d31", "92" },
		{ "78cb2e93aee0cdbd79b15a866bc93a54", "78" },
	};

	private readonly FastZipEvents _fastZipEvents = new FastZipEvents();

	private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

	private IAppData AppData;

	private Dictionary<string, string> PackageDirectoryMap;

	private LaunchMode _launchMode;

	private string _launchCommandLine = App.LaunchSettings.RobloxLaunchArgs;

	private Version? _latestVersion;

	private string _latestVersionGuid;

	private string _latestVersionDirectory;

	private PackageManifest _versionPackageManifest;

	private bool _isInstalling;

	private double _progressIncrement;

	private double _taskbarProgressIncrement;

	private double _taskbarProgressMaximum;

	private long _totalDownloadedBytes;

	private bool _packageExtractionSuccess = true;

	private bool _noConnection;

	private AsyncMutex? _mutex;

	private int _appPid;

	public IBootstrapperDialog? Dialog;

	private bool _mustUpgrade
	{
		get
		{
			if (!App.LaunchSettings.ForceFlag.Active && !App.State.Prop.ForceReinstall && !string.IsNullOrEmpty(AppData.State.VersionGuid))
			{
				return !File.Exists(AppData.ExecutablePath);
			}
			return true;
		}
	}

	public bool IsStudioLaunch => _launchMode != LaunchMode.Player;

	public string MutexName { get; set; } = "Orbitstrap-Bootstrapper";

	public bool QuitIfMutexExists { get; set; }

	public Bootstrapper(LaunchMode launchMode)
	{
		_launchMode = launchMode;
		FastZipEvents fastZipEvents = _fastZipEvents;
		fastZipEvents.FileFailure = (FileFailureHandler)Delegate.Combine(fastZipEvents.FileFailure, (FileFailureHandler)delegate(object _, ScanFailureEventArgs e)
		{
			if (!e.Name.EndsWith(".ttf"))
			{
				throw e.Exception;
			}
			App.Logger.WriteLine("FastZipEvents::OnFileFailure", "Failed to extract " + e.Name);
			_packageExtractionSuccess = false;
		});
		FastZipEvents fastZipEvents2 = _fastZipEvents;
		fastZipEvents2.DirectoryFailure = (DirectoryFailureHandler)Delegate.Combine(fastZipEvents2.DirectoryFailure, (DirectoryFailureHandler)delegate(object _, ScanFailureEventArgs e)
		{
			throw e.Exception;
		});
		FastZipEvents fastZipEvents3 = _fastZipEvents;
		fastZipEvents3.ProcessFile = (ProcessFileHandler)Delegate.Combine(fastZipEvents3.ProcessFile, (ProcessFileHandler)delegate(object _, ScanEventArgs e)
		{
			e.ContinueRunning = !_cancelTokenSource.IsCancellationRequested;
		});
		SetupAppData();
	}

	private void SetupAppData()
	{
		IAppData appData2;
		if (!IsStudioLaunch)
		{
			IAppData appData = new RobloxPlayerData();
			appData2 = appData;
		}
		else
		{
			IAppData appData = new RobloxStudioData();
			appData2 = appData;
		}
		AppData = appData2;
		Deployment.BinaryType = AppData.BinaryType;
	}

	private async Task SetupPackageDictionaries()
	{
		await App.RemoteData.WaitUntilDataFetched();
		Dictionary<string, string> dictionary = App.RemoteData.Prop.PackageMaps[IsStudioLaunch ? "studio" : "player"];
		Dictionary<string, string> commonPackageMap = App.RemoteData.Prop.PackageMaps.CommonPackageMap;
		PackageDirectoryMap = new Dictionary<string, string>(commonPackageMap);
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			PackageDirectoryMap[item.Key] = item.Value;
		}
	}

	private void SetStatus(string message)
	{
		message = message.Replace("{product}", AppData.ProductName);
		if (Dialog != null)
		{
			Dialog.Message = message;
		}
	}

	private void UpdateProgressBar()
	{
		if (Dialog != null)
		{
			int value = (int)Math.Floor(_progressIncrement * (double)_totalDownloadedBytes);
			value = Math.Clamp(value, 0, 10000);
			Dialog.ProgressValue = value;
			double value2 = _taskbarProgressIncrement * (double)_totalDownloadedBytes;
			value2 = Math.Clamp(value2, 0.0, _taskbarProgressMaximum);
			Dialog.TaskbarProgressValue = value2;
		}
	}

	private void HandleConnectionError(Exception exception)
	{
		_noConnection = true;
		App.Logger.WriteLine("Bootstrapper::HandleConnectionError", "Connectivity check failed");
		App.Logger.WriteException("Bootstrapper::HandleConnectionError", exception);
		string text = Strings.Dialog_Connectivity_BadConnection;
		if (exception is AggregateException)
		{
			exception = exception.InnerException;
		}
		if (exception is HttpRequestException && exception.InnerException == null)
		{
			text = string.Format(Strings.Dialog_Connectivity_RobloxDown, "[status.roblox.com](https://status.roblox.com)");
		}
		Frontend.ShowConnectivityDialog(description: (!_mustUpgrade) ? (text + "\n\n" + Strings.Dialog_Connectivity_RobloxUpgradeSkip) : (text + "\n\n" + Strings.Dialog_Connectivity_RobloxUpgradeNeeded + "\n\n" + Strings.Dialog_Connectivity_TryAgainLater), title: string.Format(Strings.Dialog_Connectivity_UnableToConnect, "Roblox"), image: _mustUpgrade ? MessageBoxImage.Hand : MessageBoxImage.Exclamation, exception: exception);
		if (_mustUpgrade)
		{
			App.Terminate(ErrorCode.ERROR_CANCELLED);
		}
	}

	public async Task Run()
	{
		App.Logger.WriteLine("Bootstrapper::Run", "Running bootstrapper");
		if (Dialog != null)
		{
			Dialog.CancelEnabled = true;
		}
		SetStatus(Strings.Bootstrapper_Status_Connecting);
		Exception ex = await Deployment.InitializeConnectivity();
		App.Logger.WriteLine("Bootstrapper::Run", "Connectivity check finished");
		if (ex != null)
		{
			HandleConnectionError(ex);
		}
		if (App.Settings.Prop.CheckForUpdates && !App.LaunchSettings.UpgradeFlag.Active && await CheckForUpdates())
		{
			return;
		}
		bool mutexExists = Utilities.DoesMutexExist(MutexName);
		if (mutexExists)
		{
			if (QuitIfMutexExists)
			{
				App.Logger.WriteLine("Bootstrapper::Run", MutexName + " mutex exists, exiting!");
				return;
			}
			App.Logger.WriteLine("Bootstrapper::Run", MutexName + " mutex exists, waiting...");
			SetStatus(Strings.Bootstrapper_Status_WaitingOtherInstances);
		}
		await using AsyncMutex mutex = new AsyncMutex(initiallyOwned: false, MutexName);
		await mutex.AcquireAsync(_cancelTokenSource.Token);
		_mutex = mutex;
		if (mutexExists)
		{
			App.Settings.Load();
			App.State.Load();
			App.RobloxState.Load();
		}
		if (!_noConnection)
		{
			try
			{
				await GetLatestVersionInfo();
			}
			catch (Exception exception)
			{
				HandleConnectionError(exception);
			}
		}
		CleanupVersionsFolder();
		bool allModificationsApplied = true;
		if (!_noConnection)
		{
			if (App.RemoteData.LoadedState == GenericTriState.Unknown)
			{
				SetStatus(Strings.Bootstrapper_Status_WaitingForData);
			}
			await SetupPackageDictionaries();
			if (File.Exists(Path.Combine(AppData.Directory, "eurotrucks2.exe")))
			{
				Frontend.ShowMessageBox(Strings.Bootstrapper_Dialog_AnselDisabled, MessageBoxImage.Exclamation);
				await UpgradeRoblox();
			}
			if (AppData.State.VersionGuid != _latestVersionGuid || _mustUpgrade)
			{
				bool flag = Utilities.DoesMutexExist("Orbitstrap-BackgroundUpdater");
				if (App.LaunchSettings.BackgroundUpdaterFlag.Active)
				{
					flag = false;
				}
				App.Logger.WriteLine("Bootstrapper::Run", $"Background updater running: {flag}");
				if (flag && _mustUpgrade)
				{
					Utilities.KillBackgroundUpdater();
					flag = false;
				}
				if (!flag)
				{
					if (!IsEligibleForBackgroundUpdate())
					{
						await UpgradeRoblox();
					}
					else
					{
						StartBackgroundUpdater();
					}
				}
			}
			if (_cancelTokenSource.IsCancellationRequested)
			{
				return;
			}
			allModificationsApplied = await ApplyModifications();
		}
		if (IsStudioLaunch)
		{
			WindowsRegistry.RegisterStudio();
		}
		else
		{
			WindowsRegistry.RegisterPlayer();
		}
		WindowsRegistry.RegisterClientLocation(IsStudioLaunch, _latestVersionDirectory);
		if (_launchMode != LaunchMode.Player)
		{
			await mutex.ReleaseAsync();
		}
		if (!App.LaunchSettings.NoLaunchFlag.Active && !_cancelTokenSource.IsCancellationRequested)
		{
			if (!App.LaunchSettings.QuietFlag.Active)
			{
				if (!_packageExtractionSuccess)
				{
					Frontend.ShowBalloonTip(Strings.Bootstrapper_ExtractionFailed_Title, Strings.Bootstrapper_ExtractionFailed_Message, ToolTipIcon.Warning);
				}
				else if (!allModificationsApplied)
				{
					Frontend.ShowBalloonTip(Strings.Bootstrapper_ModificationsFailed_Title, Strings.Bootstrapper_ModificationsFailed_Message, ToolTipIcon.Warning);
				}
			}
			StartRoblox();
		}
		await mutex.ReleaseAsync();
		Dialog?.CloseBootstrapper();
	}

	private async Task GetLatestVersionInfo()
	{
		using (RegistryKey key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\ROBLOX Corporation\\Environments\\" + AppData.RegistryName + "\\Channel"))
		{
			Match match = Regex.Match(App.LaunchSettings.RobloxLaunchArgs, "channel:([a-zA-Z0-9-_]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			bool num = App.LaunchSettings.ChannelFlag.Active && !string.IsNullOrEmpty(App.LaunchSettings.ChannelFlag.Data);
			string channel = ((match.Groups.Count == 2) ? match.Groups[1].Value.ToLowerInvariant() : "production");
			if (!num)
			{
				switch (App.Settings.Prop.ChannelChangeMode)
				{
				case ChannelChangeMode.Automatic:
					App.Logger.WriteLine("Bootstrapper::GetLatestVersionInfo", "Enrolling into channel");
					EnrollChannel(channel);
					break;
				case ChannelChangeMode.Prompt:
				{
					App.Logger.WriteLine("Bootstrapper::GetLatestVersionInfo", "Prompting channel enrollment");
					if (!match.Success || match.Groups.Count != 2 || match.Groups[1].Value.ToLowerInvariant() == Deployment.Channel)
					{
						App.Logger.WriteLine("Bootstrapper::GetLatestVersionInfo", "Channel is either equal or incorrectly formatted");
						break;
					}
					string arg = ((!string.IsNullOrEmpty(match.Groups[1].Value)) ? match.Groups[1].Value : "production");
					if (Frontend.ShowMessageBox(string.Format(Strings.Bootstrapper_Bootstrapper_Dialog_PromptChannelChange, arg, App.Settings.Prop.Channel), MessageBoxImage.Question, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
					{
						EnrollChannel(channel);
					}
					break;
				}
				case ChannelChangeMode.Ignore:
					App.Logger.WriteLine("Bootstrapper::GetLatestVersionInfo", "Ignoring channel enrollment");
					break;
				}
			}
			else
			{
				string data = App.LaunchSettings.ChannelFlag.Data;
				if (!string.IsNullOrEmpty(data))
				{
					App.Logger.WriteLine("Bootstrapper::GetLatestVersionInfo", "Forcing channel " + data);
					EnrollChannel(data);
				}
			}
			if (!App.LaunchSettings.VersionFlag.Active || string.IsNullOrEmpty(App.LaunchSettings.VersionFlag.Data))
			{
				ClientVersion clientVersion;
				try
				{
					clientVersion = await Deployment.GetInfo(Deployment.Channel);
				}
				catch (InvalidChannelException ex)
				{
					if (ex.StatusCode == HttpStatusCode.NotFound)
					{
						App.Logger.WriteLine("Bootstrapper::GetLatestVersionInfo", "Reverting enrolled channel to production because a WindowsPlayer build does not exist for " + App.Settings.Prop.Channel);
					}
					else
					{
						if (ex.StatusCode != HttpStatusCode.Unauthorized)
						{
							throw;
						}
						App.Logger.WriteLine("Bootstrapper::GetLatestVersionInfo", $"Reverting enrolled channel to {"production"} because {App.Settings.Prop.Channel} is restricted for public use.");
						if (App.Settings.Prop.ChannelChangeMode != ChannelChangeMode.Automatic)
						{
							Frontend.ShowMessageBox(string.Format(Strings.Boostrapper_Dialog_UnauthorizedChannel, Deployment.Channel, "production"), MessageBoxImage.Asterisk);
						}
					}
					RevertChannel();
					clientVersion = await Deployment.GetInfo(Deployment.Channel);
				}
				if (clientVersion.IsBehindDefaultChannel)
				{
					if (App.Settings.Prop.ChannelChangeMode switch
					{
						ChannelChangeMode.Prompt => (int)Frontend.ShowMessageBox(string.Format(Strings.Bootstrapper_Dialog_ChannelOutOfDate, Deployment.Channel, "production"), MessageBoxImage.Exclamation, MessageBoxButton.YesNo), 
						ChannelChangeMode.Automatic => 6, 
						ChannelChangeMode.Ignore => 7, 
						_ => 0, 
					} == 6)
					{
						App.Logger.WriteLine("Bootstrapper::CheckLatestVersion", "Changed Roblox channel from " + App.Settings.Prop.Channel + " to production");
						RevertChannel();
						await Deployment.GetInfo(Deployment.Channel);
					}
					RevertChannel();
					clientVersion = await Deployment.GetInfo();
				}
				key.SetValueSafe("www.roblox.com", Deployment.IsDefaultChannel ? "" : Deployment.Channel);
				_latestVersionGuid = clientVersion.VersionGuid;
				_latestVersion = Utilities.ParseVersionSafe(clientVersion.Version);
			}
			else
			{
				App.Logger.WriteLine("Bootstrapper::GetLatestVersionInfo", "Version set to " + App.LaunchSettings.VersionFlag.Data + " from arguments");
				_latestVersionGuid = App.LaunchSettings.VersionFlag.Data;
			}
			_latestVersionDirectory = Path.Combine(Paths.Versions, _latestVersionGuid);
			string location = Deployment.GetLocation("/" + _latestVersionGuid + "-rbxPkgManifest.txt");
			_versionPackageManifest = new PackageManifest(await App.HttpClient.GetStringAsync(location));
			if (_launchMode == LaunchMode.Unknown)
			{
				App.Logger.WriteLine("Bootstrapper::GetLatestVersionInfo", "Identifying launch mode from package manifest");
				bool flag = _versionPackageManifest.Exists((Package x) => x.Name == "RobloxApp.zip");
				App.Logger.WriteLine("Bootstrapper::GetLatestVersionInfo", $"isPlayer: {flag}");
				_launchMode = (flag ? LaunchMode.Player : LaunchMode.Studio);
				SetupAppData();
			}
		}
		static void EnrollChannel(string Channel = "production")
		{
			Deployment.Channel = Channel;
			App.Settings.Prop.Channel = Channel;
			App.Settings.Save();
		}
		static void RevertChannel()
		{
			Deployment.Channel = "production";
			App.Settings.Prop.Channel = "production";
			App.Settings.Save();
		}
	}

	private bool IsEligibleForBackgroundUpdate()
	{
		if (App.LaunchSettings.BackgroundUpdaterFlag.Active)
		{
			App.Logger.WriteLine("Bootstrapper::IsEligibleForBackgroundUpdate", "Not eligible: Is the background updater process");
			return false;
		}
		if (!App.Settings.Prop.BackgroundUpdatesEnabled)
		{
			App.Logger.WriteLine("Bootstrapper::IsEligibleForBackgroundUpdate", "Not eligible: Background updates disabled");
			return false;
		}
		if (IsStudioLaunch)
		{
			App.Logger.WriteLine("Bootstrapper::IsEligibleForBackgroundUpdate", "Not eligible: Studio launch");
			return false;
		}
		if (_mustUpgrade)
		{
			App.Logger.WriteLine("Bootstrapper::IsEligibleForBackgroundUpdate", "Not eligible: Must upgrade is true");
			return false;
		}
		long freeDiskSpace = Filesystem.GetFreeDiskSpace(Paths.Base);
		if (freeDiskSpace < 3000000000u)
		{
			App.Logger.WriteLine("Bootstrapper::IsEligibleForBackgroundUpdate", $"Not eligible: User has {freeDiskSpace} free space, at least {3000000000L} is required");
			return false;
		}
		if (_latestVersion == null)
		{
			App.Logger.WriteLine("Bootstrapper::IsEligibleForBackgroundUpdate", "Not eligible: Latest version is undefined");
			return false;
		}
		Version robloxVersion = Utilities.GetRobloxVersion(AppData);
		if (robloxVersion == null)
		{
			App.Logger.WriteLine("Bootstrapper::IsEligibleForBackgroundUpdate", "Not eligible: Current version is undefined");
			return false;
		}
		if (robloxVersion.Minor > _latestVersion.Minor)
		{
			App.Logger.WriteLine("Bootstrapper::IsEligibleForBackgroundUpdate", "Not eligible: Downgrade");
			return false;
		}
		int num = _latestVersion.Minor - robloxVersion.Minor;
		if (num == 0 || num == 1)
		{
			App.Logger.WriteLine("Bootstrapper::IsEligibleForBackgroundUpdate", "Eligible");
			return true;
		}
		App.Logger.WriteLine("Bootstrapper::IsEligibleForBackgroundUpdate", $"Not eligible: Major version diff is {num}");
		return false;
	}

	private static void LaunchMultiInstanceWatcher()
	{
		if (Utilities.DoesMutexExist("ROBLOX_singletonMutex"))
		{
			App.Logger.WriteLine("Bootstrapper::LaunchMultiInstanceWatcher", "Roblox singleton mutex already exists");
			return;
		}
		using EventWaitHandle eventWaitHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, "Orbitstrap-MultiInstanceWatcherInitialisationFinished");
		Process.Start(Paths.Process, "-multiinstancewatcher");
		if (eventWaitHandle.WaitOne(TimeSpan.FromSeconds(2.0)))
		{
			App.Logger.WriteLine("Bootstrapper::LaunchMultiInstanceWatcher", "Initialisation finished signalled, continuing.");
		}
		else
		{
			App.Logger.WriteLine("Bootstrapper::LaunchMultiInstanceWatcher", "Did not receive the initialisation finished signal, continuing.");
		}
	}

	private async void StartRoblox()
	{
		SetStatus(Strings.Bootstrapper_Status_Starting);
		if (_launchMode == LaunchMode.Player)
		{
			if (App.Settings.Prop.MultiInstanceLaunching)
			{
				LaunchMultiInstanceWatcher();
			}
			if (App.Settings.Prop.ForceRobloxLanguage)
			{
				Match match = Regex.Match(_launchCommandLine, "gameLocale:([a-z_]+)", RegexOptions.CultureInvariant);
				if (match.Groups.Count == 2)
				{
					_launchCommandLine = _launchCommandLine.Replace("robloxLocale:en_us", "robloxLocale:" + match.Groups[1].Value, StringComparison.OrdinalIgnoreCase);
				}
			}
		}
		string[] obj = new string[3] { "RobloxPlayerBeta.exe", "eurotrucks2.exe", "RobloxStudioBeta.exe" };
		string ResolvedName = null;
		string[] array = obj;
		foreach (string text in array)
		{
			if (File.Exists(Path.Combine(AppData.Directory, text)))
			{
				ResolvedName = text;
			}
		}
		if (string.IsNullOrEmpty(ResolvedName))
		{
			await UpgradeRoblox();
		}
		ProcessStartInfo processStartInfo = new ProcessStartInfo
		{
			FileName = Path.Combine(AppData.Directory, ResolvedName),
			Arguments = _launchCommandLine,
			WorkingDirectory = AppData.Directory
		};
		if (_launchMode == LaunchMode.Player && ShouldRunAsAdmin())
		{
			processStartInfo.Verb = "runas";
			processStartInfo.UseShellExecute = true;
		}
		else if (_launchMode == LaunchMode.StudioAuth)
		{
			Process.Start(processStartInfo);
			return;
		}
		string logFileName = null;
		string text2 = Path.Combine(Paths.LocalAppData, "Roblox");
		if (!Directory.Exists(text2))
		{
			Directory.CreateDirectory(text2);
		}
		string path = Path.Combine(text2, "logs");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		FileSystemWatcher logWatcher = new FileSystemWatcher
		{
			Path = path,
			Filter = "*.log",
			EnableRaisingEvents = true
		};
		AutoResetEvent logCreatedEvent = new AutoResetEvent(initialState: false);
		logWatcher.Created += delegate(object _, FileSystemEventArgs e)
		{
			logWatcher.EnableRaisingEvents = false;
			logFileName = e.FullPath;
			logCreatedEvent.Set();
		};
		List<int> list = new List<int>();
		foreach (CustomIntegration customIntegration in App.Settings.Prop.CustomIntegrations)
		{
			if (customIntegration != null && customIntegration.PreLaunch)
			{
				App.Logger.WriteLine("Bootstrapper::StartRoblox", $"Pre-Launching custom integration '{customIntegration.Name}' ({customIntegration.Location} {customIntegration.LaunchArgs} - autoclose is {customIntegration.AutoClose})");
				int num = 0;
				try
				{
					num = Process.Start(new ProcessStartInfo
					{
						FileName = customIntegration.Location,
						Arguments = customIntegration.LaunchArgs.Replace("\r\n", " "),
						WorkingDirectory = Path.GetDirectoryName(customIntegration.Location),
						UseShellExecute = true
					}).Id;
				}
				catch (Exception ex)
				{
					App.Logger.WriteLine("Bootstrapper::StartRoblox", "Failed to pre-launch integration '" + customIntegration.Name + "'!");
					App.Logger.WriteLine("Bootstrapper::StartRoblox", ex.Message);
				}
				if (customIntegration != null && customIntegration.AutoClose && num != 0)
				{
					list.Add(num);
				}
				if (customIntegration != null)
				{
					_ = customIntegration.Delay;
					Thread.Sleep(customIntegration.Delay);
				}
			}
		}
		try
		{
			using Process process = Process.Start(processStartInfo);
			_appPid = process.Id;
		}
		catch (Win32Exception ex2) when (ex2.NativeErrorCode == 1223)
		{
			return;
		}
		catch (Exception)
		{
			File.Delete(AppData.ExecutablePath);
			throw;
		}
		App.Logger.WriteLine("Bootstrapper::StartRoblox", $"Started Roblox (PID {_appPid}), waiting for log file");
		logCreatedEvent.WaitOne(TimeSpan.FromSeconds(15.0));
		if (string.IsNullOrEmpty(logFileName))
		{
			App.Logger.WriteLine("Bootstrapper::StartRoblox", "Unable to identify log file");
			return;
		}
		App.Logger.WriteLine("Bootstrapper::StartRoblox", "Got log file as " + logFileName);
		_mutex?.ReleaseAsync();
		if (IsStudioLaunch)
		{
			return;
		}
		foreach (CustomIntegration customIntegration2 in App.Settings.Prop.CustomIntegrations)
		{
			if (customIntegration2 != null && (customIntegration2 == null || !customIntegration2.PreLaunch))
			{
				App.Logger.WriteLine("Bootstrapper::StartRoblox", $"Launching custom integration '{customIntegration2.Name}' ({customIntegration2.Location} {customIntegration2?.LaunchArgs} - autoclose is {customIntegration2.AutoClose})");
				int num2 = 0;
				try
				{
					num2 = Process.Start(new ProcessStartInfo
					{
						FileName = customIntegration2.Location,
						Arguments = customIntegration2.LaunchArgs.Replace("\r\n", " "),
						WorkingDirectory = Path.GetDirectoryName(customIntegration2.Location),
						UseShellExecute = true
					}).Id;
				}
				catch (Exception ex4)
				{
					App.Logger.WriteLine("Bootstrapper::StartRoblox", "Failed to launch integration '" + customIntegration2.Name + "'!");
					App.Logger.WriteLine("Bootstrapper::StartRoblox", ex4.Message);
				}
				if (customIntegration2.AutoClose && num2 != 0)
				{
					list.Add(num2);
				}
			}
		}
		if (App.Settings.Prop.EnableActivityTracking || App.LaunchSettings.TestModeFlag.Active || list.Any())
		{
			using InterProcessLock interProcessLock = new InterProcessLock("Watcher", TimeSpan.FromSeconds(5.0));
			WatcherData value = new WatcherData
			{
				ProcessId = _appPid,
				LogFile = logFileName,
				AutoclosePids = list
			};
			string text3 = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value)));
			string text4 = "-watcher \"" + text3 + "\"";
			if (App.LaunchSettings.TestModeFlag.Active)
			{
				text4 += " -testmode";
			}
			_ = interProcessLock.IsAcquired;
			Process.Start(Paths.Process, text4);
		}
		Thread.Sleep(1000);
	}

	private bool ShouldRunAsAdmin()
	{
		foreach (RegistryKey root in WindowsRegistry.Roots)
		{
			using RegistryKey registryKey = root.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers");
			if (registryKey != null)
			{
				string text = (string)registryKey.GetValue(AppData.ExecutablePath);
				if (text != null && text.Contains("RUNASADMIN", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Cancel()
	{
		if (_cancelTokenSource.IsCancellationRequested)
		{
			return;
		}
		App.Logger.WriteLine("Bootstrapper::Cancel", "Cancelling launch...");
		_cancelTokenSource.Cancel();
		if (Dialog != null)
		{
			Dialog.CancelEnabled = false;
		}
		if (_isInstalling)
		{
			try
			{
				WindowsRegistry.RegisterClientLocation(IsStudioLaunch, null);
				if (Directory.Exists(_latestVersionDirectory))
				{
					Directory.Delete(_latestVersionDirectory, recursive: true);
				}
			}
			catch (Exception ex)
			{
				App.Logger.WriteLine("Bootstrapper::Cancel", "Could not fully clean up installation!");
				App.Logger.WriteException("Bootstrapper::Cancel", ex);
			}
		}
		else if (_appPid != 0)
		{
			try
			{
				using Process process = Process.GetProcessById(_appPid);
				process.Kill();
			}
			catch (Exception)
			{
			}
		}
		Dialog?.CloseBootstrapper();
		App.SoftTerminate(ErrorCode.ERROR_CANCELLED);
	}

	private async Task<bool> CheckForUpdates()
	{
		if (Process.GetProcessesByName("Orbitstrap").Length > 1)
		{
			App.Logger.WriteLine("Bootstrapper::CheckForUpdates", "More than one Orbitstrap instance running, aborting update check");
			return false;
		}
		App.Logger.WriteLine("Bootstrapper::CheckForUpdates", "Checking for updates...");
		GithubRelease githubRelease = await App.GetLatestRelease();
		if (githubRelease == null)
		{
			return false;
		}
		VersionComparison versionComparison = Utilities.CompareVersions(App.Version, githubRelease.TagName);
		if ((App.IsProductionBuild && versionComparison == VersionComparison.Equal) || versionComparison == VersionComparison.GreaterThan)
		{
			App.Logger.WriteLine("Bootstrapper::CheckForUpdates", "No updates found");
			return false;
		}
		if (Dialog != null)
		{
			Dialog.CancelEnabled = false;
		}
		string version = githubRelease.TagName;
		SetStatus(Strings.Bootstrapper_Status_UpgradingOrbitstrap);
		try
		{
			GithubReleaseAsset githubReleaseAsset = githubRelease.Assets[0];
			string downloadLocation = Path.Combine(Paths.TempUpdates, githubReleaseAsset.Name);
			Directory.CreateDirectory(Paths.TempUpdates);
			App.Logger.WriteLine("Bootstrapper::CheckForUpdates", "Downloading " + githubRelease.TagName + "...");
			if (!File.Exists(downloadLocation))
			{
				HttpResponseMessage httpResponseMessage = await App.HttpClient.GetAsync(githubReleaseAsset.BrowserDownloadUrl);
				await using FileStream fileStream = new FileStream(downloadLocation, FileMode.OpenOrCreate, FileAccess.Write);
				await httpResponseMessage.Content.CopyToAsync(fileStream);
			}
			App.Logger.WriteLine("Bootstrapper::CheckForUpdates", "Starting " + version + "...");
			ProcessStartInfo processStartInfo = new ProcessStartInfo
			{
				FileName = downloadLocation
			};
			processStartInfo.ArgumentList.Add("-upgrade");
			string[] args = App.LaunchSettings.Args;
			foreach (string item in args)
			{
				processStartInfo.ArgumentList.Add(item);
			}
			if (_launchMode == LaunchMode.Player && !processStartInfo.ArgumentList.Contains("-player"))
			{
				processStartInfo.ArgumentList.Add("-player");
			}
			else if (_launchMode == LaunchMode.Studio && !processStartInfo.ArgumentList.Contains("-studio"))
			{
				processStartInfo.ArgumentList.Add("-studio");
			}
			App.Settings.Save();
			new InterProcessLock("AutoUpdater");
			Process.Start(processStartInfo);
			return true;
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("Bootstrapper::CheckForUpdates", "An exception occurred when running the auto-updater");
			App.Logger.WriteException("Bootstrapper::CheckForUpdates", ex);
			Frontend.ShowMessageBox(string.Format(Strings.Bootstrapper_AutoUpdateFailed, version), MessageBoxImage.Asterisk);
			Utilities.ShellExecute("https://github.com/4r6z/");
		}
		return false;
	}

	private static bool TryDeleteRobloxInDirectory(string dir)
	{
		string path = Path.Combine(dir, "RobloxPlayerBeta.exe");
		if (!File.Exists(dir))
		{
			path = Path.Combine(dir, "RobloxStudioBeta.exe");
			if (!File.Exists(dir))
			{
				return true;
			}
		}
		try
		{
			File.Delete(path);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static void CleanupVersionsFolder()
	{
		if (App.LaunchSettings.BackgroundUpdaterFlag.Active)
		{
			App.Logger.WriteLine("Bootstrapper::CleanupVersionsFolder", "Background updater tried to cleanup, stopping!");
			return;
		}
		if (!Directory.Exists(Paths.Versions))
		{
			App.Logger.WriteLine("Bootstrapper::CleanupVersionsFolder", "Versions directory does not exist, skipping cleanup.");
			return;
		}
		string[] directories = Directory.GetDirectories(Paths.Versions);
		foreach (string text in directories)
		{
			string fileName = Path.GetFileName(text);
			if (fileName != App.RobloxState.Prop.Player.VersionGuid && fileName != App.RobloxState.Prop.Studio.VersionGuid && TryDeleteRobloxInDirectory(text))
			{
				try
				{
					Directory.Delete(text, recursive: true);
				}
				catch (IOException ex)
				{
					App.Logger.WriteLine("Bootstrapper::CleanupVersionsFolder", "Failed to delete " + text);
					App.Logger.WriteException("Bootstrapper::CleanupVersionsFolder", ex);
				}
			}
		}
	}

	private void MigrateCompatibilityFlags()
	{
		string text = Path.Combine(Paths.Versions, AppData.State.VersionGuid, AppData.ExecutableName);
		string text2 = Path.Combine(_latestVersionDirectory, AppData.ExecutableName);
		using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers");
		if (registryKey.GetValue(text) is string value)
		{
			App.Logger.WriteLine("Bootstrapper::MigrateCompatibilityFlags", $"Migrating app compatibility flags from {text} to {text2}...");
			registryKey.SetValueSafe(text2, value);
			registryKey.DeleteValueSafe(text);
		}
	}

	private static void KillRobloxPlayers()
	{
		List<Process> list = new List<Process>();
		list.AddRange(Process.GetProcessesByName("RobloxPlayerBeta"));
		list.AddRange(Process.GetProcessesByName("eurotrucks2"));
		list.AddRange(Process.GetProcessesByName("RobloxCrashHandler"));
		foreach (Process item in list)
		{
			try
			{
				item.Kill();
			}
			catch (Exception ex)
			{
				App.Logger.WriteLine("Bootstrapper::KillRobloxPlayers", $"Failed to close process {item.Id}");
				App.Logger.WriteException("Bootstrapper::KillRobloxPlayers", ex);
			}
		}
	}

	private async Task UpgradeRoblox()
	{
		bool flag = !App.Settings.Prop.UpdateRoblox;
		if (flag)
		{
			SetStatus(Strings.Bootstrapper_Status_CancelUpgrade);
			App.Logger.WriteLine("Bootstrapper::UpgradeRoblox", "Upgrading disabled, cancelling the upgrade.");
			Thread.Sleep(2000);
		}
		if (flag && !Directory.Exists(_latestVersionDirectory))
		{
			Frontend.ShowMessageBox(Strings.Bootstrapper_Dialog_NoUpgradeWithoutClient, MessageBoxImage.Exclamation);
		}
		else if (flag)
		{
			return;
		}
		if (string.IsNullOrEmpty(AppData.State.VersionGuid))
		{
			SetStatus(Strings.Bootstrapper_Status_Installing);
		}
		else
		{
			SetStatus(Strings.Bootstrapper_Status_Upgrading);
		}
		Directory.CreateDirectory(Paths.Base);
		Directory.CreateDirectory(Paths.Downloads);
		Directory.CreateDirectory(Paths.Versions);
		_isInstalling = true;
		if (!App.LaunchSettings.BackgroundUpdaterFlag.Active && !IsStudioLaunch)
		{
			KillRobloxPlayers();
		}
		if (!App.LaunchSettings.BackgroundUpdaterFlag.Active && Directory.Exists(_latestVersionDirectory))
		{
			try
			{
				Directory.Delete(_latestVersionDirectory, recursive: true);
			}
			catch (Exception ex)
			{
				App.Logger.WriteLine("Bootstrapper::UpgradeRoblox", "Failed to delete the latest version directory");
				App.Logger.WriteException("Bootstrapper::UpgradeRoblox", ex);
			}
		}
		Directory.CreateDirectory(_latestVersionDirectory);
		IEnumerable<string> cachedPackageHashes = from x in Directory.GetFiles(Paths.Downloads)
			select Path.GetFileName(x);
		int num = 0;
		num += _versionPackageManifest.Where((Package x) => !cachedPackageHashes.Contains(x.Signature)).Sum((Package x) => x.PackedSize);
		num += _versionPackageManifest.Sum((Package x) => x.Size);
		if (Filesystem.GetFreeDiskSpace(Paths.Base) < num)
		{
			Frontend.ShowMessageBox(Strings.Bootstrapper_NotEnoughSpace, MessageBoxImage.Hand);
			App.Terminate(ErrorCode.ERROR_INSTALL_FAILURE);
			return;
		}
		if (Dialog != null)
		{
			Dialog.ProgressStyle = ProgressBarStyle.Continuous;
			Dialog.TaskbarProgressState = TaskbarItemProgressState.Normal;
			Dialog.ProgressMaximum = 10000;
			int num2 = _versionPackageManifest.Sum((Package package3) => package3.PackedSize);
			_progressIncrement = 10000.0 / (double)num2;
			if (Dialog is WinFormsDialogBase)
			{
				_taskbarProgressMaximum = 100.0;
			}
			else
			{
				_taskbarProgressMaximum = 1.0;
			}
			_taskbarProgressIncrement = _taskbarProgressMaximum / (double)num2;
		}
		List<Task> extractionTasks = new List<Task>();
		foreach (Package package in _versionPackageManifest)
		{
			if (_cancelTokenSource.IsCancellationRequested)
			{
				return;
			}
			await DownloadPackage(package);
			if (!(package.Name == "WebView2RuntimeInstaller.zip"))
			{
				extractionTasks.Add(Task.Run(delegate
				{
					ExtractPackage(package);
				}, _cancelTokenSource.Token));
			}
		}
		if (_cancelTokenSource.IsCancellationRequested)
		{
			return;
		}
		if (Dialog != null)
		{
			Dialog.ProgressStyle = ProgressBarStyle.Marquee;
			Dialog.TaskbarProgressState = TaskbarItemProgressState.Indeterminate;
			SetStatus(Strings.Bootstrapper_Status_Configuring);
		}
		await Task.WhenAll(extractionTasks);
		App.Logger.WriteLine("Bootstrapper::UpgradeRoblox", "Writing AppSettings.xml...");
		await File.WriteAllTextAsync(Path.Combine(_latestVersionDirectory, "AppSettings.xml"), "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<Settings>\r\n\t<ContentFolder>content</ContentFolder>\r\n\t<BaseUrl>http://www.roblox.com</BaseUrl>\r\n</Settings>\r\n");
		if (_cancelTokenSource.IsCancellationRequested)
		{
			return;
		}
		if (App.State.Prop.PromptWebView2Install)
		{
			using RegistryKey hklmKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
			using RegistryKey hkcuKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
			if (hklmKey != null || hkcuKey != null)
			{
				App.State.Prop.PromptWebView2Install = true;
			}
			else if (Frontend.ShowMessageBox(Strings.Bootstrapper_WebView2NotFound, MessageBoxImage.Exclamation, MessageBoxButton.YesNo, MessageBoxResult.Yes) != MessageBoxResult.Yes)
			{
				App.State.Prop.PromptWebView2Install = false;
			}
			else
			{
				App.Logger.WriteLine("Bootstrapper::UpgradeRoblox", "Installing WebView2 runtime...");
				Package package2 = _versionPackageManifest.Find((Package x) => x.Name == "WebView2RuntimeInstaller.zip");
				if (package2 == null)
				{
					App.Logger.WriteLine("Bootstrapper::UpgradeRoblox", "Aborted runtime install because package does not exist, has WebView2 been added in this Roblox version yet?");
					return;
				}
				string baseDirectory = Path.Combine(_latestVersionDirectory, PackageDirectoryMap[package2.Name]);
				ExtractPackage(package2);
				SetStatus(Strings.Bootstrapper_Status_InstallingWebView2);
				await Process.Start(new ProcessStartInfo
				{
					WorkingDirectory = baseDirectory,
					FileName = Path.Combine(baseDirectory, "MicrosoftEdgeWebview2Setup.exe"),
					Arguments = "/silent /install"
				}).WaitForExitAsync();
				App.Logger.WriteLine("Bootstrapper::UpgradeRoblox", "Finished installing runtime");
				Directory.Delete(baseDirectory, recursive: true);
			}
		}
		MigrateCompatibilityFlags();
		AppData.State.VersionGuid = _latestVersionGuid;
		AppData.State.PackageHashes.Clear();
		foreach (Package item in _versionPackageManifest)
		{
			AppData.State.PackageHashes.Add(item.Name, item.Signature);
		}
		CleanupVersionsFolder();
		List<string> list = new List<string>();
		list.AddRange(App.RobloxState.Prop.Player.PackageHashes.Values);
		list.AddRange(App.RobloxState.Prop.Studio.PackageHashes.Values);
		if (!App.Settings.Prop.DebugDisableVersionPackageCleanup)
		{
			foreach (string item2 in cachedPackageHashes)
			{
				if (!list.Contains(item2))
				{
					App.Logger.WriteLine("Bootstrapper::UpgradeRoblox", "Deleting unused package " + item2);
					try
					{
						File.Delete(Path.Combine(Paths.Downloads, item2));
					}
					catch (Exception ex2)
					{
						App.Logger.WriteLine("Bootstrapper::UpgradeRoblox", "Failed to delete " + item2 + "!");
						App.Logger.WriteException("Bootstrapper::UpgradeRoblox", ex2);
					}
				}
			}
		}
		App.Logger.WriteLine("Bootstrapper::UpgradeRoblox", "Registering approximate program size...");
		int size = _versionPackageManifest.Sum((Package x) => x.Size + x.PackedSize) / 1024;
		AppData.State.Size = size;
		int num3 = App.RobloxState.Prop.Player.Size + App.RobloxState.Prop.Studio.Size;
		using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Orbitstrap"))
		{
			registryKey.SetValueSafe("EstimatedSize", num3);
		}
		WindowsRegistry.RegisterClientLocation(IsStudioLaunch, _latestVersionDirectory);
		App.Logger.WriteLine("Bootstrapper::UpgradeRoblox", $"Registered as {num3} KB");
		App.State.Prop.ForceReinstall = false;
		App.State.Save();
		App.RobloxState.Save();
		_isInstalling = false;
	}

	private static void StartBackgroundUpdater()
	{
		if (Utilities.DoesMutexExist("Orbitstrap-BackgroundUpdater"))
		{
			App.Logger.WriteLine("Bootstrapper::StartBackgroundUpdater", "Background updater already running");
			return;
		}
		App.Logger.WriteLine("Bootstrapper::StartBackgroundUpdater", "Starting background updater");
		Process.Start(Paths.Process, "-backgroundupdater");
	}

	private async Task<bool> ApplyModifications()
	{
		bool success = true;
		SetStatus(Strings.Bootstrapper_Status_ApplyingModifications);
		App.Logger.WriteLine("Bootstrapper::ApplyModifications", "Checking file mods...");
		File.Delete(Path.Combine(Paths.Base, "ModManifest.txt"));
		List<string> modFolderFiles = new List<string>();
		Directory.CreateDirectory(Paths.Modifications);
		string text = Path.Combine(Paths.Modifications, "content\\fonts\\families");
		string[] files;
		if (File.Exists(Paths.CustomFont))
		{
			App.Logger.WriteLine("Bootstrapper::ApplyModifications", "Begin font check");
			Directory.CreateDirectory(text);
			string text2 = Path.Combine(_latestVersionDirectory, "content");
			Directory.CreateDirectory(text2);
			string text3 = Path.Combine(text2, "fonts");
			Directory.CreateDirectory(text3);
			string path = Path.Combine(text3, "families");
			Directory.CreateDirectory(path);
			files = Directory.GetFiles(path);
			foreach (string path2 in files)
			{
				string fileName = Path.GetFileName(path2);
				string path3 = Path.Combine(text, fileName);
				if (File.Exists(path3))
				{
					continue;
				}
				App.Logger.WriteLine("Bootstrapper::ApplyModifications", "Setting font for " + fileName);
				OrbitstrapFontFamily fontFamily = JsonSerializer.Deserialize<OrbitstrapFontFamily>(File.ReadAllText(path2));
				if (fontFamily == null)
				{
					continue;
				}
				bool flag = false;
				foreach (FontFace face in fontFamily.Faces)
				{
					if (face.AssetId != "rbxasset://fonts/CustomFont.ttf")
					{
						face.AssetId = "rbxasset://fonts/CustomFont.ttf";
						flag = true;
					}
				}
				if (flag)
				{
					File.WriteAllText(path3, JsonSerializer.Serialize(fontFamily, new JsonSerializerOptions
					{
						WriteIndented = true
					}));
				}
			}
			App.Logger.WriteLine("Bootstrapper::ApplyModifications", "End font check");
		}
		else if (Directory.Exists(text))
		{
			Directory.Delete(text, recursive: true);
		}
		files = Directory.GetFiles(Paths.Modifications, "*.*", SearchOption.AllDirectories);
		foreach (string text4 in files)
		{
			if (_cancelTokenSource.IsCancellationRequested)
			{
				return true;
			}
			string text5 = text4.Substring(Paths.Modifications.Length + 1);
			if (text5 == "README.txt")
			{
				File.Delete(text4);
			}
			else
			{
				if ((!App.Settings.Prop.UseFastFlagManager && string.Equals(text5, "ClientSettings\\ClientAppSettings.json", StringComparison.OrdinalIgnoreCase)) || text5.EndsWith(".lock"))
				{
					continue;
				}
				modFolderFiles.Add(text5);
				string text6 = Path.Combine(Paths.Modifications, text5);
				string text7 = Path.Combine(_latestVersionDirectory, text5);
				if (File.Exists(text7) && MD5Hash.FromFile(text6) == MD5Hash.FromFile(text7))
				{
					App.Logger.WriteLine("Bootstrapper::ApplyModifications", text5 + " already exists in the version folder, and is a match");
					continue;
				}
				Directory.CreateDirectory(Path.GetDirectoryName(text7));
				Filesystem.AssertReadOnly(text7);
				try
				{
					File.Copy(text6, text7, overwrite: true);
					Filesystem.AssertReadOnly(text7);
					App.Logger.WriteLine("Bootstrapper::ApplyModifications", text5 + " has been copied to the version folder");
				}
				catch (Exception ex)
				{
					App.Logger.WriteLine("Bootstrapper::ApplyModifications", "Failed to apply modification (" + text5 + ")");
					App.Logger.WriteException("Bootstrapper::ApplyModifications", ex);
					success = false;
				}
			}
		}
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		foreach (string fileLocation in App.RobloxState.Prop.ModManifest)
		{
			if (modFolderFiles.Contains(fileLocation))
			{
				continue;
			}
			KeyValuePair<string, string> keyValuePair = PackageDirectoryMap.SingleOrDefault<KeyValuePair<string, string>>((KeyValuePair<string, string> x) => !string.IsNullOrEmpty(x.Value) && fileLocation.StartsWith(x.Value));
			string key = keyValuePair.Key;
			if (string.IsNullOrEmpty(key))
			{
				App.Logger.WriteLine("Bootstrapper::ApplyModifications", fileLocation + " was removed as a mod but does not belong to a package");
				string path4 = Path.Combine(_latestVersionDirectory, fileLocation);
				if (File.Exists(path4))
				{
					File.Delete(path4);
				}
				continue;
			}
			string item = fileLocation.Substring(keyValuePair.Value.Length);
			if (!dictionary.ContainsKey(key))
			{
				dictionary[key] = new List<string>();
			}
			dictionary[key].Add(item);
			App.Logger.WriteLine("Bootstrapper::ApplyModifications", fileLocation + " was removed as a mod, restoring from " + key);
		}
		foreach (KeyValuePair<string, List<string>> entry in dictionary)
		{
			Package package = _versionPackageManifest.Find((Package x) => x.Name == entry.Key);
			if (package != null)
			{
				if (_cancelTokenSource.IsCancellationRequested)
				{
					return true;
				}
				await DownloadPackage(package);
				ExtractPackage(package, entry.Value);
			}
		}
		if (App.LaunchSettings.BackgroundUpdaterFlag.Active || !App.RobloxState.HasFileOnDiskChanged())
		{
			App.RobloxState.Prop.ModManifest = modFolderFiles;
			App.RobloxState.Save();
		}
		else
		{
			App.Logger.WriteLine("Bootstrapper::ApplyModifications", "RobloxState disk mismatch, not saving ModManifest");
		}
		// ── Skybox (Voidstrap GitHub logic) ───────────────
		if (App.Settings.Prop.SkyboxEnabled &&
		    !string.Equals(App.Settings.Prop.SkyboxName, "Default", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				await ApplySkyboxPatchToRobloxStorageAsync();
				await EnsureSkyboxPackDownloadedAsync();
				await ApplySkyboxAsync(App.Settings.Prop.SkyboxName, Paths.Modifications);
				App.Logger.WriteLine("Bootstrapper::ApplyModifications", "Skybox applied.");
			}
			catch (Exception exSky)
			{
				App.Logger.WriteLine("Bootstrapper::ApplyModifications", $"Skybox failed: {exSky.Message}");
			}
		}
		else
		{
			// Skybox disabled / Default — clear mod folder so manifest restores originals
			string modSkyDir = Path.Combine(Paths.Modifications, "PlatformContent", "pc", "textures", "sky");
			if (Directory.Exists(modSkyDir))
				Directory.Delete(modSkyDir, recursive: true);
		}
		// ── End skybox ─────────────────────────

		App.Logger.WriteLine("Bootstrapper::ApplyModifications", "Finished checking file mods");
		if (!success)
		{
			App.Logger.WriteLine("Bootstrapper::ApplyModifications", "Failed to apply all modifications");
		}
		return success;
	}

	private async Task DownloadPackage(Package package)
	{
		string LOG_IDENT = "Bootstrapper::DownloadPackage." + package.Name;
		if (_cancelTokenSource.IsCancellationRequested)
		{
			return;
		}
		Directory.CreateDirectory(Paths.Downloads);
		string packageUrl = Deployment.GetLocation("/" + _latestVersionGuid + "-" + package.Name);
		string text = Path.Combine(Paths.LocalAppData, "Roblox", "Downloads", package.Signature);
		if (File.Exists(package.DownloadPath))
		{
			FileInfo fileInfo = new FileInfo(package.DownloadPath);
			string text2 = MD5Hash.FromFile(package.DownloadPath);
			if (!(text2 != package.Signature))
			{
				App.Logger.WriteLine(LOG_IDENT, "Package is already downloaded, skipping...");
				_totalDownloadedBytes += package.PackedSize;
				UpdateProgressBar();
				return;
			}
			App.Logger.WriteLine(LOG_IDENT, $"Package is corrupted ({text2} != {package.Signature})! Deleting and re-downloading...");
			fileInfo.Delete();
		}
		else if (File.Exists(text))
		{
			App.Logger.WriteLine(LOG_IDENT, "Found existing copy at '" + text + "'! Copying to Downloads folder...");
			File.Copy(text, package.DownloadPath);
			_totalDownloadedBytes += package.PackedSize;
			UpdateProgressBar();
			return;
		}
		if (File.Exists(package.DownloadPath))
		{
			return;
		}
		App.Logger.WriteLine(LOG_IDENT, "Downloading...");
		byte[] buffer = new byte[4096];
		for (int i = 1; i <= 5; i++)
		{
			if (_cancelTokenSource.IsCancellationRequested)
			{
				break;
			}
			int totalBytesRead = 0;
			try
			{
				int num = 0;
				await using (Stream stream = await (await App.HttpClient.GetAsync(packageUrl, HttpCompletionOption.ResponseHeadersRead, _cancelTokenSource.Token)).Content.ReadAsStreamAsync(_cancelTokenSource.Token))
				{
					await using (FileStream fileStream = new FileStream(package.DownloadPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Delete))
					{
						while (true)
						{
							if (_cancelTokenSource.IsCancellationRequested)
							{
								stream.Close();
								fileStream.Close();
								break;
							}
							int bytesRead = await stream.ReadAsync(buffer, _cancelTokenSource.Token);
							if (bytesRead != 0)
							{
								totalBytesRead += bytesRead;
								await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), _cancelTokenSource.Token);
								_totalDownloadedBytes += bytesRead;
								SetStatus(string.Format(App.Settings.Prop.DownloadingStringFormat, package.Name, totalBytesRead / 1048576, package.Size / 1048576));
								UpdateProgressBar();
								continue;
							}
							string text3 = MD5Hash.FromStream(fileStream);
							if (text3 != package.Signature)
							{
								throw new ChecksumFailedException($"Failed to verify download of {packageUrl}\n\nExpected hash: {package.Signature}\nGot hash: {text3}");
							}
							App.Logger.WriteLine(LOG_IDENT, $"Finished downloading! ({totalBytesRead} bytes total)");
							goto end_IL_0000;
						}
						goto IL_06ea;
						end_IL_0000:;
					}
					break;
					IL_06ea:
					if (num == 2)
					{
					}
				}
				if (num != 2)
				{
					continue;
				}
				break;
			}
			catch (Exception ex)
			{
				App.Logger.WriteLine(LOG_IDENT, $"An exception occurred after downloading {totalBytesRead} bytes. ({i}/{5})");
				App.Logger.WriteException(LOG_IDENT, ex);
				if (ex.GetType() == typeof(ChecksumFailedException))
				{
					Frontend.ShowConnectivityDialog(Strings.Dialog_Connectivity_UnableToDownload, string.Format(Strings.Dialog_Connectivity_UnableToDownloadReason, "[https://github.com/orbitthegreatest/Orbitstrap/wiki/Orbitstrap-is-unable-to-download-Roblox](https://github.com/orbitthegreatest/Orbitstrap/wiki/Orbitstrap-is-unable-to-download-Roblox)"), MessageBoxImage.Hand, ex);
					App.Terminate(ErrorCode.ERROR_CANCELLED);
				}
				else if (i >= 5)
				{
					throw;
				}
				if (File.Exists(package.DownloadPath))
				{
					File.Delete(package.DownloadPath);
				}
				_totalDownloadedBytes -= totalBytesRead;
				UpdateProgressBar();
				if (ex.GetType() == typeof(IOException) && !packageUrl.StartsWith("http://"))
				{
					App.Logger.WriteLine(LOG_IDENT, "Retrying download over HTTP...");
					packageUrl = packageUrl.Replace("https://", "http://");
				}
			}
		}
	}

	private void ExtractPackage(Package package, List<string>? files = null)
	{
		string valueOrDefault = PackageDirectoryMap.GetValueOrDefault(package.Name);
		if (valueOrDefault == null)
		{
			App.Logger.WriteLine("Bootstrapper::ExtractPackage", "WARNING: " + package.Name + " was not found in the package map!");
			return;
		}
		string targetDirectory = Path.Combine(_latestVersionDirectory, valueOrDefault);
		string fileFilter = null;
		if (files != null)
		{
			List<string> list = new List<string>();
			foreach (string file in files)
			{
				list.Add("^" + file.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)") + "$");
			}
			fileFilter = string.Join(';', list);
		}
		App.Logger.WriteLine("Bootstrapper::ExtractPackage", "Extracting " + package.Name + "...");
		new FastZip(_fastZipEvents).ExtractZip(package.DownloadPath, targetDirectory, fileFilter);
		App.Logger.WriteLine("Bootstrapper::ExtractPackage", "Finished extracting " + package.Name);
	}

	#region Skybox

	private async Task<string> GetLatestSkyboxCommitShaAsync()
	{
		using var req = new HttpRequestMessage(HttpMethod.Get, SkyboxCommitApiUrl);
		req.Headers.UserAgent.ParseAdd("OrbitstrapSkyboxClient");
		using var res = await SkyboxHttpClient.SendAsync(req);
		res.EnsureSuccessStatusCode();
		using var stream = await res.Content.ReadAsStreamAsync();
		using var doc = await JsonDocument.ParseAsync(stream);
		return doc.RootElement.GetProperty("sha").GetString()!;
	}

	private static string? GetLocalSkyboxCommit() =>
		File.Exists(Path.Combine(PackFolder, SkyboxVersionFile))
			? File.ReadAllText(Path.Combine(PackFolder, SkyboxVersionFile))
			: null;

	private static void SaveLocalSkyboxCommit(string sha) =>
		File.WriteAllText(Path.Combine(PackFolder, SkyboxVersionFile), sha);

	public async Task EnsureSkyboxPackDownloadedAsync()
	{
		Directory.CreateDirectory(PackFolder);

		string latest = await GetLatestSkyboxCommitShaAsync();
		if (GetLocalSkyboxCommit() == latest &&
		    Directory.GetFiles(PackFolder, "*", SearchOption.AllDirectories).Length > 0)
			return;

		SetStatus("Updating Skybox Pack...");

		string tempZip = Path.Combine(Path.GetTempPath(), "OrbitstrapSkyboxPackV2.zip");

		using (var response = await SkyboxHttpClient.GetAsync(
			SkyboxZipUrl, HttpCompletionOption.ResponseHeadersRead))
		{
			response.EnsureSuccessStatusCode();
			long total = response.Content.Headers.ContentLength ?? -1L;
			long read = 0;
			byte[] buf = new byte[262144];
			var lastPrint = Stopwatch.StartNew();
			await using var src = await response.Content.ReadAsStreamAsync();
			await using var dst = new FileStream(
				tempZip, FileMode.Create, FileAccess.Write, FileShare.None, buf.Length, true);
			int chunk;
			while ((chunk = await src.ReadAsync(buf)) > 0)
			{
				await dst.WriteAsync(buf.AsMemory(0, chunk));
				read += chunk;
				if (lastPrint.ElapsedMilliseconds > 200)
				{
					SetStatus(total > 0
						? $"Downloading Skybox... {read * 100.0 / total:F1}%"
						: $"Downloading Skybox... {read / 1024 / 1024} MB");
					lastPrint.Restart();
				}
			}
		}

		if (Directory.Exists(PackFolder)) Directory.Delete(PackFolder, true);
		Directory.CreateDirectory(PackFolder);

		using (var zip = System.IO.Compression.ZipFile.OpenRead(tempZip))
		{
			foreach (var entry in zip.Entries)
			{
				if (string.IsNullOrEmpty(entry.Name)) continue;
				var parts = entry.FullName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
				string dest = Path.Combine(PackFolder, Path.Combine(parts.Skip(1).ToArray()));
				Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
				using var es = entry.Open();
				using var fs = new FileStream(dest, FileMode.Create, FileAccess.Write);
				await es.CopyToAsync(fs);
			}
		}

		SaveLocalSkyboxCommit(latest);
		File.Delete(tempZip);
	}

	public static async Task ApplySkyboxAsync(string skyboxName, string modsFolder)
	{
		string src = Path.Combine(PackFolder, skyboxName);
		if (!Directory.Exists(src))
			throw new DirectoryNotFoundException($"Skybox '{skyboxName}' not found in SkyboxPack.");

		string dest = Path.Combine(modsFolder, "PlatformContent", "pc", "textures", "sky");

		if (Directory.Exists(dest))
		{
			foreach (var f in Directory.GetFiles(dest, "*.*", SearchOption.AllDirectories))
				File.SetAttributes(f, FileAttributes.Normal);
			Directory.Delete(dest, true);
		}

		Directory.CreateDirectory(dest);

		foreach (var file in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories))
		{
			string rel = Path.GetRelativePath(src, file);
			string dOut = Path.Combine(dest, rel);
			Directory.CreateDirectory(Path.GetDirectoryName(dOut)!);
			File.Copy(file, dOut, true);
		}
	}

	public static async Task ApplySkyboxPatchToRobloxStorageAsync()
	{
		string rbxStorage = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Roblox", "rbx-storage");

		const string githubBase = "https://raw.githubusercontent.com/KloBraticc/SkyboxPatch/main/assets/";

		using var http = new HttpClient();

		foreach (var kvp in SkyboxPatchFolderMap)
		{
			string hash = kvp.Key;
			string folder = kvp.Value;
			string dir = Path.Combine(rbxStorage, folder);
			Directory.CreateDirectory(dir);
			string destFile = Path.Combine(dir, hash);
			try
			{
				byte[] data = await http.GetByteArrayAsync(githubBase + hash);
				if (File.Exists(destFile)) File.SetAttributes(destFile, FileAttributes.Normal);
				await File.WriteAllBytesAsync(destFile, data);
				File.SetAttributes(destFile, FileAttributes.ReadOnly);
			}
			catch (Exception ex)
			{
				App.Logger.WriteLine("Bootstrapper::SkyboxPatch", $"Failed {hash}: {ex.Message}");
			}
		}
	}

	#endregion
}
