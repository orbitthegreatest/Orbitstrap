using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Orbitstrap.UI.ViewModels.Installer;

public class InstallViewModel : NotifyPropertyChangedViewModel
{
	private readonly Orbitstrap.Installer installer = new Orbitstrap.Installer();

	private readonly string _originalInstallLocation;

	public EventHandler<bool>? SetCanContinueEvent;

	public string InstallLocation
	{
		get
		{
			return installer.InstallLocation;
		}
		set
		{
			if (!string.IsNullOrEmpty(ErrorMessage))
			{
				SetCanContinueEvent?.Invoke(this, e: true);
				installer.InstallLocationError = "";
				OnPropertyChanged("ErrorMessage");
			}
			installer.InstallLocation = value;
			OnPropertyChanged("DataFoundMessageVisibility");
		}
	}

	public Visibility DataFoundMessageVisibility
	{
		get
		{
			if (!installer.ExistingDataPresent)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public string ErrorMessage => installer.InstallLocationError;

	public bool CreateDesktopShortcuts
	{
		get
		{
			return installer.CreateDesktopShortcuts;
		}
		set
		{
			installer.CreateDesktopShortcuts = value;
		}
	}

	public bool CreateStartMenuShortcuts
	{
		get
		{
			return installer.CreateStartMenuShortcuts;
		}
		set
		{
			installer.CreateStartMenuShortcuts = value;
		}
	}

	public bool ImportSettings
	{
		get
		{
			return installer.ImportSettings;
		}
		set
		{
			installer.ImportSettings = value;
		}
	}

	public bool ImportSettingsEnabled => Directory.Exists(installer.OrbitstrapInstallDirectory);

	public bool ShowNotFound => !Directory.Exists(installer.OrbitstrapInstallDirectory);

	public ICommand BrowseInstallLocationCommand => new RelayCommand(BrowseInstallLocation);

	public ICommand ResetInstallLocationCommand => new RelayCommand(ResetInstallLocation);

	public ICommand OpenFolderCommand => new RelayCommand(OpenFolder);

	public InstallViewModel()
	{
		_originalInstallLocation = installer.InstallLocation;
	}

	public bool DoInstall()
	{
		if (!installer.CheckInstallLocation())
		{
			SetCanContinueEvent?.Invoke(this, e: false);
			OnPropertyChanged("ErrorMessage");
			return false;
		}
		installer.DoInstall();
		return true;
	}

	private void BrowseInstallLocation()
	{
		using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			InstallLocation = folderBrowserDialog.SelectedPath;
			OnPropertyChanged("InstallLocation");
		}
	}

	private void ResetInstallLocation()
	{
		InstallLocation = _originalInstallLocation;
		OnPropertyChanged("InstallLocation");
	}

	private void OpenFolder()
	{
		Process.Start("explorer.exe", Paths.Base);
	}
}
