using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Orbitstrap.UI.Elements.Settings.Pages;
using Orbitstrap.Models.Persistable;
using static Orbitstrap.Models.Persistable.AppSettings;

namespace Orbitstrap.Integrations
{
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
        private const string GamePlayerJoinLeaveEntry = "[ExpChat/mountClientApp (Trace)] - Player ";
        private const string GameMessageLogEntry = "[ExpChat/mountClientApp (Debug)] - Incoming MessageReceived Status: ";
        private const string GameJoiningEntryPattern = @"! Joining game '([0-9a-f\-]{36})' place ([0-9]+) at ([0-9\.]+)";
        private const string GameJoiningPrivateServerPattern = @"""accessCode"":""([0-9a-f\-]{36})""";
        private const string GameJoiningUniversePattern = @"universeid:([0-9]+).*userid:([0-9]+)";
        private const string GameJoiningUDMUXPattern = @"UDMUX Address = ([0-9\.]+), Port = [0-9]+ \| RCC Server Address = ([0-9\.]+), Port = [0-9]+";
        private const string GameJoinedEntryPattern = @"serverId:\s*([0-9a-f\-]{36})";
        private const string GameMessageEntryPattern = @"\[OrbitstrapRPC\] (.*)";
        private const string GamePlayerJoinLeavePattern = @"(added|removed): (.*) (.*[0-9])";
        private const string GameMessageLogPattern = @"Success Text: (.*)";
        // NOTE: resolution changing no longer uses a locally-declared DEVMODE/P-Invoke
        // set here. It previously had its own copy of DEVMODE that was missing several
        // trailing fields (dmICMMethod, dmICMIntent, dmMediaType, dmDitherType,
        // dmReserved1/2, dmPanningWidth/Height). That made Marshal.SizeOf(typeof(DEVMODE))
        // return a size smaller than the real Windows DEVMODE structure, so
        // EnumDisplaySettings was handed an undersized buffer to write into. This could
        // silently corrupt memory and crash this class's log-reading loop the moment a
        // game was joined (exactly when the resolution change was supposed to fire),
        // which is why the in-game resolution changer looked like it "did nothing" even
        // though the app was clearly tracking the game. All resolution reads/writes now
        // go through InGameResolutionApplier, which has the correct, complete struct.

        private int _logEntriesRead = 0;
        private bool _teleportMarker = false;
        private bool _reservedTeleportMarker = false;

        public event EventHandler<string>? OnLogEntry;
        public event EventHandler? OnGameJoin;
        public event EventHandler? OnGameLeave;
        public event EventHandler? OnLogOpen;
        public event EventHandler? OnAppClose;
        public event EventHandler<ActivityData.UserLog>? OnNewPlayerRequest;
        public event EventHandler<ActivityData.UserMessage>? OnNewMessageRequest;
        public event EventHandler<Message>? OnRPCMessage;

        private static ResolutionSetting? _originalResolution;
        private static bool _resolutionApplied = false;

        private DateTime _lastRejoinAttempt = DateTime.MinValue;
        private DateTime LastRPCRequest;

        public string LogLocation = null!;
        public bool InGame = false;
        public ActivityData Data { get; private set; } = new();
        public List<ActivityData> History = new();
        public Dictionary<int, ActivityData.UserLog> PlayerLogs => Data.PlayerLogs;
        public Dictionary<int, ActivityData.UserMessage> MessageLogs => Data.MessageLogs;
        public bool IsDisposed = false;

        public ActivityWatcher(string? logFile = null)
        {
            if (!string.IsNullOrEmpty(logFile))
                LogLocation = logFile;
        }

        public async Task<string?> GetGameIconAsync()
        {
            try
            {
                if (Data.UniverseId == 0)
                    return null;

                using var client = new System.Net.Http.HttpClient();
                string url = $"https://thumbnails.roblox.com/v1/games/icons?universeIds={Data.UniverseId}&size=150x150&format=Png&isCircular=true";
                var json = await client.GetStringAsync(url);

                using JsonDocument doc = JsonDocument.Parse(json);
                return doc.RootElement
                    .GetProperty("data")[0]
                    .GetProperty("imageUrl")
                    .GetString();
            }
            catch
            {
                return null;
            }
        }

        public async Task<int> GetPlayerCount()
        {
            if (Data.PlaceId == 0)
                return 0;

            int countFromApi = 0;
            try
            {
                using var http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                string url = $"https://games.roblox.com/v1/games/{Data.PlaceId}/servers/Public?limit=100";
                var json = await http.GetStringAsync(url);
                var response = JsonSerializer.Deserialize<ServerResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (response?.Data != null)
                {
                    var currentServer = response.Data.FirstOrDefault(s => s.Id == Data.JobId);
                    countFromApi = currentServer != null
                        ? currentServer.Playing
                        : response.Data.Sum(s => s.Playing);
                }
            }
            catch { }

            int countFromLogs = 1;
            if (Data.PlayerLogs != null && Data.PlayerLogs.Count > 0)
            {
                countFromLogs = Data.PlayerLogs.Values
                    .Where(log => log != null && !string.IsNullOrWhiteSpace(log.UserId))
                    .GroupBy(log => log.UserId!.Trim())
                    .Select(g => g.OrderByDescending(l => l.Time).First())
                    .Count(log => string.Equals(log.Type, "added", StringComparison.OrdinalIgnoreCase));
            }

            return Math.Max(countFromApi, countFromLogs);
        }

        public async void Start()
        {
            const string LOG_IDENT = "ActivityWatcher::Start";

            FileInfo logFileInfo;

            if (string.IsNullOrEmpty(LogLocation))
            {
                string logDirectory = Path.Combine(Paths.LocalAppData, "Roblox\\logs");

                if (!Directory.Exists(logDirectory))
                    return;

                App.Logger.WriteLine(LOG_IDENT, "Opening Roblox log file...");

                while (true)
                {
                    logFileInfo = new DirectoryInfo(logDirectory)
                        .GetFiles()
                        .Where(x => x.Name.Contains("Player", StringComparison.OrdinalIgnoreCase)
                                 && x.CreationTime <= DateTime.Now)
                        .OrderByDescending(x => x.CreationTime)
                        .First();

                    if (logFileInfo.CreationTime.AddSeconds(15) > DateTime.Now)
                        break;

                    App.Logger.WriteLine(LOG_IDENT, $"Could not find recent enough log file, waiting... (newest is {logFileInfo.Name})");
                    await Task.Delay(750);
                }

                LogLocation = logFileInfo.FullName;
            }
            else
            {
                logFileInfo = new FileInfo(LogLocation);
            }

            OnLogOpen?.Invoke(this, EventArgs.Empty);

            var logFileStream = logFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            App.Logger.WriteLine(LOG_IDENT, $"Opened {LogLocation}");

            using var streamReader = new StreamReader(logFileStream);

            while (!IsDisposed)
            {
                string? log = await streamReader.ReadLineAsync();

                if (log is null)
                    await Task.Delay(700);
                else
                    ReadLogEntry(log);
            }
        }

        private void ReadLogEntry(string entry)
        {
            const string LOG_IDENT = "ActivityWatcher::ReadLogEntry";
            OnLogEntry?.Invoke(this, entry);
            _logEntriesRead++;

            if (_logEntriesRead <= 1000 && _logEntriesRead % 50 == 0)
                App.Logger.WriteLine(LOG_IDENT, $"Read {_logEntriesRead} log entries");
            else if (_logEntriesRead % 100 == 0)
                App.Logger.WriteLine(LOG_IDENT, $"Read {_logEntriesRead} log entries");

            if (entry.Contains(GameLeavingEntry))
            {
                App.Logger.WriteLine(LOG_IDENT, "User is back into the desktop app");
                RestoreOriginalResolution();
                OnAppClose?.Invoke(this, EventArgs.Empty);

                if (Data.PlaceId != 0 && !InGame)
                {
                    App.Logger.WriteLine(LOG_IDENT, "User appears to be leaving from a cancelled/errored join");
                    Data = new();
                }
            }

            if (!InGame && Data.PlaceId == 0)
            {
                if (entry.Contains(GameJoiningPrivateServerEntry))
                {
                    Data.ServerType = ServerType.Private;
                    var match = Regex.Match(entry, GameJoiningPrivateServerPattern);
                    if (match.Groups.Count != 2)
                        return;
                    Data.AccessCode = match.Groups[1].Value;
                }
                else if (entry.Contains(GameJoiningEntry))
                {
                    var match = Regex.Match(entry, GameJoiningEntryPattern);
                    if (match.Groups.Count != 4)
                        return;

                    InGame = false;
                    Data.PlaceId = long.Parse(match.Groups[2].Value);
                    Data.JobId = match.Groups[1].Value;
                    Data.MachineAddress = match.Groups[3].Value;

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

                    App.Logger.WriteLine(LOG_IDENT, $"Joining Game ({Data.JobId})");
                }
            }

            else if (!InGame && Data.PlaceId != 0)
            {
                if (entry.Contains(GameJoiningUniverseEntry))
                {
                    var match = Regex.Match(entry, GameJoiningUniversePattern);
                    if (match.Groups.Count != 3)
                        return;

                    Data.UniverseId = long.Parse(match.Groups[1].Value);
                    Data.UserId = long.Parse(match.Groups[2].Value);

                    if (History.Any())
                    {
                        var lastActivity = History.First();
                        if (Data.UniverseId == lastActivity.UniverseId && Data.IsTeleport)
                            Data.RootActivity = lastActivity.RootActivity ?? lastActivity;
                    }
                }
                else if (entry.Contains(GameJoiningUDMUXEntry))
                {
                    var match = Regex.Match(entry, GameJoiningUDMUXPattern);
                    if (match.Groups.Count != 3)
                        return;

                    Data.MachineAddress = match.Groups[1].Value;
                    App.Logger.WriteLine(LOG_IDENT, $"Server is UDMUX protected ({Data.MachineAddress})");
                }
                else if (entry.Contains(GameJoinedEntry))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Confirmed game join (JobId = {Data.JobId})");

                    InGame = true;
                    Data.TimeJoined = DateTime.Now;

                    ApplyInGameResolutionIfNeeded();
                    OnGameJoin?.Invoke(this, EventArgs.Empty);
                }
            }

            else if (InGame && Data.PlaceId != 0)
            {
                if (entry.Contains(GameDisconnectedEntry))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Disconnected from Game ({Data.JobId})");

                    RestoreOriginalResolution();

                    Data.TimeLeft = DateTime.Now;
                    History.Insert(0, Data);

                    InGame = false;

                    var lastData = Data;
                    Data = new();

                    OnGameLeave?.Invoke(this, EventArgs.Empty);
                }
                else if (entry.Contains(GameTeleportingEntry))
                {
                    _teleportMarker = true;
                    App.Logger.WriteLine(LOG_IDENT, $"Initiating teleport ({Data.JobId})");
                }
                else if (entry.Contains(GameJoiningReservedServerEntry))
                {
                    _teleportMarker = true;
                    _reservedTeleportMarker = true;
                }
                else if (entry.Contains(GameMessageEntry))
                {
                    var match = Regex.Match(entry, GameMessageEntryPattern);
                    if (match.Groups.Count != 2)
                        return;

                    string messagePlain = match.Groups[1].Value;

                    if ((DateTime.Now - LastRPCRequest).TotalSeconds <= 1)
                        return;

                    Message? message;
                    try
                    {
                        message = JsonSerializer.Deserialize<Message>(messagePlain);
                    }
                    catch
                    {
                        return;
                    }

                    if (message is null)
                        return;

                    if (message.Command == "SetLaunchData")
                    {
                        string? data = message.Data.Deserialize<string>();
                        if (data != null && data.Length <= 200)
                            Data.RPCLaunchData = data;
                    }

                    OnRPCMessage?.Invoke(this, message);
                    LastRPCRequest = DateTime.Now;
                }
                else if (entry.Contains(GamePlayerJoinLeaveEntry))
                {
                    var match = Regex.Match(entry, GamePlayerJoinLeavePattern);
                    if (match.Groups.Count != 4)
                        return;

                    var userLog = new ActivityData.UserLog
                    {
                        Type = match.Groups[1].Value,
                        Username = match.Groups[2].Value,
                        UserId = match.Groups[3].Value,
                        Time = DateTime.Now
                    };

                    Data.PlayerLogs[Data.PlayerLogs.Count] = userLog;
                    OnNewPlayerRequest?.Invoke(this, userLog);
                }
                else if (entry.Contains(GameMessageLogEntry))
                {
                    var match = Regex.Match(entry, GameMessageLogPattern);
                    if (match.Groups.Count != 2)
                        return;

                    var messageLog = new ActivityData.UserMessage
                    {
                        Message = match.Groups[1].Value,
                        Time = DateTime.Now
                    };

                    Data.MessageLogs[Data.MessageLogs.Count] = messageLog;
                    OnNewMessageRequest?.Invoke(this, messageLog);
                }
            }
        }

        private void RestoreOriginalResolution()
        {
            try
            {
                if (!_resolutionApplied || _originalResolution is null)
                    return;

                App.Logger.WriteLine("ActivityWatcher", "Restoring original desktop resolution");
                InGameResolutionApplier.Apply(_originalResolution);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("ActivityWatcher", $"Failed to restore original resolution: {ex.Message}");
            }
            finally
            {
                _resolutionApplied = false;
                _originalResolution = null;
            }
        }

        /// <summary>
        /// Finds a matching resolution rule for the game that was just joined, preferring
        /// the "Advanced" per-game rule list (supports many games at once) and falling
        /// back to the legacy single Place ID setting for backwards compatibility with
        /// existing configs. Logs the reason for every early return so the log file can
        /// be used to diagnose configuration problems.
        /// </summary>
        private static ResolutionSetting? FindMatchingResolution(AppSettings settings, ActivityData data)
        {
            const string LOG_IDENT = "ActivityWatcher::FindMatchingResolution";

            // ---- Advanced per-game rules ----
            if (settings.GameResolutionRules is { Count: > 0 })
            {
                App.Logger.WriteLine(LOG_IDENT,
                    $"Checking {settings.GameResolutionRules.Count} advanced rule(s) (Place={data.PlaceId}, Universe={data.UniverseId})");

                foreach (var rule in settings.GameResolutionRules)
                {
                    if (rule.Resolution is null)
                        continue;

                    bool isMatch = rule.MatchUniverseId
                        ? rule.UniverseId.HasValue && rule.UniverseId.Value != 0 && rule.UniverseId.Value == data.UniverseId
                        : long.TryParse(rule.PlaceId, out long rulePlaceId) && rulePlaceId != 0 && rulePlaceId == data.PlaceId;

                    if (isMatch)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Advanced rule '{rule.Name}' matched");
                        return rule.Resolution;
                    }
                }

                App.Logger.WriteLine(LOG_IDENT,
                    $"No advanced rule matched for Place={data.PlaceId} / Universe={data.UniverseId}; falling back to single-game setting");
            }
            else
            {
                App.Logger.WriteLine(LOG_IDENT, "No advanced rules configured; checking single-game setting");
            }

            // ---- Legacy single-game setting (matches by Place ID only) ----
            // NOTE: The top-level AppSettings.MatchUniverseId / TargetUniverseId fields are
            // NOT used here. Those fields have no UI binding in the Resolution Settings panel
            // and may be set to non-default values by other features, which would make this
            // check silently fail for the resolution feature. Universe ID matching for the
            // resolution feature is available through the Advanced per-game rules above.
            if (!settings.UsePlaceId)
            {
                App.Logger.WriteLine(LOG_IDENT,
                    "In-game resolution is disabled — tick 'Apply Resolution PlaceID' in Resolution Settings to enable it");
                return null;
            }

            if (settings.InGameResolution is null)
            {
                App.Logger.WriteLine(LOG_IDENT,
                    "No in-game resolution configured — pick a resolution from the in-game dropdown in Resolution Settings");
                return null;
            }

            if (!long.TryParse(settings.PlaceId, out long targetPlaceId) || targetPlaceId == 0)
            {
                App.Logger.WriteLine(LOG_IDENT,
                    $"Configured Place ID '{settings.PlaceId}' is not a valid number — enter the numeric Place ID in Resolution Settings");
                return null;
            }

            if (data.PlaceId != targetPlaceId)
            {
                App.Logger.WriteLine(LOG_IDENT,
                    $"Place ID mismatch — configured: {targetPlaceId}, joined: {data.PlaceId}");
                return null;
            }

            return settings.InGameResolution;
        }

        private void ApplyInGameResolutionIfNeeded()
        {
            try
            {
                if (_resolutionApplied)
                    return;

                var settings = App.Settings.Prop;

                App.Logger.WriteLine("ActivityWatcher",
                    $"ApplyInGameResolutionIfNeeded: Place={Data.PlaceId}, Universe={Data.UniverseId}");

                var match = FindMatchingResolution(settings, Data);

                if (match is null)
                    return;

                App.Logger.WriteLine("ActivityWatcher",
                    $"Applying in-game resolution (Universe={Data.UniverseId}, Place={Data.PlaceId})");

                _originalResolution ??= InGameResolutionApplier.GetCurrent();
                _resolutionApplied = true;

                InGameResolutionApplier.Apply(match);

                // Roblox re-asserts its own preferred display mode a few seconds after
                // the level finishes loading (well after this event fires), which can
                // silently undo the change above. Re-apply a couple more times over the
                // following seconds so our setting "wins" the race instead of only being
                // applied once right at connection time.
                _ = ReapplyResolutionAfterDelaysAsync(match);
            }
            catch (Exception ex)
            {
                // A failure here must never propagate up into ReadLogEntry/Start's log
                // loop - doing so previously killed activity tracking for the rest of the
                // session, which is why the resolution changer could appear to silently
                // stop working entirely after one bad attempt.
                App.Logger.WriteLine("ActivityWatcher", $"Failed to apply in-game resolution: {ex.Message}");
            }
        }

        private async Task ReapplyResolutionAfterDelaysAsync(ResolutionSetting target)
        {
            int[] delaysMs = { 2000, 5000, 10000 };

            foreach (int delay in delaysMs)
            {
                await Task.Delay(delay);

                // Bail out if we've since left the game / restored the resolution, or the
                // watcher was disposed - don't fight a session that has already ended.
                if (!_resolutionApplied || IsDisposed)
                    return;

                var current = InGameResolutionApplier.GetCurrent();
                if (current is not null &&
                    current.Width == target.Width &&
                    current.Height == target.Height &&
                    current.RefreshRate == target.RefreshRate)
                {
                    continue; // still applied correctly, nothing to do
                }

                App.Logger.WriteLine("ActivityWatcher",
                    "Detected the in-game resolution was reverted (likely by Roblox itself) - re-applying");
                InGameResolutionApplier.Apply(target);
            }
        }

        public void Dispose()
        {
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}