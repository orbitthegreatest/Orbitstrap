using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Orbitstrap.AppData;
using Orbitstrap.Enums;
using Orbitstrap.Models.SettingTasks;
using Orbitstrap.Resources;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace Orbitstrap.UI.ViewModels.Settings;

public class ModsViewModel : NotifyPropertyChangedViewModel
{
	private readonly Dictionary<string, byte[]> FontHeaders = new Dictionary<string, byte[]>
	{
		{
			"ttf",
			new byte[4] { 0, 1, 0, 0 }
		},
		{
			"otf",
			new byte[4] { 79, 84, 84, 79 }
		},
		{
			"ttc",
			new byte[4] { 116, 116, 99, 102 }
		}
	};

	public ICommand OpenModsFolderCommand => new RelayCommand(OpenModsFolder);

	public Visibility ChooseCustomFontVisibility
	{
		get
		{
			if (string.IsNullOrEmpty(TextFontTask.NewState))
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public Visibility DeleteCustomFontVisibility
	{
		get
		{
			if (string.IsNullOrEmpty(TextFontTask.NewState))
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public ICommand ManageCustomFontCommand => new RelayCommand(ManageCustomFont);

	public ICommand OpenCompatSettingsCommand => new RelayCommand(OpenCompatSettings);

	public ModPresetTask OldAvatarBackgroundTask { get; } = new ModPresetTask("OldAvatarBackground", "ExtraContent\\places\\Mobile.rbxl", "OldAvatarBackground.rbxl");

	public ModPresetTask OldCharacterSoundsTask { get; } = new ModPresetTask("OldCharacterSounds", new Dictionary<string, string>
	{
		{ "content\\sounds\\action_footsteps_plastic.mp3", "Sounds.OldWalk.mp3" },
		{ "content\\sounds\\action_jump.mp3", "Sounds.OldJump.mp3" },
		{ "content\\sounds\\action_get_up.mp3", "Sounds.OldGetUp.mp3" },
		{ "content\\sounds\\action_falling.mp3", "Sounds.Empty.mp3" },
		{ "content\\sounds\\action_jump_land.mp3", "Sounds.Empty.mp3" },
		{ "content\\sounds\\action_swim.mp3", "Sounds.Empty.mp3" },
		{ "content\\sounds\\impact_water.mp3", "Sounds.Empty.mp3" }
	});

	public EmojiModPresetTask EmojiFontTask { get; } = new EmojiModPresetTask();

	public EnumModPresetTask<Orbitstrap.Enums.CursorType> CursorTypeTask { get; } = new EnumModPresetTask<Orbitstrap.Enums.CursorType>("CursorType", new Dictionary<Orbitstrap.Enums.CursorType, Dictionary<string, string>>
	{
		{
			Orbitstrap.Enums.CursorType.From2006,
			new Dictionary<string, string>
			{
				{ "content\\textures\\Cursors\\KeyboardMouse\\ArrowCursor.png", "Cursor.From2006.ArrowCursor.png" },
				{ "content\\textures\\Cursors\\KeyboardMouse\\ArrowFarCursor.png", "Cursor.From2006.ArrowFarCursor.png" }
			}
		},
		{
			Orbitstrap.Enums.CursorType.From2013,
			new Dictionary<string, string>
			{
				{ "content\\textures\\Cursors\\KeyboardMouse\\ArrowCursor.png", "Cursor.From2013.ArrowCursor.png" },
				{ "content\\textures\\Cursors\\KeyboardMouse\\ArrowFarCursor.png", "Cursor.From2013.ArrowFarCursor.png" }
			}
		}
	});

	public FontModPresetTask TextFontTask { get; } = new FontModPresetTask();

	private void OpenModsFolder()
	{
		Process.Start("explorer.exe", Paths.Modifications);
	}

	private void ManageCustomFont()
	{
		if (!string.IsNullOrEmpty(TextFontTask.NewState))
		{
			TextFontTask.NewState = "";
		}
		else
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				Filter = Strings.Menu_FontFiles + "|*.ttf;*.otf;*.ttc"
			};
			if (dialog.ShowDialog() != true)
			{
				return;
			}
			string key = dialog.FileName.Substring(dialog.FileName.Length - 3, 3).ToLowerInvariant();
			if (!FontHeaders.ContainsKey(key) || !FontHeaders.Any<KeyValuePair<string, byte[]>>((KeyValuePair<string, byte[]> x) => File.ReadAllBytes(dialog.FileName).Take(4).SequenceEqual(x.Value)))
			{
				Frontend.ShowMessageBox(Strings.Menu_Mods_Misc_CustomFont_Invalid, MessageBoxImage.Hand);
				return;
			}
			TextFontTask.NewState = dialog.FileName;
		}
		OnPropertyChanged("ChooseCustomFontVisibility");
		OnPropertyChanged("DeleteCustomFontVisibility");
	}

	private void OpenCompatSettings()
	{
		string executablePath = new RobloxPlayerData().ExecutablePath;
		if (File.Exists(executablePath))
		{
			PInvoke.SHObjectProperties(HWND.Null, SHOP_TYPE.SHOP_FILEPATH, executablePath, "Compatibility");
		}
		else
		{
			Frontend.ShowMessageBox(Strings.Common_RobloxNotInstalled, MessageBoxImage.Hand);
		}
	}

	// ==========================================================
	// Skybox System (Voidstrap logic — packs from GitHub)
	// ==========================================================

	public class SkyboxPack
	{
		public string Name { get; set; } = "Default";
		public override string ToString() => Name;
	}

	private static readonly string SkyboxRepoApiUrl =
		"https://api.github.com/repos/KloBraticc/SkyboxPackV2/contents";

	private readonly HttpClient _skyboxHttp = new HttpClient();

	public ObservableCollection<SkyboxPack> AvailableSkyboxPacks { get; } = new();

	private SkyboxPack? _selectedSkyboxPack;
	public SkyboxPack? SelectedSkyboxPack
	{
		get => _selectedSkyboxPack;
		set
		{
			if (_selectedSkyboxPack == value) return;
			_selectedSkyboxPack = value;
			OnPropertyChanged(nameof(SelectedSkyboxPack));
			if (value != null) App.Settings.Prop.SkyboxName = value.Name;
		}
	}

	public bool SkyboxEnabled
	{
		get => App.Settings.Prop.SkyboxEnabled;
		set => App.Settings.Prop.SkyboxEnabled = value;
	}

	/// <summary>
	/// Populates AvailableSkyboxPacks by querying the GitHub API for folders
	/// in KloBraticc/SkyboxPackV2 — same logic as Voidstrap.
	/// </summary>
	public async Task LoadSkyboxPacksAsync()
	{
		try
		{
			_skyboxHttp.DefaultRequestHeaders.UserAgent.TryParseAdd("OrbitstrapSkyboxClient");

			var response = await _skyboxHttp.GetFromJsonAsync<System.Text.Json.JsonElement[]>(SkyboxRepoApiUrl);
			if (response == null) return;

			var folders = response
				.Where(e => e.GetProperty("type").GetString() == "dir")
				.Select(e => e.GetProperty("name").GetString()!)
				.ToList();

			AvailableSkyboxPacks.Clear();
			AvailableSkyboxPacks.Add(new SkyboxPack { Name = "Default" });

			foreach (var name in folders.Where(f => !f.Equals("Default", StringComparison.OrdinalIgnoreCase)))
				AvailableSkyboxPacks.Add(new SkyboxPack { Name = name });

			var selected = AvailableSkyboxPacks.FirstOrDefault(s =>
				s.Name.Equals(App.Settings.Prop.SkyboxName, StringComparison.OrdinalIgnoreCase))
				?? AvailableSkyboxPacks.First();

			SelectedSkyboxPack = selected;
			App.Settings.Prop.SkyboxName = selected.Name;
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("ModsViewModel::LoadSkyboxPacksAsync",
				$"Failed to load skybox packs from GitHub: {ex.Message}");

			if (!AvailableSkyboxPacks.Any())
				AvailableSkyboxPacks.Add(new SkyboxPack { Name = "Default" });
		}
	}

}
