using System;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Orbitstrap.Enums;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.About;

namespace Orbitstrap.UI.ViewModels.Installer;

public class LaunchMenuViewModel
{
	public string Version => string.Format(Strings.Menu_About_Version, App.Version);

	public Visibility RobloxStudioOptionVisibility
	{
		get
		{
			if (!App.IsStudioVisible)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public ICommand LaunchSettingsCommand => new RelayCommand(LaunchSettings);

	public ICommand LaunchRobloxCommand => new RelayCommand(LaunchRoblox);

	public ICommand LaunchRobloxStudioCommand => new RelayCommand(LaunchRobloxStudio);

	public ICommand LaunchAboutCommand => new RelayCommand(LaunchAbout);

	public event EventHandler<NextAction>? CloseWindowRequest;

	private void LaunchSettings()
	{
		this.CloseWindowRequest?.Invoke(this, NextAction.LaunchSettings);
	}

	private void LaunchRoblox()
	{
		this.CloseWindowRequest?.Invoke(this, NextAction.LaunchRoblox);
	}

	private void LaunchRobloxStudio()
	{
		this.CloseWindowRequest?.Invoke(this, NextAction.LaunchRobloxStudio);
	}

	private void LaunchAbout()
	{
		new MainWindow().ShowDialog();
	}
}
