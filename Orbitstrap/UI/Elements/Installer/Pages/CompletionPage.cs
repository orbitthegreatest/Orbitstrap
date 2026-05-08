using System.Windows;
using System.Windows.Markup;
using Orbitstrap.Enums;
using Orbitstrap.Resources;
using Orbitstrap.UI.ViewModels.Installer;
using Wpf.Ui.Controls;

namespace Orbitstrap.UI.Elements.Installer.Pages;

public partial class CompletionPage : UiPage, IComponentConnector
{
	private readonly CompletionViewModel _viewModel = new CompletionViewModel();

	public CompletionPage()
	{
		_viewModel.CloseWindowRequest += delegate(object? _, NextAction closeAction)
		{
			if (Window.GetWindow(this) is MainWindow mainWindow)
			{
				mainWindow.CloseAction = closeAction;
				mainWindow.Close();
			}
		};
		base.DataContext = _viewModel;
		InitializeComponent();
	}

	private void UiPage_Loaded(object sender, RoutedEventArgs e)
	{
		if (Window.GetWindow(this) is MainWindow mainWindow)
		{
			mainWindow.SetNextButtonText(Strings.Common_Navigation_Next);
			mainWindow.SetButtonEnabled("back", state: false);
		}
	}
}
