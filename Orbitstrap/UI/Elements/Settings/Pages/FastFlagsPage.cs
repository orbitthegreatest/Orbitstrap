using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Orbitstrap.UI.ViewModels.Settings;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;

namespace Orbitstrap.UI.Elements.Settings.Pages;

public partial class FastFlagsPage : UiPage, IComponentConnector
{
	private bool _initialLoad;

	private FastFlagsViewModel _viewModel;

	public FastFlagsPage()
	{
		SetupViewModel();
		InitializeComponent();
	}

	private void SetupViewModel()
	{
		_viewModel = new FastFlagsViewModel();
		_viewModel.OpenFlagEditorEvent += OpenFlagEditor;
		_viewModel.RequestPageReloadEvent += delegate
		{
			SetupViewModel();
		};
		base.DataContext = _viewModel;
	}

	private void OpenFlagEditor(object? sender, EventArgs e)
	{
		if (Window.GetWindow(this) is INavigationWindow navigationWindow)
		{
			navigationWindow.Navigate(typeof(FastFlagEditorPage));
		}
	}

	private void Page_Loaded(object sender, RoutedEventArgs e)
	{
		if (!_initialLoad)
		{
			_initialLoad = true;
		}
		else
		{
			SetupViewModel();
		}
	}

	private void ValidateInt32(object sender, TextCompositionEventArgs e)
	{
		e.Handled = e.Text != "-" && !int.TryParse(e.Text, out var _);
	}

	private void ValidateUInt32(object sender, TextCompositionEventArgs e)
	{
		e.Handled = !uint.TryParse(e.Text, out var _);
	}
}
