using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Orbitstrap.Enums;
using Orbitstrap.Extensions;
using Orbitstrap.Integrations;
using Orbitstrap.Resources;
using Orbitstrap.UI;
using Orbitstrap.UI.Elements.Dialogs;
using Orbitstrap.UI.Elements.Installer;
using Orbitstrap.UI.Elements.Settings;
using Orbitstrap.Utility;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Orbitstrap;

public static class LaunchHandler
{
	public static void ProcessNextAction(NextAction action, bool isUnfinishedInstall = false)
	{
		switch (action)
		{
		case NextAction.LaunchSettings:
			App.Logger.WriteLine("LaunchHandler::ProcessNextAction", "Opening settings");
			LaunchSettings();
			break;
		case NextAction.LaunchRoblox:
			App.Logger.WriteLine("LaunchHandler::ProcessNextAction", "Opening Roblox");
			LaunchRoblox(LaunchMode.Player);
			break;
		case NextAction.LaunchRobloxStudio:
			App.Logger.WriteLine("LaunchHandler::ProcessNextAction", "Opening Roblox Studio");
			LaunchRoblox(LaunchMode.Studio);
			break;
		default:
			App.Logger.WriteLine("LaunchHandler::ProcessNextAction", "Closing");
			App.Terminate(isUnfinishedInstall ? ErrorCode.ERROR_INSTALL_USEREXIT : ErrorCode.ERROR_SUCCESS);
			break;
		}
	}

	public static void ProcessLaunchArgs()
	{
		if (App.LaunchSettings.UninstallFlag.Active)
		{
			App.Logger.WriteLine("LaunchHandler::ProcessLaunchArgs", "Opening uninstaller");
			LaunchUninstaller();
		}
		else if (App.LaunchSettings.MenuFlag.Active)
		{
			App.Logger.WriteLine("LaunchHandler::ProcessLaunchArgs", "Opening settings");
			LaunchSettings();
		}
		else if (App.LaunchSettings.WatcherFlag.Active)
		{
			App.Logger.WriteLine("LaunchHandler::ProcessLaunchArgs", "Opening watcher");
			LaunchWatcher();
		}
		else if (App.LaunchSettings.MultiInstanceWatcherFlag.Active)
		{
			App.Logger.WriteLine("LaunchHandler::ProcessLaunchArgs", "Opening multi-instance watcher");
			LaunchMultiInstanceWatcher();
		}
		else if (App.LaunchSettings.BackgroundUpdaterFlag.Active)
		{
			App.Logger.WriteLine("LaunchHandler::ProcessLaunchArgs", "Opening background updater");
			LaunchBackgroundUpdater();
		}
		else if (App.LaunchSettings.RobloxLaunchMode != LaunchMode.None)
		{
			App.Logger.WriteLine("LaunchHandler::ProcessLaunchArgs", $"Opening bootstrapper ({App.LaunchSettings.RobloxLaunchMode})");
			LaunchRoblox(App.LaunchSettings.RobloxLaunchMode);
		}
		else if (App.LaunchSettings.BloxshadeFlag.Active)
		{
			App.Logger.WriteLine("LaunchHandler::ProcessLaunchArgs", "Opening Bloxshade");
			LaunchBloxshadeConfig();
		}
		else if (!App.LaunchSettings.QuietFlag.Active)
		{
			App.Logger.WriteLine("LaunchHandler::ProcessLaunchArgs", "Opening menu");
			LaunchMenu();
		}
		else
		{
			App.Logger.WriteLine("LaunchHandler::ProcessLaunchArgs", "Closing - quiet flag active");
			App.Terminate();
		}
	}

	public static void LaunchInstaller()
	{
		using InterProcessLock interProcessLock = new InterProcessLock("Installer");
		if (!interProcessLock.IsAcquired)
		{
			Frontend.ShowMessageBox(Strings.Dialog_AlreadyRunning_Installer, MessageBoxImage.Hand);
			App.Terminate();
		}
		else if (App.LaunchSettings.UninstallFlag.Active)
		{
			Frontend.ShowMessageBox(Strings.Bootstrapper_FirstRunUninstall, MessageBoxImage.Hand);
			App.Terminate(ErrorCode.ERROR_INVALID_FUNCTION);
		}
		else if (App.LaunchSettings.QuietFlag.Active)
		{
			Installer installer = new Installer();
			if (!installer.CheckInstallLocation())
			{
				App.Terminate(ErrorCode.ERROR_INSTALL_FAILURE);
			}
			installer.DoInstall();
			interProcessLock.Dispose();
			ProcessLaunchArgs();
		}
		else
		{
			new LanguageSelectorDialog().ShowDialog();
			Orbitstrap.UI.Elements.Installer.MainWindow mainWindow = new Orbitstrap.UI.Elements.Installer.MainWindow();
			mainWindow.ShowDialog();
			interProcessLock.Dispose();

			if (mainWindow.Finished)
			{
				// Installation completed - relaunch from AppData so everything runs from the correct location.
				string launchArgs = mainWindow.CloseAction switch
				{
					NextAction.LaunchSettings    => "-menu",
					NextAction.LaunchRoblox       => "-player",
					NextAction.LaunchRobloxStudio => "-studio",
					_                             => ""
				};
				try
				{
					Process.Start(new ProcessStartInfo
					{
						FileName        = Paths.Application,
						Arguments       = launchArgs,
						UseShellExecute = true
					});
				}
				catch (Exception relaunchEx)
				{
					App.Logger.WriteException("LaunchHandler::LaunchInstaller", relaunchEx);
				}
				App.Terminate();
				return;
			}

			ProcessNextAction(mainWindow.CloseAction, !mainWindow.Finished);
		}
	}

	public static void LaunchUninstaller()
	{
		using InterProcessLock interProcessLock = new InterProcessLock("Uninstaller");
		if (!interProcessLock.IsAcquired)
		{
			Frontend.ShowMessageBox(Strings.Dialog_AlreadyRunning_Uninstaller, MessageBoxImage.Hand);
			App.Terminate();
			return;
		}
		bool flag = false;
		bool keepData = true;
		if (App.LaunchSettings.QuietFlag.Active)
		{
			flag = true;
		}
		else
		{
			UninstallerDialog uninstallerDialog = new UninstallerDialog();
			uninstallerDialog.ShowDialog();
			flag = uninstallerDialog.Confirmed;
			keepData = uninstallerDialog.KeepData;
		}
		if (!flag)
		{
			App.Terminate();
			return;
		}
		Installer.DoUninstall(keepData);
		Frontend.ShowMessageBox(Strings.Bootstrapper_SuccessfullyUninstalled, MessageBoxImage.Asterisk);
		App.Terminate();
	}

	public static void LaunchSettings()
	{
		using InterProcessLock interProcessLock = new InterProcessLock("Settings");
		if (interProcessLock.IsAcquired)
		{
			new Orbitstrap.UI.Elements.Settings.MainWindow(Process.GetProcessesByName("Orbitstrap").Length > 1).ShowDialog();
			return;
		}
		App.Logger.WriteLine("LaunchHandler::LaunchSettings", "Found an already existing menu window");
		Process process = (from x in Utilities.GetProcessesSafe()
			where x.MainWindowTitle == Strings.Menu_Title
			select x).FirstOrDefault();
		if (process != null)
		{
			PInvoke.SetForegroundWindow((HWND)process.MainWindowHandle);
		}
		App.Terminate();
	}

	public static void LaunchMenu()
	{
		LaunchMenuDialog launchMenuDialog = new LaunchMenuDialog();
		launchMenuDialog.ShowDialog();
		ProcessNextAction(launchMenuDialog.CloseAction);
	}

	public static void LaunchRoblox(LaunchMode launchMode)
	{
		if (launchMode == LaunchMode.None)
		{
			throw new InvalidOperationException("No Roblox launch mode set");
		}
		if (!File.Exists(Path.Combine(Paths.System, "mfplat.dll")))
		{
			Frontend.ShowMessageBox(Strings.Bootstrapper_WMFNotFound, MessageBoxImage.Hand);
			if (!App.LaunchSettings.QuietFlag.Active)
			{
				Utilities.ShellExecute("https://support.microsoft.com/en-us/topic/media-feature-pack-list-for-windows-n-editions-c1c6fffa-d052-8338-7a79-a4bb980a700a");
			}
			App.Terminate(ErrorCode.ERROR_FILE_NOT_FOUND);
		}
		if (App.Settings.Prop.ConfirmLaunches && Mutex.TryOpenExisting("ROBLOX_singletonMutex", out Mutex _) && !App.Settings.Prop.MultiInstanceLaunching && Frontend.ShowMessageBox(Strings.Bootstrapper_ConfirmLaunch, MessageBoxImage.Exclamation, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
		{
			App.Terminate();
			return;
		}
		App.Logger.WriteLine("LaunchHandler::LaunchRoblox", "Initializing bootstrapper");
		App.Bootstrapper = new Bootstrapper(launchMode);
		IBootstrapperDialog bootstrapperDialog = null;
		if (!App.LaunchSettings.QuietFlag.Active)
		{
			App.Logger.WriteLine("LaunchHandler::LaunchRoblox", "Initializing bootstrapper dialog");
			bootstrapperDialog = App.Settings.Prop.BootstrapperStyle.GetNew();
			App.Bootstrapper.Dialog = bootstrapperDialog;
			bootstrapperDialog.Bootstrapper = App.Bootstrapper;
		}
		_ = Task.Run((Func<Task?>)App.Bootstrapper.Run).ContinueWith(delegate(Task t)
		{
			App.Logger.WriteLine("LaunchHandler::LaunchRoblox", "Bootstrapper task has finished");
			if (t.IsFaulted)
			{
				App.Logger.WriteLine("LaunchHandler::LaunchRoblox", "An exception occurred when running the bootstrapper");
				if (t.Exception != null)
				{
					App.FinalizeExceptionHandling(t.Exception);
				}
			}
			App.Terminate();
		});
		bootstrapperDialog?.ShowBootstrapper();
		App.Logger.WriteLine("LaunchHandler::LaunchRoblox", "Exiting");
	}

	public static void LaunchWatcher()
	{
		Watcher watcher = new Watcher();
		_ = Task.Run((Func<Task?>)watcher.Run).ContinueWith(delegate(Task t)
		{
			App.Logger.WriteLine("LaunchHandler::LaunchWatcher", "Watcher task has finished");
			watcher.Dispose();
			if (t.IsFaulted)
			{
				App.Logger.WriteLine("LaunchHandler::LaunchWatcher", "An exception occurred when running the watcher");
				if (t.Exception != null)
				{
					App.FinalizeExceptionHandling(t.Exception);
				}
			}
			if (App.Settings.Prop.CleanerOptions != CleanerOptions.Never)
			{
				Cleaner.DoCleaning();
			}
			App.Terminate();
		});
	}

	public static void LaunchMultiInstanceWatcher()
	{
		App.Logger.WriteLine("LaunchHandler::LaunchMultiInstanceWatcher", "Starting multi-instance watcher");
		_ = Task.Run((Action)MultiInstanceWatcher.Run).ContinueWith(delegate(Task t)
		{
			App.Logger.WriteLine("LaunchHandler::LaunchMultiInstanceWatcher", "Multi instance watcher task has finished");
			if (t.IsFaulted)
			{
				App.Logger.WriteLine("LaunchHandler::LaunchMultiInstanceWatcher", "An exception occurred when running the multi-instance watcher");
				if (t.Exception != null)
				{
					App.FinalizeExceptionHandling(t.Exception);
				}
			}
			App.Terminate();
		});
	}

	public static void LaunchBloxshadeConfig()
	{
		App.Logger.WriteLine("LaunchHandler::LaunchBloxshade", "Showing unsupported warning");
		new BloxshadeDialog().ShowDialog();
		App.SoftTerminate();
	}

	public static void LaunchBackgroundUpdater()
	{
		App.LaunchSettings.QuietFlag.Active = true;
		App.LaunchSettings.NoLaunchFlag.Active = true;
		App.Logger.WriteLine("LaunchHandler::LaunchBackgroundUpdater", "Initializing bootstrapper");
		App.Bootstrapper = new Bootstrapper(LaunchMode.Player)
		{
			MutexName = "Orbitstrap-BackgroundUpdater",
			QuitIfMutexExists = true
		};
		CancellationTokenSource cts = new CancellationTokenSource();
		_ = Task.Run(delegate
		{
			App.Logger.WriteLine("LaunchHandler::LaunchBackgroundUpdater", "Started event waiter");
			using (EventWaitHandle eventWaitHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, "Orbitstrap-BackgroundUpdaterKillEvent"))
			{
				eventWaitHandle.WaitOne();
			}
			App.Logger.WriteLine("LaunchHandler::LaunchBackgroundUpdater", "Received close event, killing it all!");
			App.Bootstrapper.Cancel();
		}, cts.Token);
		_ = Task.Run((Func<Task?>)App.Bootstrapper.Run).ContinueWith(delegate(Task t)
		{
			App.Logger.WriteLine("LaunchHandler::LaunchBackgroundUpdater", "Bootstrapper task has finished");
			cts.Cancel();
			if (t.IsFaulted)
			{
				App.Logger.WriteLine("LaunchHandler::LaunchBackgroundUpdater", "An exception occurred when running the bootstrapper");
				if (t.Exception != null)
				{
					App.FinalizeExceptionHandling(t.Exception);
				}
			}
			App.Terminate();
		});
		App.Logger.WriteLine("LaunchHandler::LaunchBackgroundUpdater", "Exiting");
	}
}
