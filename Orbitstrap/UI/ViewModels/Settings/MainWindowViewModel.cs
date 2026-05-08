using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Orbitstrap.Enums;
using Orbitstrap.Models.SettingTasks.Base;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.About;

namespace Orbitstrap.UI.ViewModels.Settings;

public class MainWindowViewModel : NotifyPropertyChangedViewModel
{
	public EventHandler? RequestSaveNoticeEvent;

	public EventHandler? RequestCloseWindowEvent;

	public bool GBSEnabled = App.GlobalSettings.Loaded;

	public ICommand OpenAboutCommand => new RelayCommand(OpenAbout);

	public ICommand SaveSettingsCommand => new RelayCommand(SaveSettings);

	public ICommand SaveAndLaunchSettingsCommand => new RelayCommand(SaveAndLaunchSettings);

	public ICommand CloseWindowCommand => new RelayCommand(CloseWindow);

	public bool TestModeEnabled
	{
		get
		{
			return App.LaunchSettings.TestModeFlag.Active;
		}
		set
		{
			if (value && !App.State.Prop.TestModeWarningShown)
			{
				if (Frontend.ShowMessageBox(Strings.Menu_TestMode_Prompt, MessageBoxImage.Asterisk, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
				{
					return;
				}
				App.State.Prop.TestModeWarningShown = true;
			}
			App.LaunchSettings.TestModeFlag.Active = value;
		}
	}

	private void OpenAbout()
	{
		new MainWindow().ShowDialog();
	}

	private void CloseWindow()
	{
		RequestCloseWindowEvent?.Invoke(this, EventArgs.Empty);
	}

	private void SaveSettings()
	{
		App.Settings.Save();
		App.State.Save();
		App.FastFlags.Save();
		App.GlobalSettings.Save();
		foreach (KeyValuePair<string, BaseTask> pendingSettingTask in App.PendingSettingTasks)
		{
			BaseTask value = pendingSettingTask.Value;
			if (value.Changed)
			{
				App.Logger.WriteLine("MainWindowViewModel::SaveSettings", $"Executing pending task '{value}'");
				value.Execute();
			}
		}
		App.PendingSettingTasks.Clear();
		RequestSaveNoticeEvent?.Invoke(this, EventArgs.Empty);
	}

	public void SaveAndLaunchSettings()
	{
		SaveSettings();
		if (!App.LaunchSettings.TestModeFlag.Active)
		{
			LaunchHandler.LaunchRoblox(LaunchMode.Player);
		}
		CloseWindow();
	}
}
