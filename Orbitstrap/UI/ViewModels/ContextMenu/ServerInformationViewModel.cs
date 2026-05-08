using System;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Orbitstrap.Extensions;
using Orbitstrap.Integrations;
using Orbitstrap.Resources;
using Orbitstrap.Utility;

namespace Orbitstrap.UI.ViewModels.ContextMenu;

internal class ServerInformationViewModel : NotifyPropertyChangedViewModel
{
	private readonly ActivityWatcher _activityWatcher;

	public string InstanceId => _activityWatcher.Data.JobId;

	public string ServerType => _activityWatcher.Data.ServerType.ToTranslatedString();

	public string ServerLocation { get; private set; } = Strings.Common_Loading;

	public string ServerUptime { get; private set; } = Strings.Common_Loading;

	public Visibility ServerLocationVisibility
	{
		get
		{
			if (!App.Settings.Prop.ShowServerDetails)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public Visibility ServerUptimeVisibility
	{
		get
		{
			if (!App.Settings.Prop.ShowServerUptime)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public ICommand CopyInstanceIdCommand => new RelayCommand(CopyInstanceId);

	public ServerInformationViewModel(Watcher watcher)
	{
		_activityWatcher = watcher.ActivityWatcher;
		if (ServerLocationVisibility == Visibility.Visible)
		{
			QueryServerLocation();
		}
		if (ServerUptimeVisibility == Visibility.Visible)
		{
			QueryServerUptime();
		}
	}

	public async void QueryServerLocation()
	{
		string text = await _activityWatcher.Data.QueryServerLocation();
		if (string.IsNullOrEmpty(text))
		{
			ServerLocation = Strings.Common_NotAvailable;
		}
		else
		{
			ServerLocation = text;
		}
		OnPropertyChanged("ServerLocation");
	}

	public async void QueryServerUptime()
	{
		DateTime? dateTime = await _activityWatcher.Data.QueryServerTime();
		TimeSpan timeSpan = DateTime.UtcNow - dateTime.Value;
		string serverUptime = Strings.ContextMenu_ServerInformation_Notification_ServerNotTracked;
		if (timeSpan.TotalSeconds > 60.0)
		{
			serverUptime = Time.FormatTimeSpan(timeSpan);
		}
		ServerUptime = serverUptime;
		OnPropertyChanged("ServerUptime");
	}

	private void CopyInstanceId()
	{
		Clipboard.SetDataObject(InstanceId);
	}
}
