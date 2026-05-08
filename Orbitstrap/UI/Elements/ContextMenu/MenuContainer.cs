using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using Orbitstrap.Enums;
using Orbitstrap.Integrations;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Base;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Orbitstrap.UI.Elements.ContextMenu;

public partial class MenuContainer : WpfUiWindow, IComponentConnector
{
	private readonly Watcher _watcher;

	private ServerInformation? _serverInformationWindow;

	private ServerHistory? _gameHistoryWindow;

	private ActivityWatcher? _activityWatcher => _watcher.ActivityWatcher;

	public MenuContainer(Watcher watcher)
	{
		InitializeComponent();
		_watcher = watcher;
		if (_activityWatcher != null)
		{
			_activityWatcher.OnLogOpen += ActivityWatcher_OnLogOpen;
			_activityWatcher.OnGameJoin += ActivityWatcher_OnGameJoin;
			_activityWatcher.OnGameLeave += ActivityWatcher_OnGameLeave;
			if (!App.Settings.Prop.UseDisableAppPatch)
			{
				GameHistoryMenuItem.Visibility = Visibility.Visible;
			}
		}
		if (_watcher.RichPresence != null)
		{
			RichPresenceMenuItem.Visibility = Visibility.Visible;
		}
		VersionTextBlock.Text = "Orbitstrap v" + App.Version;
	}

	public void ShowServerInformationWindow()
	{
		if (_serverInformationWindow == null)
		{
			_serverInformationWindow = new ServerInformation(_watcher);
			_serverInformationWindow.Closed += delegate
			{
				_serverInformationWindow = null;
			};
		}
		if (!_serverInformationWindow.IsVisible)
		{
			_serverInformationWindow.ShowDialog();
		}
		else
		{
			_serverInformationWindow.Activate();
		}
	}

	public void ActivityWatcher_OnLogOpen(object? sender, EventArgs e)
	{
		base.Dispatcher.Invoke(() => LogTracerMenuItem.Visibility = Visibility.Visible);
	}

	public void ActivityWatcher_OnGameJoin(object? sender, EventArgs e)
	{
		if (_activityWatcher == null)
		{
			return;
		}
		base.Dispatcher.Invoke(delegate
		{
			if (_activityWatcher.Data.ServerType == ServerType.Public)
			{
				InviteDeeplinkMenuItem.Visibility = Visibility.Visible;
			}
			ServerDetailsMenuItem.Visibility = Visibility.Visible;
		});
	}

	public void ActivityWatcher_OnGameLeave(object? sender, EventArgs e)
	{
		base.Dispatcher.Invoke(delegate
		{
			InviteDeeplinkMenuItem.Visibility = Visibility.Collapsed;
			ServerDetailsMenuItem.Visibility = Visibility.Collapsed;
			_serverInformationWindow?.Close();
		});
	}

	private void Window_Loaded(object? sender, RoutedEventArgs e)
	{
		HWND hWnd = (HWND)new WindowInteropHelper(this).Handle;
		int windowLong = PInvoke.GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
		windowLong |= 0x80;
		PInvoke.SetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, windowLong);
	}

	private void Window_Closed(object sender, EventArgs e)
	{
		App.Logger.WriteLine("MenuContainer::Window_Closed", "Context menu container closed");
	}

	private void RichPresenceMenuItem_Click(object sender, RoutedEventArgs e)
	{
		_watcher.RichPresence?.SetVisibility(((MenuItem)sender).IsChecked);
	}

	private void InviteDeeplinkMenuItem_Click(object sender, RoutedEventArgs e)
	{
		Clipboard.SetDataObject(_activityWatcher?.Data.GetInviteDeeplink());
	}

	private void ServerDetailsMenuItem_Click(object sender, RoutedEventArgs e)
	{
		ShowServerInformationWindow();
	}

	private void LogTracerMenuItem_Click(object sender, RoutedEventArgs e)
	{
		string text = _activityWatcher?.LogLocation;
		if (text != null)
		{
			Utilities.ShellExecute(text);
		}
	}

	private void CloseRobloxMenuItem_Click(object sender, RoutedEventArgs e)
	{
		if (Frontend.ShowMessageBox(Strings.ContextMenu_CloseRobloxMessage, MessageBoxImage.Exclamation, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
		{
			_watcher.KillRobloxProcess();
		}
	}

	private void JoinLastServerMenuItem_Click(object sender, RoutedEventArgs e)
	{
		if (_activityWatcher == null)
		{
			throw new ArgumentNullException("_activityWatcher");
		}
		if (_gameHistoryWindow == null)
		{
			_gameHistoryWindow = new ServerHistory(_activityWatcher);
			_gameHistoryWindow.Closed += delegate
			{
				_gameHistoryWindow = null;
			};
		}
		if (!_gameHistoryWindow.IsVisible)
		{
			_gameHistoryWindow.ShowDialog();
		}
		else
		{
			_gameHistoryWindow.Activate();
		}
	}
}
