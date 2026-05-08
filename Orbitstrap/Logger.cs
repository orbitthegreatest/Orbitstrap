using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using Orbitstrap.Resources;
using Orbitstrap.UI;

namespace Orbitstrap;

public class Logger
{
	private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

	private FileStream? _filestream;

	public readonly List<string> History = new List<string>();

	public bool Initialized;

	public bool NoWriteMode;

	public string? FileLocation;

	public string AsDocument => string.Join('\n', History);

	public void Initialize(bool useTempDir = false)
	{
		string text = (useTempDir ? Path.Combine(Paths.TempLogs) : Path.Combine(Paths.Base, "Logs"));
		string text2 = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");
		string path = "Orbitstrap_" + text2 + ".log";
		string text3 = Path.Combine(text, path);
		WriteLine("Logger::Initialize", "Initializing at " + text3);
		if (Initialized)
		{
			WriteLine("Logger::Initialize", "Failed to initialize because logger is already initialized");
			return;
		}
		Directory.CreateDirectory(text);
		if (File.Exists(text3))
		{
			WriteLine("Logger::Initialize", "Failed to initialize because log file already exists");
			return;
		}
		try
		{
			_filestream = File.Open(text3, FileMode.Create, FileAccess.Write, FileShare.Read);
		}
		catch (IOException)
		{
			WriteLine("Logger::Initialize", "Failed to initialize because log file already exists");
			return;
		}
		catch (UnauthorizedAccessException)
		{
			if (!NoWriteMode)
			{
				WriteLine("Logger::Initialize", "Failed to initialize because Orbitstrap cannot write to " + text);
				Frontend.ShowMessageBox(string.Format(Strings.Logger_NoWriteMode, text), MessageBoxImage.Exclamation);
				NoWriteMode = true;
			}
			return;
		}
		Initialized = true;
		if (History.Count > 0)
		{
			WriteToLog(string.Join("\r\n", History));
		}
		WriteLine("Logger::Initialize", "Finished initializing!");
		FileLocation = text3;
		if (!Paths.Initialized || !Directory.Exists(Paths.Logs))
		{
			return;
		}
		FileInfo[] files = new DirectoryInfo(Paths.Logs).GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			if (!(fileInfo.LastWriteTimeUtc.AddDays(7.0) > DateTime.UtcNow))
			{
				WriteLine("Logger::Initialize", "Cleaning up old log file '" + fileInfo.Name + "'");
				try
				{
					fileInfo.Delete();
				}
				catch (Exception ex3)
				{
					WriteLine("Logger::Initialize", "Failed to delete log!");
					WriteException("Logger::Initialize", ex3);
				}
			}
		}
	}

	private void WriteLine(string message)
	{
		string text = string.Concat(DateTime.UtcNow.ToString("s") + "Z", " ", message).Replace(Paths.UserProfile, "%UserProfile%", StringComparison.InvariantCultureIgnoreCase);
		WriteToLog(text);
		History.Add(text);
	}

	public void WriteLine(string identifier, string message)
	{
		WriteLine("[" + identifier + "] " + message);
	}

	public void WriteException(string identifier, Exception ex)
	{
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
		string value = "0x" + ex.HResult.ToString("X8");
		WriteLine($"[{identifier}] ({value}) {ex}");
		Thread.CurrentThread.CurrentUICulture = Locale.CurrentCulture;
	}

	private async void WriteToLog(string message)
	{
		if (!Initialized)
		{
			return;
		}
		try
		{
			await _semaphore.WaitAsync();
			await _filestream.WriteAsync(Encoding.UTF8.GetBytes(message + "\r\n"));
			_filestream.FlushAsync();
		}
		finally
		{
			_semaphore.Release();
		}
	}
}
