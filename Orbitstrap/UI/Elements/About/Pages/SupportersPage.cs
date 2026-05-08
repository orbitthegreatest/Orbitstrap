using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Orbitstrap.UI.ViewModels.About;
using Wpf.Ui.Controls;

namespace Orbitstrap.UI.Elements.About.Pages;

public partial class SupportersPage : UiPage, IComponentConnector
{
	private readonly SupportersViewModel _viewModel = new SupportersViewModel();

	public SupportersPage()
	{
		base.DataContext = _viewModel;
		InitializeComponent();
	}

	private void UiPage_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		_viewModel.WindowResizeEvent?.Invoke(sender, e);
	}
}
