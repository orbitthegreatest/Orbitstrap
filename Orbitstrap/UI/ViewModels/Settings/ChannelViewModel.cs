using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Orbitstrap.Enums;
using Orbitstrap.Exceptions;
using Orbitstrap.Models;
using Orbitstrap.Models.APIs.Roblox;
using Orbitstrap.Resources;
using Orbitstrap.RobloxInterfaces;

namespace Orbitstrap.UI.ViewModels.Settings;

public class ChannelViewModel : NotifyPropertyChangedViewModel
{
	private string _oldPlayerVersionGuid = "";

	private string _oldStudioVersionGuid = "";

	public bool UpdateCheckingEnabled
	{
		get
		{
			return App.Settings.Prop.CheckForUpdates;
		}
		set
		{
			App.Settings.Prop.CheckForUpdates = value;
		}
	}

	public bool ShowLoadingError { get; set; }

	public bool ShowChannelWarning { get; set; }

	public DeployInfo? ChannelDeployInfo { get; private set; }

	public string ChannelInfoLoadingText { get; private set; }

	public string ViewChannel
	{
		get
		{
			return App.Settings.Prop.Channel;
		}
		set
		{
			value = value.Trim();
			Task.Run(() => LoadChannelDeployInfo(value));
			if (value.ToLower() == "live" || value.ToLower() == "zlive")
			{
				App.Settings.Prop.Channel = "production";
			}
			else
			{
				App.Settings.Prop.Channel = value;
			}
		}
	}

	public string ChannelHash
	{
		get
		{
			return App.Settings.Prop.ChannelHash;
		}
		set
		{
			if (Regex.Match(value, "version-(.*)").Success || string.IsNullOrEmpty(value))
			{
				App.Settings.Prop.ChannelHash = value;
			}
		}
	}

	public bool UpdateRoblox
	{
		get
		{
			return App.Settings.Prop.UpdateRoblox;
		}
		set
		{
			App.Settings.Prop.UpdateRoblox = value;
		}
	}

	public IReadOnlyDictionary<string, ChannelChangeMode> ChannelChangeModes => new Dictionary<string, ChannelChangeMode>
	{
		{
			Strings.Menu_Channel_ChangeAction_Automatic,
			ChannelChangeMode.Automatic
		},
		{
			Strings.Menu_Channel_ChangeAction_Prompt,
			ChannelChangeMode.Prompt
		},
		{
			Strings.Menu_Channel_ChangeAction_Ignore,
			ChannelChangeMode.Ignore
		}
	};

	public string SelectedChannelChangeMode
	{
		get
		{
			return ChannelChangeModes.FirstOrDefault<KeyValuePair<string, ChannelChangeMode>>((KeyValuePair<string, ChannelChangeMode> x) => x.Value == App.Settings.Prop.ChannelChangeMode).Key;
		}
		set
		{
			App.Settings.Prop.ChannelChangeMode = ChannelChangeModes[value];
		}
	}

	public bool ForceRobloxReinstallation
	{
		get
		{
			if (string.IsNullOrEmpty(App.RobloxState.Prop.Player.VersionGuid))
			{
				return string.IsNullOrEmpty(App.RobloxState.Prop.Studio.VersionGuid);
			}
			return false;
		}
		set
		{
			if (value)
			{
				_oldPlayerVersionGuid = App.RobloxState.Prop.Player.VersionGuid;
				_oldStudioVersionGuid = App.RobloxState.Prop.Studio.VersionGuid;
				App.RobloxState.Prop.Player.VersionGuid = "";
				App.RobloxState.Prop.Studio.VersionGuid = "";
			}
			else
			{
				App.RobloxState.Prop.Player.VersionGuid = _oldPlayerVersionGuid;
				App.RobloxState.Prop.Studio.VersionGuid = _oldStudioVersionGuid;
			}
		}
	}

	public ChannelViewModel()
	{
		Task.Run(() => LoadChannelDeployInfo(App.Settings.Prop.Channel));
	}

	private async Task LoadChannelDeployInfo(string channel)
	{
		ShowLoadingError = false;
		OnPropertyChanged("ShowLoadingError");
		ChannelInfoLoadingText = Strings.Menu_Channel_Switcher_Fetching;
		OnPropertyChanged("ChannelInfoLoadingText");
		ChannelDeployInfo = null;
		OnPropertyChanged("ChannelDeployInfo");
		try
		{
			ClientVersion clientVersion = await Deployment.GetInfo(channel);
			ShowChannelWarning = clientVersion.IsBehindDefaultChannel;
			OnPropertyChanged("ShowChannelWarning");
			ChannelDeployInfo = new DeployInfo
			{
				Version = clientVersion.Version,
				VersionGuid = clientVersion.VersionGuid
			};
			App.State.Prop.IgnoreOutdatedChannel = true;
			OnPropertyChanged("ChannelDeployInfo");
		}
		catch (InvalidChannelException ex)
		{
			ShowLoadingError = true;
			OnPropertyChanged("ShowLoadingError");
			if (ex.StatusCode == HttpStatusCode.Unauthorized)
			{
				ChannelInfoLoadingText = Strings.Menu_Channel_Switcher_Unauthorized;
			}
			else
			{
				ChannelInfoLoadingText = $"An http error has occured ({ex.StatusCode})";
			}
			OnPropertyChanged("ChannelInfoLoadingText");
		}
	}
}
