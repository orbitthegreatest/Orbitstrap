using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using Orbitstrap.UI.Elements.Base;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace Orbitstrap.UI.Elements.About;

public partial class MainWindow : WpfUiWindow, INavigationWindow, IComponentConnector
{
	public MainWindow()
	{
		InitializeComponent();
		App.Logger.WriteLine("MainWindow", "Initializing about window");
		if (Locale.CurrentCulture.Name.StartsWith("tr"))
		{
			TranslatorsText.FontSize = 9.0;
		}
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
