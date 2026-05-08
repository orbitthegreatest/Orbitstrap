using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using Orbitstrap.Resources;
using Orbitstrap.UI;
using Orbitstrap.Utility;

namespace Orbitstrap;

public class JsonManager<T> where T : class, new()
{
	public T OriginalProp { get; set; } = new T();

	public T Prop { get; set; } = new T();

	public string? LastFileHash { get; private set; }

	public bool Loaded { get; set; }

	public virtual string ClassName => typeof(T).Name;

	public virtual string ProfilesLocation => Path.Combine(Paths.Base, "Profiles.json");

	public virtual string FileLocation => Path.Combine(Paths.Base, ClassName + ".json");

	public virtual string LOG_IDENT_CLASS => "JsonManager<" + ClassName + ">";

	public virtual void Load(bool alertFailure = true)
	{
		string identifier = LOG_IDENT_CLASS + "::Load";
		App.Logger.WriteLine(identifier, "Loading from " + FileLocation + "...");
		try
		{
			string text = File.ReadAllText(FileLocation);
			T val = JsonSerializer.Deserialize<T>(text);
			if (val == null)
			{
				throw new ArgumentNullException("Deserialization returned null");
			}
			Prop = val;
			Loaded = true;
			LastFileHash = MD5Hash.FromString(text);
			App.Logger.WriteLine(identifier, "Loaded successfully!");
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine(identifier, "Failed to load!");
			App.Logger.WriteException(identifier, ex);
			if (alertFailure)
			{
				string text2 = "";
				if (ClassName == "Settings")
				{
					text2 = Strings.JsonManager_SettingsLoadFailed;
				}
				else if (ClassName == "FastFlagManager")
				{
					text2 = Strings.JsonManager_FastFlagsLoadFailed;
				}
				if (!string.IsNullOrEmpty(text2))
				{
					Frontend.ShowMessageBox(text2 + "\n\n" + ex.Message, MessageBoxImage.Exclamation);
				}
				try
				{
					File.Copy(FileLocation, FileLocation + ".bak", overwrite: true);
				}
				catch (Exception ex2)
				{
					App.Logger.WriteLine(identifier, "Failed to create backup file: " + FileLocation + ".bak");
					App.Logger.WriteException(identifier, ex2);
				}
			}
			Save();
		}
	}

	public virtual void Save()
	{
		string identifier = LOG_IDENT_CLASS + "::Save";
		App.Logger.WriteLine(identifier, "Saving to " + FileLocation + "...");
		Directory.CreateDirectory(Path.GetDirectoryName(FileLocation));
		try
		{
			string text = JsonSerializer.Serialize(Prop, new JsonSerializerOptions
			{
				WriteIndented = true
			});
			File.WriteAllText(FileLocation, text);
			LastFileHash = MD5Hash.FromString(text);
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			App.Logger.WriteLine(identifier, "Failed to save");
			App.Logger.WriteException(identifier, ex);
			Frontend.ShowMessageBox(string.Format(Strings.Bootstrapper_JsonManagerSaveFailed, ClassName, ex.Message), MessageBoxImage.Exclamation);
			return;
		}
		App.Logger.WriteLine(identifier, "Save complete!");
	}

	public bool HasFileOnDiskChanged()
	{
		return LastFileHash != MD5Hash.FromFile(FileLocation);
	}
}
