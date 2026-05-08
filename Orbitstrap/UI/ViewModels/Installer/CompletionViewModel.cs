using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Orbitstrap.Enums;

namespace Orbitstrap.UI.ViewModels.Installer;

public class CompletionViewModel
{
	public ICommand LaunchSettingsCommand => new RelayCommand(LaunchSettings);

	public ICommand LaunchRobloxCommand => new RelayCommand(LaunchRoblox);

	public event EventHandler<NextAction>? CloseWindowRequest;

	private void LaunchSettings()
	{
		this.CloseWindowRequest?.Invoke(this, NextAction.LaunchSettings);
	}

	private void LaunchRoblox()
	{
		this.CloseWindowRequest?.Invoke(this, NextAction.LaunchRoblox);
	}
}
