using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Orbitstrap.Enums;
using Orbitstrap.Models.Entities;
using Orbitstrap.Models.OrbitstrapRPC;

namespace Orbitstrap.Integrations;

public class ActivityWatcher : IDisposable
{
	private const string GameMessageEntry = "[FLog::Output] [OrbitstrapRPC]";

	private const string GameJoiningEntry = "[FLog::Output] ! Joining game";

	private const string GameTeleportingEntry = "[FLog::GameJoinUtil] GameJoinUtil::initiateTeleportToPlace";

	private const string GameJoiningPrivateServerEntry = "[FLog::GameJoinUtil] GameJoinUtil::joinGamePostPrivateServer";

	private const string GameJoiningReservedServerEntry = "[FLog::GameJoinUtil] GameJoinUtil::initiateTeleportToReservedServer";

	private const string GameJoiningUniverseEntry = "[FLog::GameJoinLoadTime] Report game_join_loadtime:";

	private const string GameJoiningUDMUXEntry = "[FLog::Network] UDMUX Address = ";

	private const string GameJoinedEntry = "[FLog::Network] serverId:";

	private const string GameDisconnectedEntry = "[FLog::Network] Time to disconnect replication data:";

	private const string GameLeavingEntry = "[FLog::SingleSurfaceApp] leaveUGCGameInternal";

	private const string GameJoiningEntryPattern = "! Joining game '([0-9a-f\\-]{36})' place ([0-9]+) at ([0-9\\.]+)";

	private const string GameJoiningPrivateServerPattern = "\"accessCode\":\"([0-9a-f\\-]{36})\"";

	private const string GameJoiningUniversePattern = "universeid:([0-9]+).*userid:([0-9]+)";

	private const string GameJoiningUDMUXPattern = "UDMUX Address = ([0-9\\.]+), Port = [0-9]+ \\| RCC Server Address = ([0-9\\.]+), Port = [0-9]+";

	private const string GameJoinedEntryPattern = "serverId: ([0-9\\.]+)\\|[0-9]+";

	private const string GameMessageEntryPattern = "\\[OrbitstrapRPC\\] (.*)";

	private int _logEntriesRead;

	private bool _teleportMarker;

	private bool _reservedTeleportMarker;

	private DateTime LastRPCRequest;

	public string LogLocation;

	public bool InGame;

	public List<ActivityData> History = new List<ActivityData>();

	public bool IsDisposed;

	public ActivityData Data { get; private set; } = new ActivityData();

	public event EventHandler<string>? OnLogEntry;

	public event EventHandler? OnGameJoin;

	public event EventHandler? OnGameLeave;

	public event EventHandler? OnLogOpen;

	public event EventHandler? OnAppClose;

	public event EventHandler<Message>? OnRPCMessage;

	public ActivityWatcher(string? logFile = null)
	{
		if (!string.IsNullOrEmpty(logFile))
		{
			LogLocation = logFile;
		}
	}

	public async void Start()
	{
		FileInfo fileInfo;
		if (string.IsNullOrEmpty(LogLocation))
		{
			string logDirectory = Path.Combine(Paths.LocalAppData, "Roblox\\logs");
			if (!Directory.Exists(logDirectory))
			{
				return;
			}
			App.Logger.WriteLine("ActivityWatcher::Start", "Opening Roblox log file...");
			while (true)
			{
				fileInfo = (from x in new DirectoryInfo(logDirectory).GetFiles()
					where x.Name.Contains("Player", StringComparison.OrdinalIgnoreCase) && x.CreationTime <= DateTime.Now
					orderby x.CreationTime descending
					select x).FirstOrDefault();
				if (fileInfo.CreationTime.AddSeconds(15.0) > DateTime.Now)
				{
					break;
				}
				App.Logger.WriteLine("ActivityWatcher::Start", "Could not find recent enough log file, waiting... (newest is " + fileInfo.Name + ")");
				await Task.Delay(1000);
			}
			LogLocation = fileInfo.FullName;
		}
		else
		{
			fileInfo = new FileInfo(LogLocation);
		}
		this.OnLogOpen?.Invoke(this, EventArgs.Empty);
		FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		App.Logger.WriteLine("ActivityWatcher::Start", "Opened " + LogLocation);
		using StreamReader streamReader = new StreamReader(stream);
		while (!IsDisposed)
		{
			string text = await streamReader.ReadLineAsync();
			if (text == null)
			{
				await Task.Delay(1000);
			}
			else
			{
				ReadLogEntry(text);
			}
		}
	}

	private void ReadLogEntry(string entry)
	{
		this.OnLogEntry?.Invoke(this, entry);
		_logEntriesRead++;
		if (_logEntriesRead <= 1000 && _logEntriesRead % 50 == 0)
		{
			App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", $"Read {_logEntriesRead} log entries");
		}
		else if (_logEntriesRead % 100 == 0)
		{
			App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", $"Read {_logEntriesRead} log entries");
		}
		if (entry.Contains("[FLog::SingleSurfaceApp] leaveUGCGameInternal"))
		{
			App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "User is back into the desktop app");
			this.OnAppClose?.Invoke(this, EventArgs.Empty);
			if (Data.PlaceId != 0L && !InGame)
			{
				App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "User appears to be leaving from a cancelled/errored join");
				Data = new ActivityData();
			}
		}
		if (!InGame && Data.PlaceId == 0L)
		{
			if (entry.Contains("[FLog::GameJoinUtil] GameJoinUtil::joinGamePostPrivateServer"))
			{
				Data.ServerType = ServerType.Private;
				Match match = Regex.Match(entry, "\"accessCode\":\"([0-9a-f\\-]{36})\"");
				if (match.Groups.Count != 2)
				{
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "Failed to assert format for game join private server entry");
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", entry);
				}
				else
				{
					Data.AccessCode = match.Groups[1].Value;
				}
			}
			else
			{
				if (!entry.Contains("[FLog::Output] ! Joining game"))
				{
					return;
				}
				Match match2 = Regex.Match(entry, "! Joining game '([0-9a-f\\-]{36})' place ([0-9]+) at ([0-9\\.]+)");
				if (match2.Groups.Count != 4)
				{
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "Failed to assert format for game join entry");
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", entry);
					return;
				}
				InGame = false;
				Data.PlaceId = long.Parse(match2.Groups[2].Value);
				Data.JobId = match2.Groups[1].Value;
				Data.MachineAddress = match2.Groups[3].Value;
				if (App.Settings.Prop.ShowServerDetails && Data.MachineAddressValid)
				{
					Data.QueryServerLocation();
				}
				if (App.Settings.Prop.ShowServerUptime && Data.JobId != null)
				{
					Data.QueryServerTime();
				}
				if (_teleportMarker)
				{
					Data.IsTeleport = true;
					_teleportMarker = false;
				}
				if (_reservedTeleportMarker)
				{
					Data.ServerType = ServerType.Reserved;
					_reservedTeleportMarker = false;
				}
				App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", $"Joining Game ({Data})");
			}
		}
		else if (!InGame && Data.PlaceId != 0L)
		{
			if (entry.Contains("[FLog::GameJoinLoadTime] Report game_join_loadtime:"))
			{
				Match match3 = Regex.Match(entry, "universeid:([0-9]+).*userid:([0-9]+)");
				if (match3.Groups.Count != 3)
				{
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "Failed to assert format for game join universe entry");
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", entry);
					return;
				}
				Data.UniverseId = long.Parse(match3.Groups[1].Value);
				Data.UserId = long.Parse(match3.Groups[2].Value);
				if (History.Any())
				{
					ActivityData activityData = History.FirstOrDefault();
					if (Data.UniverseId == activityData.UniverseId && Data.IsTeleport)
					{
						Data.RootActivity = activityData.RootActivity ?? activityData;
					}
				}
			}
			else if (entry.Contains("[FLog::Network] UDMUX Address = "))
			{
				Match match4 = Regex.Match(entry, "UDMUX Address = ([0-9\\.]+), Port = [0-9]+ \\| RCC Server Address = ([0-9\\.]+), Port = [0-9]+");
				if (match4.Groups.Count != 3 || match4.Groups[2].Value != Data.MachineAddress)
				{
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "Failed to assert format for game join UDMUX entry");
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", entry);
					return;
				}
				Data.MachineAddress = match4.Groups[1].Value;
				if (App.Settings.Prop.ShowServerDetails)
				{
					Data.QueryServerLocation();
				}
				App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", $"Server is UDMUX protected ({Data})");
			}
			else if (entry.Contains("[FLog::Network] serverId:"))
			{
				Match match5 = Regex.Match(entry, "serverId: ([0-9\\.]+)\\|[0-9]+");
				if (match5.Groups.Count != 2 || match5.Groups[1].Value != Data.MachineAddress)
				{
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "Failed to assert format for game joined entry");
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", entry);
					return;
				}
				App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", $"Joined Game ({Data})");
				InGame = true;
				Data.TimeJoined = DateTime.Now;
				this.OnGameJoin?.Invoke(this, EventArgs.Empty);
			}
		}
		else
		{
			if (!InGame || Data.PlaceId == 0L)
			{
				return;
			}
			if (entry.Contains("[FLog::Network] Time to disconnect replication data:"))
			{
				App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", $"Disconnected from Game ({Data})");
				Data.TimeLeft = DateTime.Now;
				History.Insert(0, Data);
				InGame = false;
				Data = new ActivityData();
				this.OnGameLeave?.Invoke(this, EventArgs.Empty);
			}
			else if (entry.Contains("[FLog::GameJoinUtil] GameJoinUtil::initiateTeleportToPlace"))
			{
				App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", $"Initiating teleport to server ({Data})");
				_teleportMarker = true;
			}
			else if (entry.Contains("[FLog::GameJoinUtil] GameJoinUtil::initiateTeleportToReservedServer"))
			{
				_teleportMarker = true;
				_reservedTeleportMarker = true;
			}
			else
			{
				if (!entry.Contains("[FLog::Output] [OrbitstrapRPC]"))
				{
					return;
				}
				Match match6 = Regex.Match(entry, "\\[OrbitstrapRPC\\] (.*)");
				if (match6.Groups.Count != 2)
				{
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "Failed to assert format for RPC message entry");
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", entry);
					return;
				}
				string value = match6.Groups[1].Value;
				App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "Received message: '" + value + "'");
				if ((DateTime.Now - LastRPCRequest).TotalSeconds <= 1.0)
				{
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "Dropping message as ratelimit has been hit");
					return;
				}
				Message message;
				try
				{
					message = JsonSerializer.Deserialize<Message>(value);
				}
				catch (Exception)
				{
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "Failed to parse message! (JSON deserialization threw an exception)");
					return;
				}
				if (message == null)
				{
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "Failed to parse message! (JSON deserialization returned null)");
					return;
				}
				if (string.IsNullOrEmpty(message.Command))
				{
					App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "Failed to parse message! (Command is empty)");
					return;
				}
				if (message.Command == "SetLaunchData")
				{
					string text;
					try
					{
						text = message.Data.Deserialize<string>();
					}
					catch (Exception)
					{
						App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "Failed to parse message! (JSON deserialization threw an exception)");
						return;
					}
					if (text == null)
					{
						App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "Failed to parse message! (JSON deserialization returned null)");
						return;
					}
					if (text.Length > 200)
					{
						App.Logger.WriteLine("ActivityWatcher::ReadLogEntry", "Data cannot be longer than 200 characters");
						return;
					}
					Data.RPCLaunchData = text;
				}
				this.OnRPCMessage?.Invoke(this, message);
				LastRPCRequest = DateTime.Now;
			}
		}
	}

	public void Dispose()
	{
		IsDisposed = true;
		GC.SuppressFinalize(this);
	}
}
