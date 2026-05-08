using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Orbitstrap.Resources;

namespace Orbitstrap.UI.ViewModels.Dialogs;

public class UninstallerViewModel
{
	public string Text => string.Format(Strings.Uninstaller_Text, "https://github.com/orbitthegreatest/Orbitstrap/wiki/Roblox-crashes-or-does-not-launch", Paths.Base);

	public bool KeepData { get; set; } = true;

	public ICommand ConfirmUninstallCommand => new RelayCommand(ConfirmUninstall);

	public event EventHandler? ConfirmUninstallRequest;

	private void ConfirmUninstall()
	{
		this.ConfirmUninstallRequest?.Invoke(this, new EventArgs());
	}
}
