using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using Orbitstrap.Enums;
using Orbitstrap.Extensions;
using Orbitstrap.Models;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Dialogs;
using Orbitstrap.UI.Elements.Editor;
using Orbitstrap.UI.Elements.Settings;
using Orbitstrap.Utility;

namespace Orbitstrap.UI.ViewModels.Settings;

public class AppearanceViewModel : NotifyPropertyChangedViewModel
{
	private readonly Page _page;

	public ICommand PreviewBootstrapperCommand => new RelayCommand(PreviewBootstrapper);

	public ICommand BrowseCustomIconLocationCommand => new RelayCommand(BrowseCustomIconLocation);

	public ICommand AddCustomThemeCommand => new RelayCommand(AddCustomTheme);

	public ICommand DeleteCustomThemeCommand => new RelayCommand(DeleteCustomTheme);

	public ICommand RenameCustomThemeCommand => new RelayCommand(RenameCustomTheme);

	public ICommand EditCustomThemeCommand => new RelayCommand(EditCustomTheme);

	public ICommand ExportCustomThemeCommand => new RelayCommand(ExportCustomTheme);

	public IEnumerable<Theme> Themes { get; } = Enum.GetValues(typeof(Theme)).Cast<Theme>();

	public Theme Theme
	{
		get
		{
			return App.Settings.Prop.Theme;
		}
		set
		{
			App.Settings.Prop.Theme = value;
			((MainWindow)Window.GetWindow(_page)).ApplyTheme();
		}
	}

	public static List<string> Languages => Locale.GetLanguages();

	public string SelectedLanguage
	{
		get
		{
			return Locale.SupportedLocales[App.Settings.Prop.Locale];
		}
		set
		{
			App.Settings.Prop.Locale = Locale.GetIdentifierFromName(value);
		}
	}

	public IEnumerable<BootstrapperStyle> Dialogs { get; } = BootstrapperStyleEx.Selections;

	public BootstrapperStyle Dialog
	{
		get
		{
			return App.Settings.Prop.BootstrapperStyle;
		}
		set
		{
			App.Settings.Prop.BootstrapperStyle = value;
			OnPropertyChanged("CustomThemesExpanded");
		}
	}

	public bool CustomThemesExpanded => App.Settings.Prop.BootstrapperStyle == BootstrapperStyle.CustomDialog;

	public ObservableCollection<BootstrapperIconEntry> Icons { get; set; } = new ObservableCollection<BootstrapperIconEntry>();

	public BootstrapperIcon Icon
	{
		get
		{
			return App.Settings.Prop.BootstrapperIcon;
		}
		set
		{
			App.Settings.Prop.BootstrapperIcon = value;
		}
	}

	public string Title
	{
		get
		{
			return App.Settings.Prop.BootstrapperTitle;
		}
		set
		{
			App.Settings.Prop.BootstrapperTitle = value;
		}
	}

	public string CustomIconLocation
	{
		get
		{
			return App.Settings.Prop.BootstrapperIconCustomLocation;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				if (App.Settings.Prop.BootstrapperIcon == BootstrapperIcon.IconCustom)
				{
					App.Settings.Prop.BootstrapperIcon = BootstrapperIcon.IconOrbitstrap;
				}
			}
			else
			{
				App.Settings.Prop.BootstrapperIcon = BootstrapperIcon.IconCustom;
			}
			App.Settings.Prop.BootstrapperIconCustomLocation = value;
			OnPropertyChanged("Icon");
			OnPropertyChanged("Icons");
		}
	}

	public string? SelectedCustomTheme
	{
		get
		{
			return App.Settings.Prop.SelectedCustomTheme;
		}
		set
		{
			App.Settings.Prop.SelectedCustomTheme = value;
		}
	}

	public string SelectedCustomThemeName { get; set; } = "";

	public int SelectedCustomThemeIndex { get; set; }

	public ObservableCollection<string> CustomThemes { get; set; } = new ObservableCollection<string>();

	public bool IsCustomThemeSelected => SelectedCustomTheme != null;

	private void PreviewBootstrapper()
	{
		IBootstrapperDialog bootstrapperDialog = App.Settings.Prop.BootstrapperStyle.GetNew();
		if (App.Settings.Prop.BootstrapperStyle == BootstrapperStyle.ByfronDialog)
		{
			bootstrapperDialog.Message = Strings.Bootstrapper_StylePreview_ImageCancel;
		}
		else
		{
			bootstrapperDialog.Message = Strings.Bootstrapper_StylePreview_TextCancel;
		}
		bootstrapperDialog.CancelEnabled = true;
		bootstrapperDialog.ShowBootstrapper();
	}

	private void BrowseCustomIconLocation()
	{
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Filter = Strings.Menu_IconFiles + "|*.ico"
		};
		if (openFileDialog.ShowDialog() == true)
		{
			CustomIconLocation = openFileDialog.FileName;
			OnPropertyChanged("CustomIconLocation");
		}
	}

	public AppearanceViewModel(Page page)
	{
		_page = page;
		foreach (BootstrapperIcon selection in BootstrapperIconEx.Selections)
		{
			Icons.Add(new BootstrapperIconEntry
			{
				IconType = selection
			});
		}
		PopulateCustomThemes();
	}

	private void DeleteCustomThemeStructure(string name)
	{
		Directory.Delete(Path.Combine(Paths.CustomThemes, name), recursive: true);
	}

	private void RenameCustomThemeStructure(string oldName, string newName)
	{
		string sourceDirName = Path.Combine(Paths.CustomThemes, oldName);
		string destDirName = Path.Combine(Paths.CustomThemes, newName);
		Directory.Move(sourceDirName, destDirName);
	}

	private void AddCustomTheme()
	{
		AddCustomThemeDialog addCustomThemeDialog = new AddCustomThemeDialog();
		addCustomThemeDialog.ShowDialog();
		if (addCustomThemeDialog.Created)
		{
			CustomThemes.Add(addCustomThemeDialog.ThemeName);
			SelectedCustomThemeIndex = CustomThemes.Count - 1;
			OnPropertyChanged("SelectedCustomThemeIndex");
			OnPropertyChanged("IsCustomThemeSelected");
			if (addCustomThemeDialog.OpenEditor)
			{
				EditCustomTheme();
			}
		}
	}

	private void DeleteCustomTheme()
	{
		if (SelectedCustomTheme != null)
		{
			try
			{
				DeleteCustomThemeStructure(SelectedCustomTheme);
			}
			catch (Exception ex)
			{
				App.Logger.WriteException("AppearanceViewModel::DeleteCustomTheme", ex);
				Frontend.ShowMessageBox(string.Format(Strings.Menu_Appearance_CustomThemes_DeleteFailed, SelectedCustomTheme, ex.Message), MessageBoxImage.Hand);
				return;
			}
			CustomThemes.Remove(SelectedCustomTheme);
			if (CustomThemes.Any())
			{
				SelectedCustomThemeIndex = CustomThemes.Count - 1;
				OnPropertyChanged("SelectedCustomThemeIndex");
			}
			OnPropertyChanged("IsCustomThemeSelected");
		}
	}

	private void RenameCustomTheme()
	{
		if (SelectedCustomTheme == null || SelectedCustomTheme == SelectedCustomThemeName)
		{
			return;
		}
		if (string.IsNullOrEmpty(SelectedCustomThemeName))
		{
			Frontend.ShowMessageBox(Strings.CustomTheme_Add_Errors_NameEmpty, MessageBoxImage.Hand);
			return;
		}
		PathValidator.ValidationResult validationResult = PathValidator.IsFileNameValid(SelectedCustomThemeName);
		switch (validationResult)
		{
		case PathValidator.ValidationResult.IllegalCharacter:
			Frontend.ShowMessageBox(Strings.CustomTheme_Add_Errors_NameIllegalCharacters, MessageBoxImage.Hand);
			break;
		case PathValidator.ValidationResult.ReservedFileName:
			Frontend.ShowMessageBox(Strings.CustomTheme_Add_Errors_NameReserved, MessageBoxImage.Hand);
			break;
		default:
			App.Logger.WriteLine("AppearanceViewModel::RenameCustomTheme", $"Got unhandled PathValidator::ValidationResult {validationResult}");
			Frontend.ShowMessageBox(Strings.CustomTheme_Add_Errors_Unknown, MessageBoxImage.Hand);
			break;
		case PathValidator.ValidationResult.Ok:
		{
			if (File.Exists(Path.Combine(Paths.CustomThemes, SelectedCustomThemeName, "Theme.xml")))
			{
				Frontend.ShowMessageBox(Strings.CustomTheme_Add_Errors_NameTaken, MessageBoxImage.Hand);
				break;
			}
			try
			{
				RenameCustomThemeStructure(SelectedCustomTheme, SelectedCustomThemeName);
			}
			catch (Exception ex)
			{
				App.Logger.WriteException("AppearanceViewModel::RenameCustomTheme", ex);
				Frontend.ShowMessageBox(string.Format(Strings.Menu_Appearance_CustomThemes_RenameFailed, SelectedCustomTheme, ex.Message), MessageBoxImage.Hand);
				break;
			}
			int num = CustomThemes.IndexOf(SelectedCustomTheme);
			CustomThemes[num] = SelectedCustomThemeName;
			SelectedCustomThemeIndex = num;
			OnPropertyChanged("SelectedCustomThemeIndex");
			break;
		}
		}
	}

	private void EditCustomTheme()
	{
		if (SelectedCustomTheme != null)
		{
			new BootstrapperEditorWindow(SelectedCustomTheme).ShowDialog();
		}
	}

	private void ExportCustomTheme()
	{
		if (SelectedCustomTheme == null)
		{
			return;
		}
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			FileName = SelectedCustomTheme + ".zip",
			Filter = Strings.FileTypes_ZipArchive + "|*.zip"
		};
		if (saveFileDialog.ShowDialog() != true)
		{
			return;
		}
		string text = Path.Combine(Paths.CustomThemes, SelectedCustomTheme);
		using MemoryStream memoryStream = new MemoryStream();
		using ZipOutputStream zipOutputStream = new ZipOutputStream(memoryStream);
		foreach (string item in Directory.EnumerateFiles(text, "*.*", SearchOption.AllDirectories))
		{
			string current;
			string path = (current = item);
			int num = text.Length + 1;
			zipOutputStream.PutNextEntry(new ZipEntry(current.Substring(num, current.Length - num))
			{
				DateTime = DateTime.Now
			});
			using FileStream fileStream = File.OpenRead(path);
			fileStream.CopyTo(zipOutputStream);
		}
		zipOutputStream.CloseEntry();
		zipOutputStream.Finish();
		memoryStream.Position = 0L;
		using FileStream destination = File.OpenWrite(saveFileDialog.FileName);
		memoryStream.CopyTo(destination);
		Process.Start("explorer.exe", "/select,\"" + saveFileDialog.FileName + "\"");
	}

	private void PopulateCustomThemes()
	{
		string selectedCustomTheme = App.Settings.Prop.SelectedCustomTheme;
		Directory.CreateDirectory(Paths.CustomThemes);
		string[] directories = Directory.GetDirectories(Paths.CustomThemes);
		foreach (string text in directories)
		{
			if (File.Exists(Path.Combine(text, "Theme.xml")))
			{
				string fileName = Path.GetFileName(text);
				CustomThemes.Add(fileName);
			}
		}
		if (selectedCustomTheme != null)
		{
			int num = CustomThemes.IndexOf(selectedCustomTheme);
			if (num != -1)
			{
				SelectedCustomThemeIndex = num;
				OnPropertyChanged("SelectedCustomThemeIndex");
			}
			else
			{
				SelectedCustomTheme = null;
			}
		}
	}
}
