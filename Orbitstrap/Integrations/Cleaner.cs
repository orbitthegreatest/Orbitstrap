using System;
using System.Collections.Generic;
using System.IO;
using Orbitstrap.Enums;

namespace Orbitstrap.Integrations;

public class Cleaner
{
	private const int MaxFiles = 200;

	public static Dictionary<string, string?> Directories = new Dictionary<string, string>
	{
		{
			"OrbitstrapLogs",
			Paths.Logs
		},
		{
			"OrbitstrapCache",
			Paths.Downloads
		},
		{
			"RobloxLogs",
			Paths.RobloxLogs
		},
		{
			"RobloxCache",
			Paths.RobloxCache
		}
	};

	public static void DoCleaning()
	{
		App.Logger.WriteLine("Cleaner::DoCleaning", "Cleaner has started");
		int num = App.Settings.Prop.CleanerOptions switch
		{
			CleanerOptions.OneDay => 1, 
			CleanerOptions.OneWeek => 7, 
			CleanerOptions.OneMonth => 30, 
			CleanerOptions.TwoMonths => 60, 
			CleanerOptions.Never => int.MaxValue, 
			_ => int.MaxValue, 
		};
		DateTime threshold = DateTime.Now.AddHours(-num);
		int num2 = 0;
		foreach (KeyValuePair<string, string> directory in Directories)
		{
			string value = directory.Value;
			string key = directory.Key;
			num2 = 0;
			if (!App.Settings.Prop.CleanerDirectories.Contains(key))
			{
				App.Logger.WriteLine("Cleaner::DoCleaning", "Skipping " + key);
			}
			else
			{
				if (string.IsNullOrEmpty(value) || !Directory.Exists(value))
				{
					continue;
				}
				try
				{
					string[] array = RecursivlyGetFiles(value);
					App.Logger.WriteLine("Cleaner::DoCleaning", $"Running cleaner in {directory}, {array.Length} files found");
					string[] array2 = array;
					foreach (string text in array2)
					{
						if (VerifyFile(text, threshold))
						{
							if (num2 >= 200)
							{
								App.Logger.WriteLine("Cleaner::DoCleaning", $"Reached file threshold in {directory}, continuing to next directory");
								break;
							}
							try
							{
								File.Delete(text);
								num2++;
							}
							catch (Exception ex)
							{
								App.Logger.WriteLine("Cleaner::DoCleaning", "Unable to delete " + text);
								App.Logger.WriteException("Cleaner::DoCleaning", ex);
							}
						}
					}
				}
				catch (Exception ex2)
				{
					App.Logger.WriteLine("Cleaner::DoCleaning", "Failed to clean up " + value);
					App.Logger.WriteException("Cleaner::DoCleaning", ex2);
				}
			}
		}
		App.Logger.WriteLine("Cleaner::DoCleaning", "Cleaner finished");
	}

	private static bool VerifyFile(string file, DateTime Threshold)
	{
		if (!File.Exists(file))
		{
			return false;
		}
		if (File.GetCreationTime(file) > Threshold)
		{
			return false;
		}
		if (!file.Contains("Roblox") && !file.Contains("Orbitstrap") && !file.Contains(Paths.Base))
		{
			throw new Exception(file + " was in disallowed directory");
		}
		if (file.Contains("Windows"))
		{
			throw new Exception(file + " was in Windows directory");
		}
		return true;
	}

	private static string[] RecursivlyGetFiles(string Folder)
	{
		List<string> list = new List<string>();
		if (string.IsNullOrEmpty(Folder) || !Directory.Exists(Folder))
		{
			throw new Exception("Folder was not found");
		}
		foreach (string item in Directory.EnumerateFiles(Folder, "*.*", SearchOption.AllDirectories))
		{
			list.Add(item);
		}
		return list.ToArray();
	}
}
