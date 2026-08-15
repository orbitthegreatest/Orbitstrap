using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Orbitstrap;

internal static class MultiInstanceWatcher
{
	private static int GetOpenProcessesCount()
	{
		try
		{
			return Process.GetProcesses().Count(delegate(Process x)
			{
				string processName = x.ProcessName;
				return (processName == "RobloxPlayerBeta" || processName == "Orbitstrap") ? true : false;
			}) - 1;
		}
		catch (Exception ex)
		{
			App.Logger.WriteException("MultiInstanceWatcher::GetOpenProcessesCount", ex);
			return -1;
		}
	}

	private static void FireInitialisedEvent()
	{
		using EventWaitHandle eventWaitHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, "Orbitstrap-MultiInstanceWatcherInitialisationFinished");
		eventWaitHandle.Set();
	}

	public static void Run()
	{
		using Mutex mutex = new Mutex(initiallyOwned: false, "ROBLOX_singletonMutex");
		bool flag;
		try
		{
			flag = mutex.WaitOne(0);
		}
		catch (AbandonedMutexException)
		{
			flag = true;
		}
		if (!flag)
		{
			App.Logger.WriteLine("MultiInstanceWatcher::Run", "Client singleton mutex is already acquired");
			FireInitialisedEvent();
			return;
		}
		App.Logger.WriteLine("MultiInstanceWatcher::Run", "Acquired mutex!");
		FireInitialisedEvent();
		int openProcessesCount;
		do
		{
			Thread.Sleep(5000);
			openProcessesCount = GetOpenProcessesCount();
		}
		while (openProcessesCount == -1 || openProcessesCount > 0);
		App.Logger.WriteLine("MultiInstanceWatcher::Run", "All Roblox related processes have closed, exiting!");
	}
}
