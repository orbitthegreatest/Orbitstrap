using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Orbitstrap.Enums;
using Orbitstrap.Integrations;
using Orbitstrap.Models.Entities;

namespace Orbitstrap.UI.ViewModels.ContextMenu;

internal class ServerHistoryViewModel : NotifyPropertyChangedViewModel
{
	private readonly ActivityWatcher _activityWatcher;

	public EventHandler? RequestCloseEvent;

	public List<ActivityData>? GameHistory { get; private set; }

	public GenericTriState LoadState { get; private set; } = GenericTriState.Unknown;

	public string Error { get; private set; } = string.Empty;

	public ICommand CloseWindowCommand => new RelayCommand(RequestClose);

	public ServerHistoryViewModel(ActivityWatcher activityWatcher)
	{
		_activityWatcher = activityWatcher;
		_activityWatcher.OnGameLeave += delegate
		{
			LoadData();
		};
		LoadData();
	}

	private async void LoadData()
	{
		LoadState = GenericTriState.Unknown;
		OnPropertyChanged("LoadState");
		IEnumerable<ActivityData> entries = _activityWatcher.History.Where((ActivityData x) => x.UniverseDetails == null);
		if (entries.Any())
		{
			string ids = string.Join(',', entries.Select((ActivityData x) => x.UniverseId).Distinct());
			try
			{
				await UniverseDetails.FetchBulk(ids);
			}
			catch (Exception ex)
			{
				App.Logger.WriteException("ServerHistoryViewModel::LoadData", ex);
				Error = ex.Message;
				OnPropertyChanged("Error");
				LoadState = GenericTriState.Failed;
				OnPropertyChanged("LoadState");
				return;
			}
			foreach (ActivityData item in entries)
			{
				item.UniverseDetails = UniverseDetails.LoadFromCache(item.UniverseId);
			}
		}
		GameHistory = new List<ActivityData>(_activityWatcher.History);
		List<ActivityData> list = new List<ActivityData>();
		foreach (ActivityData item2 in _activityWatcher.History)
		{
			if (item2.RootActivity != null)
			{
				if (item2.RootActivity.TimeLeft < item2.TimeLeft)
				{
					item2.RootActivity.TimeLeft = item2.TimeLeft;
				}
				if (item2.ServerType == ServerType.Public && !list.Contains(item2))
				{
					item2.RootActivity.JobId = item2.JobId;
					list.Add(item2);
				}
				GameHistory.Remove(item2);
			}
		}
		OnPropertyChanged("GameHistory");
		LoadState = GenericTriState.Successful;
		OnPropertyChanged("LoadState");
	}

	private void RequestClose()
	{
		RequestCloseEvent?.Invoke(this, EventArgs.Empty);
	}
}
