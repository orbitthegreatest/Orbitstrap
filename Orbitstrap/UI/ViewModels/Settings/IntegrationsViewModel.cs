using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Orbitstrap.Models;
using Orbitstrap.Resources;

namespace Orbitstrap.UI.ViewModels.Settings;

public class IntegrationsViewModel : NotifyPropertyChangedViewModel
{
	public ICommand AddIntegrationCommand => new RelayCommand(AddIntegration);

	public ICommand DeleteIntegrationCommand => new RelayCommand(DeleteIntegration);

	public ICommand BrowseIntegrationLocationCommand => new RelayCommand(BrowseIntegrationLocation);

	public bool ActivityTrackingEnabled
	{
		get
		{
			return App.Settings.Prop.EnableActivityTracking;
		}
		set
		{
			App.Settings.Prop.EnableActivityTracking = value;
			if (!value)
			{
				ShowServerDetailsEnabled = value;
				DisableAppPatchEnabled = value;
				DiscordActivityEnabled = value;
				DiscordActivityJoinEnabled = value;
				OnPropertyChanged("ShowServerDetailsEnabled");
				OnPropertyChanged("DisableAppPatchEnabled");
				OnPropertyChanged("DiscordActivityEnabled");
				OnPropertyChanged("DiscordActivityJoinEnabled");
			}
		}
	}

	public bool ShowServerDetailsEnabled
	{
		get
		{
			return App.Settings.Prop.ShowServerDetails;
		}
		set
		{
			App.Settings.Prop.ShowServerDetails = value;
		}
	}

	public bool ShowServerUptimeEnabled
	{
		get
		{
			return App.Settings.Prop.ShowServerUptime;
		}
		set
		{
			App.Settings.Prop.ShowServerUptime = value;
		}
	}

	public bool DiscordActivityEnabled
	{
		get
		{
			return App.Settings.Prop.UseDiscordRichPresence;
		}
		set
		{
			App.Settings.Prop.UseDiscordRichPresence = value;
			if (!value)
			{
				DiscordActivityJoinEnabled = value;
				DiscordAccountOnProfile = value;
				OnPropertyChanged("DiscordActivityJoinEnabled");
				OnPropertyChanged("DiscordAccountOnProfile");
			}
		}
	}

	public bool DiscordActivityJoinEnabled
	{
		get
		{
			return !App.Settings.Prop.HideRPCButtons;
		}
		set
		{
			App.Settings.Prop.HideRPCButtons = !value;
		}
	}

	public bool DiscordAccountOnProfile
	{
		get
		{
			return App.Settings.Prop.ShowAccountOnRichPresence;
		}
		set
		{
			App.Settings.Prop.ShowAccountOnRichPresence = value;
		}
	}

	public bool DisableAppPatchEnabled
	{
		get
		{
			return App.Settings.Prop.UseDisableAppPatch;
		}
		set
		{
			App.Settings.Prop.UseDisableAppPatch = value;
		}
	}

	public bool MultiInstanceLaunchingEnabled
	{
		get
		{
			return App.Settings.Prop.MultiInstanceLaunching;
		}
		set
		{
			App.Settings.Prop.MultiInstanceLaunching = value;
		}
	}

	public ObservableCollection<CustomIntegration> CustomIntegrations
	{
		get
		{
			return App.Settings.Prop.CustomIntegrations;
		}
		set
		{
			App.Settings.Prop.CustomIntegrations = value;
		}
	}

	public CustomIntegration? SelectedCustomIntegration { get; set; }

	public int SelectedCustomIntegrationIndex { get; set; }

	public bool IsCustomIntegrationSelected => SelectedCustomIntegration != null;

	private void AddIntegration()
	{
		CustomIntegrations.Add(new CustomIntegration
		{
			Name = Strings.Menu_Integrations_Custom_NewIntegration
		});
		SelectedCustomIntegrationIndex = CustomIntegrations.Count - 1;
		OnPropertyChanged("SelectedCustomIntegrationIndex");
		OnPropertyChanged("IsCustomIntegrationSelected");
	}

	private void DeleteIntegration()
	{
		if (SelectedCustomIntegration != null)
		{
			CustomIntegrations.Remove(SelectedCustomIntegration);
			if (CustomIntegrations.Count > 0)
			{
				SelectedCustomIntegrationIndex = CustomIntegrations.Count - 1;
				OnPropertyChanged("SelectedCustomIntegrationIndex");
			}
			OnPropertyChanged("IsCustomIntegrationSelected");
		}
	}

	private void BrowseIntegrationLocation()
	{
		if (SelectedCustomIntegration != null)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = Strings.Menu_AllFiles + "|*.*"
			};
			if (openFileDialog.ShowDialog() == true)
			{
				SelectedCustomIntegration.Name = openFileDialog.SafeFileName;
				SelectedCustomIntegration.Location = openFileDialog.FileName;
				OnPropertyChanged("SelectedCustomIntegration");
			}
		}
	}
}
