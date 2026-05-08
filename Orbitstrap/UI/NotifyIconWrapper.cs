using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using Orbitstrap.Enums;
using Orbitstrap.Integrations;
using Orbitstrap.Properties;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.ContextMenu;
using Orbitstrap.Utility;

namespace Orbitstrap.UI;

public class NotifyIconWrapper : IDisposable
{
	private bool _disposing;

	private readonly NotifyIcon _notifyIcon;

	private readonly MenuContainer _menuContainer;

	private readonly Watcher _watcher;

	private EventHandler? _alertClickHandler;

	private ActivityWatcher? _activityWatcher => _watcher.ActivityWatcher;

	public NotifyIconWrapper(Watcher watcher)
	{
		App.Logger.WriteLine("NotifyIconWrapper::NotifyIconWrapper", "Initializing notification area icon");
		_watcher = watcher;
		_notifyIcon = new NotifyIcon(new Container())
		{
			Icon = Orbitstrap.Properties.Resources.IconOrbitstrap,
			Text = "Orbitstrap",
			Visible = true
		};
		_notifyIcon.MouseClick += MouseClickEventHandler;
		if (_activityWatcher != null && (App.Settings.Prop.ShowServerDetails || App.Settings.Prop.ShowServerUptime))
		{
			_activityWatcher.OnGameJoin += OnGameJoin;
		}
		_menuContainer = new MenuContainer(_watcher);
		_menuContainer.Show();
	}

	public void MouseClickEventHandler(object? sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			_menuContainer.Activate();
			_menuContainer.ContextMenu.IsOpen = true;
		}
	}

	public async void OnGameJoin(object? sender, EventArgs e)
	{
		if (_activityWatcher == null)
		{
			return;
		}
		string title = _activityWatcher.Data.ServerType switch
		{
			ServerType.Public => Strings.ContextMenu_ServerInformation_Notification_Title_Public, 
			ServerType.Private => Strings.ContextMenu_ServerInformation_Notification_Title_Private, 
			ServerType.Reserved => Strings.ContextMenu_ServerInformation_Notification_Title_Reserved, 
			_ => "", 
		};
		bool locationActive = App.Settings.Prop.ShowServerDetails;
		bool uptimeActive = App.Settings.Prop.ShowServerUptime;
		string serverLocation = "";
		if (locationActive)
		{
			serverLocation = await _activityWatcher.Data.QueryServerLocation();
		}
		string text = "";
		if (uptimeActive)
		{
			DateTime? dateTime = await _activityWatcher.Data.QueryServerTime();
			TimeSpan timeSpan = DateTime.UtcNow - dateTime.Value;
			text = ((!(timeSpan.TotalSeconds > 60.0)) ? Strings.ContextMenu_ServerInformation_Notification_ServerNotTracked : Time.FormatTimeSpan(timeSpan));
		}
		if (!(string.IsNullOrEmpty(serverLocation) && locationActive) && !(string.IsNullOrEmpty(text) && uptimeActive))
		{
			string message = Strings.Common_UnknownStatus;
			if (locationActive && !uptimeActive)
			{
				message = string.Format(Strings.ContextMenu_ServerInformation_Notification_Text, serverLocation);
			}
			else if (!locationActive && uptimeActive)
			{
				message = string.Format(Strings.ContextMenu_ServerInformationUptime_Notification_Text, text);
			}
			else if (locationActive && uptimeActive)
			{
				message = string.Format(Strings.ContextMenu_ServerInformationUptimeAndLocation_Notification_Text, serverLocation, text);
			}
			ShowAlert(title, message, 10, delegate
			{
				_menuContainer.ShowServerInformationWindow();
			});
		}
	}

	public void ShowAlert(string caption, string message, int duration, EventHandler? clickHandler)
	{
		string text = Guid.NewGuid().ToString().Substring(0, 8);
		string LOG_IDENT = "NotifyIconWrapper::ShowAlert." + text;
		App.Logger.WriteLine(LOG_IDENT, $"Showing alert for {duration} seconds (clickHandler={clickHandler != null})");
		App.Logger.WriteLine(LOG_IDENT, caption + ": " + message.Replace("\n", "\\n"));
		_notifyIcon.BalloonTipTitle = caption;
		_notifyIcon.BalloonTipText = message;
		if (_alertClickHandler != null)
		{
			App.Logger.WriteLine(LOG_IDENT, "Previous alert still present, erasing click handler");
			_notifyIcon.BalloonTipClicked -= _alertClickHandler;
		}
		_alertClickHandler = clickHandler;
		_notifyIcon.BalloonTipClicked += clickHandler;
		_notifyIcon.ShowBalloonTip(duration);
		_ = Task.Run(async delegate
		{
			await Task.Delay(duration * 1000);
			_notifyIcon.BalloonTipClicked -= clickHandler;
			App.Logger.WriteLine(LOG_IDENT, "Duration over, erasing current click handler");
			if (_alertClickHandler == clickHandler)
			{
				_alertClickHandler = null;
			}
			else
			{
				App.Logger.WriteLine(LOG_IDENT, "Click handler has been overridden by another alert");
			}
		});
	}

	public void Dispose()
	{
		if (!_disposing)
		{
			_disposing = true;
			App.Logger.WriteLine("NotifyIconWrapper::Dispose", "Disposing NotifyIcon");
			_menuContainer.Dispatcher.Invoke(_menuContainer.Close);
			_notifyIcon.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
