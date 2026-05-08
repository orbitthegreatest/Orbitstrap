using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Orbitstrap.Enums;
using Orbitstrap.Models.APIs.Config;
using Orbitstrap.Models.Persistable;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Base;
using Orbitstrap.UI.Elements.Settings.Pages;
using Orbitstrap.UI.ViewModels.Settings;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace Orbitstrap.UI.Elements.Settings;

public partial class MainWindow : WpfUiWindow, INavigationWindow, IComponentConnector
{
	private Orbitstrap.Models.Persistable.WindowState _state => App.State.Prop.SettingsWindow;

	public MainWindow(bool showAlreadyRunningWarning)
	{
		MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
		mainWindowViewModel.RequestSaveNoticeEvent = (EventHandler)Delegate.Combine(mainWindowViewModel.RequestSaveNoticeEvent, (EventHandler)delegate
		{
			SettingsSavedSnackbar.Show();
		});
		mainWindowViewModel.RequestCloseWindowEvent = (EventHandler)Delegate.Combine(mainWindowViewModel.RequestCloseWindowEvent, (EventHandler)delegate
		{
			Close();
		});
		base.DataContext = mainWindowViewModel;
		InitializeComponent();
		App.Logger.WriteLine("MainWindow", "Initializing settings window");
		if (showAlreadyRunningWarning)
		{
			ShowAlreadyRunningSnackbar();
		}
		gbs.Opacity = (mainWindowViewModel.GBSEnabled ? 1.0 : 0.5);
		gbs.IsEnabled = mainWindowViewModel.GBSEnabled;
		LoadState();
		string lastPage = App.State.Prop.LastPage;
		Type type = ((lastPage == null) ? null : Type.GetType(lastPage));
		App.RemoteData.Subscribe(delegate
		{
			RemoteDataBase prop = App.RemoteData.Prop;
			AlertBar.Visibility = ((!prop.AlertEnabled) ? Visibility.Collapsed : Visibility.Visible);
			AlertBar.Message = prop.AlertContent;
			AlertBar.Severity = prop.AlertSeverity;
			if (prop.KillFlags)
			{
				fastflags.PageType = typeof(FastFlagsDisabled);
			}
		});
		if (type != null)
		{
			SafeNavigate(type);
		}
		RootNavigation.Navigated += OnNavigation;
		void OnNavigation(object? sender, RoutedNavigationEventArgs e)
		{
			INavigationItem current = RootNavigation.Current;
			App.State.Prop.LastPage = current?.PageType.FullName;
		}
	}

	public void LoadState()
	{
		if (_state.Left > SystemParameters.VirtualScreenWidth)
		{
			_state.Left = 0.0;
		}
		if (_state.Top > SystemParameters.VirtualScreenHeight)
		{
			_state.Top = 0.0;
		}
		if (_state.Width > 0.0)
		{
			base.Width = _state.Width;
		}
		if (_state.Height > 0.0)
		{
			base.Height = _state.Height;
		}
		if (_state.Left > 0.0 && _state.Top > 0.0)
		{
			base.WindowStartupLocation = WindowStartupLocation.Manual;
			base.Left = _state.Left;
			base.Top = _state.Top;
		}
	}

	private async void SafeNavigate(Type page)
	{
		await Task.Delay(500);
		if (!(page == typeof(GBSEditorPage)) || App.GlobalSettings.Loaded)
		{
			Navigate(page);
		}
	}

	private async void ShowAlreadyRunningSnackbar()
	{
		await Task.Delay(500);
		AlreadyRunningSnackbar.Show();
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

	private void WpfUiWindow_Closing(object sender, CancelEventArgs e)
	{
		if ((App.FastFlags.Changed || App.PendingSettingTasks.Any()) && Frontend.ShowMessageBox(Strings.Menu_UnsavedChanges, MessageBoxImage.Exclamation, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
		{
			e.Cancel = true;
		}
		_state.Width = base.Width;
		_state.Height = base.Height;
		_state.Top = base.Top;
		_state.Left = base.Left;
		App.State.Save();
	}

	private void WpfUiWindow_Closed(object sender, EventArgs e)
	{
		if (App.LaunchSettings.TestModeFlag.Active)
		{
			LaunchHandler.LaunchRoblox(LaunchMode.Player);
		}
		else
		{
			App.SoftTerminate();
		}
	}
}
