using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Orbitstrap.Resources;
using Orbitstrap.UI.ViewModels.Installer;
using Wpf.Ui.Controls;

namespace Orbitstrap.UI.Elements.Installer.Pages;

public partial class WelcomePage : UiPage, IComponentConnector
{
	private readonly WelcomeViewModel _viewModel = new WelcomeViewModel();

	public WelcomePage()
	{
		if (Window.GetWindow(this) is MainWindow mainWindow)
		{
			mainWindow.SetButtonEnabled("next", state: true);
		}
		base.DataContext = _viewModel;
		InitializeComponent();
	}

	private void UiPage_Loaded(object sender, RoutedEventArgs e)
	{
		if (Window.GetWindow(this) is MainWindow mainWindow)
		{
			mainWindow.SetNextButtonText(Strings.Common_Navigation_Next);
		}
	}
}
