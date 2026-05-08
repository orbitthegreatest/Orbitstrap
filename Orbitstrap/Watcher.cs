using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Orbitstrap.Integrations;
using Orbitstrap.Models;
using Orbitstrap.UI;
using Orbitstrap.Utility;

namespace Orbitstrap;

public class Watcher : IDisposable
{
	private readonly InterProcessLock _lock = new InterProcessLock("Watcher");

	private readonly WatcherData? _watcherData;

	private readonly NotifyIconWrapper? _notifyIcon;

	public readonly ActivityWatcher? ActivityWatcher;

	public readonly DiscordRichPresence? RichPresence;

	public Watcher()
	{
		if (!_lock.IsAcquired)
		{
			App.Logger.WriteLine("Watcher", "Watcher instance already exists");
			return;
		}
		string data = App.LaunchSettings.WatcherFlag.Data;
		if (string.IsNullOrEmpty(data))
		{
			throw new Exception("Watcher data not specified");
		}
		_watcherData = JsonSerializer.Deserialize<WatcherData>(Encoding.UTF8.GetString(Convert.FromBase64String(data)));
		if (_watcherData == null)
		{
			throw new Exception("Watcher data is invalid");
		}
		if (App.Settings.Prop.EnableActivityTracking)
		{
			ActivityWatcher = new ActivityWatcher(_watcherData.LogFile);
			if (App.Settings.Prop.UseDisableAppPatch)
			{
				ActivityWatcher.OnAppClose += delegate
				{
					App.Logger.WriteLine("Watcher", "Received desktop app exit, closing Roblox");
					using Process process = Process.GetProcessById(_watcherData.ProcessId);
					process.CloseMainWindow();
				};
			}
			if (App.Settings.Prop.UseDiscordRichPresence && !App.State.Prop.WatcherRunning)
			{
				App.Logger.WriteLine("Watcher", "Running rpc");
				RichPresence = new DiscordRichPresence(ActivityWatcher);
			}
		}
		_notifyIcon = new NotifyIconWrapper(this);
	}

	public void KillRobloxProcess()
	{
		CloseProcess(_watcherData.ProcessId, force: true);
	}

	public void CloseProcess(int pid, bool force = false)
	{
		try
		{
			using Process process = Process.GetProcessById(pid);
			App.Logger.WriteLine("Watcher::CloseProcess", $"Killing process '{process.ProcessName}' (pid={pid}, force={force})");
			if (process.HasExited)
			{
				App.Logger.WriteLine("Watcher::CloseProcess", $"PID {pid} has already exited");
			}
			else if (force)
			{
				process.Kill();
			}
			else
			{
				process.CloseMainWindow();
			}
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("Watcher::CloseProcess", $"PID {pid} could not be closed");
			App.Logger.WriteException("Watcher::CloseProcess", ex);
		}
	}

	public async Task Run()
	{
		if (!_lock.IsAcquired || _watcherData == null)
		{
			return;
		}
		ActivityWatcher?.Start();
		while (Utilities.GetProcessesSafe().Any((Process x) => x.Id == _watcherData.ProcessId))
		{
			await Task.Delay(1000);
		}
		if (_watcherData.AutoclosePids != null)
		{
			foreach (int autoclosePid in _watcherData.AutoclosePids)
			{
				CloseProcess(autoclosePid);
			}
		}
		if (App.LaunchSettings.TestModeFlag.Active)
		{
			Process.Start(Paths.Process, "-settings -testmode");
		}
	}

	public void Dispose()
	{
		App.Logger.WriteLine("Watcher::Dispose", "Disposing Watcher");
		_notifyIcon?.Dispose();
		RichPresence?.Dispose();
		App.State.Prop.WatcherRunning = false;
		GC.SuppressFinalize(this);
	}
}
