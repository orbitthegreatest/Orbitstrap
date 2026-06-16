using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Orbitstrap;

public class ModInjector
{
	private const string OFFSETS_URL = "https://imtheo.lol/Offsets/FFlags.hpp";

	private readonly HttpClient _httpClient = new HttpClient();

	private Dictionary<string, int> _offsets = new Dictionary<string, int>();

	private DispatcherTimer _timer;

	private Dictionary<int, DateTime> _lastInjectionTimes = new Dictionary<int, DateTime>();

	private const int rblxchecker = 2;

	private const int rblxreinjecter = 5;

	private const uint PROCESS_VM_READ = 16u;

	private const uint PROCESS_VM_WRITE = 32u;

	private const uint PROCESS_VM_OPERATION = 8u;

	private const uint PROCESS_QUERY_INFORMATION = 1024u;

	public bool IsEnabled => App.Settings.Prop.ModInjectorEnabled;

	[DllImport("kernel32.dll")]
	private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

	[DllImport("kernel32.dll")]
	private static extern bool CloseHandle(IntPtr hObject);

	[DllImport("kernel32.dll")]
	private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

	[DllImport("kernel32.dll")]
	private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

	public ModInjector()
	{
		_timer = new DispatcherTimer();
		_timer.Interval = TimeSpan.FromSeconds(2.0);
		_timer.Tick += delegate
		{
			checkinject();
		};
	}

	public void Initialize()
	{
		if (IsEnabled)
		{
			Task.Run((Func<Task?>)valueoffsets);
			_timer.Start();
		}
	}

	public void Toggle(bool enabled)
	{
		if (enabled)
		{
			if (!_timer.IsEnabled)
			{
				Task.Run((Func<Task?>)valueoffsets);
				_timer.Start();
				checkinject();
			}
		}
		else if (_timer.IsEnabled)
		{
			_timer.Stop();
			_lastInjectionTimes.Clear();
		}
	}

	private async Task valueoffsets()
	{
		if (_offsets.Count > 0)
		{
			return;
		}
		try
		{
			_offsets = ParseCppOffsets(await _httpClient.GetStringAsync("https://imtheo.lol/Offsets/FFlags.hpp"));
		}
		catch (Exception)
		{
		}
	}

	private Dictionary<string, int> ParseCppOffsets(string cppContent)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (Match item in new Regex("uintptr_t\\s+(\\w+)\\s*=\\s*0x([0-9A-Fa-f]+)", RegexOptions.Multiline).Matches(cppContent))
		{
			if (int.TryParse(item.Groups[2].Value, NumberStyles.HexNumber, null, out var result))
			{
				dictionary[item.Groups[1].Value] = result;
			}
		}
		foreach (Match item2 in new Regex("inline\\s+constexpr\\s+uintptr_t\\s+(\\w+)\\s*=\\s*0x([0-9A-Fa-f]+)", RegexOptions.Multiline).Matches(cppContent))
		{
			if (int.TryParse(item2.Groups[2].Value, NumberStyles.HexNumber, null, out var result2))
			{
				dictionary[item2.Groups[1].Value] = result2;
			}
		}
		return dictionary;
	}

	private void checkinject()
	{
		if (!IsEnabled || _offsets.Count == 0)
		{
			return;
		}
		Process[] processesByName = Process.GetProcessesByName("RobloxPlayerBeta");
		HashSet<int> hashSet = new HashSet<int>(processesByName.Select((Process p) => p.Id));
		foreach (int item in _lastInjectionTimes.Keys.ToList())
		{
			if (!hashSet.Contains(item))
			{
				_lastInjectionTimes.Remove(item);
			}
		}
		Process[] array = processesByName;
		foreach (Process process in array)
		{
			bool flag = false;
			if (!_lastInjectionTimes.TryGetValue(process.Id, out var value))
			{
				flag = true;
			}
			else if ((DateTime.Now - value).TotalMinutes >= 5.0)
			{
				flag = true;
			}
			if (flag)
			{
				injectintorblx(process);
				_lastInjectionTimes[process.Id] = DateTime.Now;
			}
		}
	}

	private void injectintorblx(Process proc)
	{
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = OpenProcess(1080u, bInheritHandle: false, proc.Id);
			if (intPtr == IntPtr.Zero)
			{
				return;
			}
			ProcessModule? mainModule = proc.MainModule;
			long num;
			IntPtr lpNumberOfBytesWritten;
			if (mainModule == null)
			{
				num = 0L;
			}
			else
			{
				lpNumberOfBytesWritten = mainModule.BaseAddress;
				num = lpNumberOfBytesWritten.ToInt64();
			}
			long num2 = num;
			if (num2 == 0L)
			{
				return;
			}
			int num3 = 0;
			foreach (KeyValuePair<string, object> item in App.FastFlags.Prop)
			{
				string key = prefixfflags(item.Key);
				if (_offsets.TryGetValue(key, out var value))
				{
					long value2 = num2 + value;
					IntPtr lpBaseAddress = new IntPtr(value2);
					byte[] array = flagencode(item.Key, item.Value);
					if (array != null && WriteProcessMemory(intPtr, lpBaseAddress, array, array.Length, out lpNumberOfBytesWritten))
					{
						num3++;
					}
				}
			}
			_ = 0;
		}
		catch (Exception)
		{
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				CloseHandle(intPtr);
			}
		}
	}

	private string prefixfflags(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return string.Empty;
		}
		string[] array = new string[8] { "FFlag", "DFFlag", "FInt", "DFInt", "FLog", "DFLog", "FString", "DFString" };
		foreach (string text in array)
		{
			if (name.StartsWith(text, StringComparison.Ordinal))
			{
				return name.Substring(text.Length);
			}
		}
		return name;
	}

	private byte[]? flagencode(string fullName, object valueObj)
	{
		string text = valueObj?.ToString() ?? "";
		bool num = fullName.StartsWith("FFlag", StringComparison.Ordinal) || fullName.StartsWith("DFFlag", StringComparison.Ordinal);
		bool flag = fullName.StartsWith("FInt", StringComparison.Ordinal) || fullName.StartsWith("DFInt", StringComparison.Ordinal);
		if (num)
		{
			bool flag2 = text.Equals("True", StringComparison.OrdinalIgnoreCase);
			return new byte[1] { flag2 ? ((byte)1) : ((byte)0) };
		}
		if (flag && int.TryParse(text, out var result))
		{
			return BitConverter.GetBytes(result);
		}
		return null;
	}
}
