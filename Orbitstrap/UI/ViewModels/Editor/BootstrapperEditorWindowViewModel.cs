using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Bootstrapper;

namespace Orbitstrap.UI.ViewModels.Editor;

public class BootstrapperEditorWindowViewModel : NotifyPropertyChangedViewModel
{
	private CustomDialog? _dialog;

	public ICommand PreviewCommand => new RelayCommand(Preview);

	public ICommand SaveCommand => new RelayCommand(Save);

	public ICommand OpenThemeFolderCommand => new RelayCommand(OpenThemeFolder);

	public Action<bool, string> ThemeSavedCallback { get; set; }

	public string Directory { get; set; } = "";

	public string Name { get; set; } = "";

	public string Title { get; set; } = "Editing \"Custom Theme\"";

	public string Code { get; set; } = "";

	public bool CodeChanged { get; set; }

	private void Preview()
	{
		try
		{
			CustomDialog customDialog = new CustomDialog();
			customDialog.ApplyCustomTheme(Name, Code);
			_dialog?.CloseBootstrapper();
			_dialog = customDialog;
			customDialog.Message = Strings.Bootstrapper_StylePreview_TextCancel;
			customDialog.CancelEnabled = true;
			customDialog.ShowBootstrapper();
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("BootstrapperEditorWindowViewModel::Preview", "Failed to preview custom theme");
			App.Logger.WriteException("BootstrapperEditorWindowViewModel::Preview", ex);
			Frontend.ShowMessageBox("Failed to preview theme: " + ex.Message, MessageBoxImage.Hand);
		}
	}

	private void Save()
	{
		string path = Path.Combine(Directory, "Theme.xml");
		try
		{
			File.WriteAllText(path, Code);
			CodeChanged = false;
			ThemeSavedCallback(arg1: true, Strings.CustomTheme_Editor_Save_Success_Description);
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("BootstrapperEditorWindowViewModel::Save", "Failed to save custom theme");
			App.Logger.WriteException("BootstrapperEditorWindowViewModel::Save", ex);
			ThemeSavedCallback(arg1: false, ex.Message);
		}
	}

	private void OpenThemeFolder()
	{
		Process.Start("explorer.exe", Directory);
	}
}
