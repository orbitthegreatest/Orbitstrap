using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Orbitstrap.AppData;
using Orbitstrap.Enums;
using Orbitstrap.Exceptions;
using Orbitstrap.Extensions;
using Orbitstrap.Models.APIs;
using Orbitstrap.Models.APIs.RoValra;
using Orbitstrap.Resources;
using Orbitstrap.UI;
using Orbitstrap.Utility;

namespace Orbitstrap.Models.Entities;

public class ActivityData
{
	private long _universeId;

	public ActivityData? RootActivity;

	private SemaphoreSlim serverQuerySemaphore = new SemaphoreSlim(1, 1);

	private SemaphoreSlim serverTimeSemaphore = new SemaphoreSlim(1, 1);

	public long UniverseId
	{
		get
		{
			return _universeId;
		}
		set
		{
			_universeId = value;
			Orbitstrap.Models.Entities.UniverseDetails.LoadFromCache(value);
		}
	}

	public long PlaceId { get; set; }

	public string JobId { get; set; } = string.Empty;

	public string AccessCode { get; set; } = string.Empty;

	public long UserId { get; set; }

	public string MachineAddress { get; set; } = string.Empty;

	public bool MachineAddressValid
	{
		get
		{
			if (!string.IsNullOrEmpty(MachineAddress))
			{
				return !MachineAddress.StartsWith("10.");
			}
			return false;
		}
	}

	public bool IsTeleport { get; set; }

	public ServerType ServerType { get; set; }

	public DateTime TimeJoined { get; set; }

	public DateTime? TimeLeft { get; set; }

	public string RPCLaunchData { get; set; } = string.Empty;

	public UniverseDetails? UniverseDetails { get; set; }

	public string GameHistoryDescription
	{
		get
		{
			string text = string.Format("{0} • {1} {2} {3}", UniverseDetails?.Data.Creator.Name, TimeJoined.ToString("t"), Locale.CurrentCulture.Name.StartsWith("ja") ? '~' : '-', TimeLeft?.ToString("t"));
			if (ServerType != ServerType.Public)
			{
				text = text + " • " + ServerType.ToTranslatedString();
			}
			return text;
		}
	}

	public ICommand RejoinServerCommand => new RelayCommand(RejoinServer);

	public string GetInviteDeeplink(bool launchData = true)
	{
		string text = $"{App.RemoteData.Prop.DeeplinkUrl}?placeId={PlaceId}";
		text = ((ServerType != ServerType.Private) ? (text + "&gameInstanceId=" + JobId) : (text + "&accessCode=" + AccessCode));
		if (launchData && !string.IsNullOrEmpty(RPCLaunchData))
		{
			text = text + "&launchData=" + HttpUtility.UrlEncode(RPCLaunchData);
		}
		return text;
	}

	public async Task<DateTime?> QueryServerTime()
	{
		if (string.IsNullOrEmpty(JobId))
		{
			throw new InvalidOperationException("JobId is null");
		}
		if (PlaceId == 0L)
		{
			throw new InvalidOperationException("PlaceId is null");
		}
		await serverTimeSemaphore.WaitAsync();
		if (GlobalCache.ServerTime.TryGetValue(JobId, out var value))
		{
			serverTimeSemaphore.Release();
			return value;
		}
		DateTime? firstSeen = DateTime.UtcNow;
		try
		{
			RoValraTimeResponse roValraTimeResponse = await Http.GetJson<RoValraTimeResponse>($"https://apis.rovalra.com/v1/servers/details?place_id={PlaceId}&server_ids={JobId}");
			HttpContent content = new StringContent(JsonSerializer.Serialize(new RoValraProcessServerBody
			{
				PlaceId = PlaceId,
				ServerIds = new List<string> { JobId }
			}), Encoding.UTF8, "application/json");
			App.HttpClient.PostAsync("https://apis.rovalra.com/process_servers", content);
			RoValraServer roValraServer = null;
			if (roValraTimeResponse?.Servers != null && roValraTimeResponse.Servers.Count > 0)
			{
				roValraServer = roValraTimeResponse.Servers[0];
			}
			if (roValraServer != null && roValraServer.FirstSeen.HasValue)
			{
				firstSeen = roValraServer.FirstSeen;
			}
			GlobalCache.ServerTime[JobId] = firstSeen;
			serverTimeSemaphore.Release();
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("ActivityData::QueryServerTime", $"Failed to get server time for {PlaceId}/{JobId}");
			App.Logger.WriteException("ActivityData::QueryServerTime", ex);
			GlobalCache.ServerTime[JobId] = firstSeen;
			serverTimeSemaphore.Release();
			Frontend.ShowConnectivityDialog(string.Format(Strings.Dialog_Connectivity_UnableToConnect, "rovalra.com"), Strings.ActivityWatcher_LocationQueryFailed, MessageBoxImage.Exclamation, ex);
		}
		return firstSeen;
	}

	public async Task<string?> QueryServerLocation()
	{
		if (!MachineAddressValid)
		{
			throw new InvalidOperationException("Machine address is invalid (" + MachineAddress + ")");
		}
		await serverQuerySemaphore.WaitAsync();
		if (GlobalCache.ServerLocation.TryGetValue(MachineAddress, out string location))
		{
			serverQuerySemaphore.Release();
			return location;
		}
		try
		{
			IPInfoResponse iPInfoResponse = await Http.GetJson<IPInfoResponse>("https://ipinfo.io/" + MachineAddress + "/json");
			if (string.IsNullOrEmpty(iPInfoResponse.City))
			{
				throw new InvalidHTTPResponseException("Reported city was blank");
			}
			location = ((!(iPInfoResponse.City == iPInfoResponse.Region)) ? $"{iPInfoResponse.City}, {iPInfoResponse.Region}, {iPInfoResponse.Country}" : (iPInfoResponse.Region + ", " + iPInfoResponse.Country));
			GlobalCache.ServerLocation[MachineAddress] = location;
			serverQuerySemaphore.Release();
		}
		catch (Exception ex)
		{
			App.Logger.WriteLine("ActivityData::QueryServerLocation", "Failed to get server location for " + MachineAddress);
			App.Logger.WriteException("ActivityData::QueryServerLocation", ex);
			GlobalCache.ServerLocation[MachineAddress] = location;
			serverQuerySemaphore.Release();
			Frontend.ShowConnectivityDialog(string.Format(Strings.Dialog_Connectivity_UnableToConnect, "ipinfo.io"), Strings.ActivityWatcher_LocationQueryFailed, MessageBoxImage.Exclamation, ex);
		}
		return location;
	}

	public override string ToString()
	{
		return $"{PlaceId}/{JobId}";
	}

	private void RejoinServer()
	{
		Process.Start(new RobloxPlayerData().ExecutablePath, GetInviteDeeplink(launchData: false));
	}
}
