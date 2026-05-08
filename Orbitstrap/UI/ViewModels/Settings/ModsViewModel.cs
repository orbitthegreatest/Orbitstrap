using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
	// Orbitstrap Custom Skybox System (ported from Voidstrap)
	// ==========================================================

	public class SkyboxPack
	{
		public string Name { get; set; } = "Default";
		/// <summary>Full path to the local folder containing this pack's .tex files.</summary>
		public string? LocalPath { get; set; }
		public override string ToString() => Name;
	}

	// Skyboxes folder sits next to the exe: <install dir>\Skyboxes\<PackName>\sky512_*.tex
	private static string SkyboxesDir =>
		Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Skyboxes");

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
	/// Populates AvailableSkyboxPacks from the local Skyboxes\ folder bundled with the exe.
	/// No network access — reads subdirectories that contain .tex files.
	/// </summary>
	public Task LoadSkyboxPacksAsync()
	{
		try
		{
			AvailableSkyboxPacks.Clear();

			if (Directory.Exists(SkyboxesDir))
			{
				var dirs = Directory.GetDirectories(SkyboxesDir)
					.OrderBy(d => Path.GetFileName(d), StringComparer.OrdinalIgnoreCase);

				foreach (var dir in dirs)
				{
					string packName = Path.GetFileName(dir);
					if (Directory.GetFiles(dir, "*.tex").Length > 0)
						AvailableSkyboxPacks.Add(new SkyboxPack { Name = packName, LocalPath = dir });
				}
			}

			// "Default" must always be present; prepend it if it wasn't found in the folder
			if (!AvailableSkyboxPacks.Any(s => s.Name.Equals("Default", StringComparison.OrdinalIgnoreCase)))
			{
				string defaultDir = Path.Combine(SkyboxesDir, "Default");
				AvailableSkyboxPacks.Insert(0, new SkyboxPack
				{
					Name = "Default",
					LocalPath = Directory.Exists(defaultDir) ? defaultDir : null
				});
			}

			var selected = AvailableSkyboxPacks.FirstOrDefault(s =>
				s.Name.Equals(App.Settings.Prop.SkyboxName, StringComparison.OrdinalIgnoreCase))
				?? AvailableSkyboxPacks.FirstOrDefault();

			SelectedSkyboxPack = selected;
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("ModsViewModel::LoadSkyboxPacksAsync", $"Failed: {ex.Message}");
		}

		return Task.CompletedTask;
	}

}
