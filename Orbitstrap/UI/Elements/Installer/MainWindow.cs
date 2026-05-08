using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Orbitstrap.Enums;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Base;
using Orbitstrap.UI.Elements.Installer.Pages;
using Orbitstrap.UI.ViewModels.Installer;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace Orbitstrap.UI.Elements.Installer;

public partial class MainWindow : WpfUiWindow, INavigationWindow, IComponentConnector
{
	internal readonly MainWindowViewModel _viewModel = new MainWindowViewModel();

	private Type _currentPage = typeof(WelcomePage);

	private List<Type> _pages = new List<Type>
	{
		typeof(WelcomePage),
		typeof(InstallPage),
		typeof(CompletionPage)
	};

	private DateTimeOffset _lastNavigation = DateTimeOffset.Now;

	public Func<bool>? NextPageCallback;

	public NextAction CloseAction;

	public bool Finished => _currentPage == _pages.Last();

	public MainWindow()
	{
		_viewModel.CloseWindowRequest += delegate
		{
			CloseWindow();
		};
		_viewModel.PageRequest += delegate(object? _, string type)
		{
			if (!(DateTimeOffset.Now.Subtract(_lastNavigation).TotalMilliseconds < 500.0))
			{
				if (type == "next")
				{
					NextPage();
				}
				else if (type == "back")
				{
					BackPage();
				}
				_lastNavigation = DateTimeOffset.Now;
			}
		};
		base.DataContext = _viewModel;
		InitializeComponent();
		App.Logger.WriteLine("MainWindow", "Initializing installer window");
		base.Closing += MainWindow_Closing;
	}

	private void NextPage()
	{
		if ((NextPageCallback == null || NextPageCallback()) && !(_currentPage == _pages.Last()))
		{
			Type type = _pages[_pages.IndexOf(_currentPage) + 1];
			Navigate(type);
			SetButtonEnabled("next", type != _pages.Last());
			SetButtonEnabled("back", state: true);
		}
	}

	private void BackPage()
	{
		if (!(_currentPage == _pages.First()))
		{
			Type type = _pages[_pages.IndexOf(_currentPage) - 1];
			Navigate(type);
			SetButtonEnabled("next", state: true);
			SetButtonEnabled("back", type != _pages.First());
		}
	}

	private void MainWindow_Closing(object? sender, CancelEventArgs e)
	{
		if (!Finished && Frontend.ShowMessageBox(Strings.Installer_ShouldCancel, MessageBoxImage.Exclamation, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
		{
			e.Cancel = true;
		}
	}

	public void SetNextButtonText(string text)
	{
		_viewModel.SetNextButtonText(text);
	}

	public void SetButtonEnabled(string type, bool state)
	{
		_viewModel.SetButtonEnabled(type, state);
	}

	public Frame GetFrame()
	{
		return RootFrame;
	}

	public INavigation GetNavigation()
	{
		return RootNavigation;
	}

	public bool Navigate(Type pageType)
	{
		_currentPage = pageType;
		NextPageCallback = null;
		return RootNavigation.Navigate(pageType);
	}

	public void SetPageService(IPageService pageService)
	{
		RootNavigation.PageService = pageService;
	}

	public void ShowWindow()
	{
		Show();
	}

	public void CloseWindow()
	{
		Close();
	}
}
