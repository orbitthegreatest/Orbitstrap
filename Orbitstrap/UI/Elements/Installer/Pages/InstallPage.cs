using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Orbitstrap.Resources;
using Orbitstrap.UI.ViewModels.Installer;
using Wpf.Ui.Controls;

namespace Orbitstrap.UI.Elements.Installer.Pages;

public partial class InstallPage : UiPage, IComponentConnector
{
	private readonly InstallViewModel _viewModel = new InstallViewModel();

	public InstallPage()
	{
		base.DataContext = _viewModel;
		InstallViewModel viewModel = _viewModel;
		viewModel.SetCanContinueEvent = (EventHandler<bool>)Delegate.Combine(viewModel.SetCanContinueEvent, (EventHandler<bool>)delegate(object? _, bool state)
		{
			if (Window.GetWindow(this) is MainWindow mainWindow)
			{
				mainWindow.SetButtonEnabled("next", state);
			}
		});
		InitializeComponent();
	}

	private void UiPage_Loaded(object sender, RoutedEventArgs e)
	{
		if (Window.GetWindow(this) is MainWindow mainWindow)
		{
			mainWindow.SetNextButtonText(Strings.Common_Navigation_Install);
			mainWindow.NextPageCallback = (Func<bool>)Delegate.Combine(mainWindow.NextPageCallback, new Func<bool>(NextPageCallback));
		}
	}

	public bool NextPageCallback()
	{
		return _viewModel.DoInstall();
	}
}
