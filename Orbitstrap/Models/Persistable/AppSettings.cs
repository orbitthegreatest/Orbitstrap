using System.Collections.ObjectModel;
using System.IO;
using Orbitstrap.Enums;

namespace Orbitstrap.Models.Persistable
{
    /// <summary>
    /// Represents configuration settings for Orbitstrap.
    /// </summary>
    public class AppSettings
    {
        // General Configuration
        public BootstrapperStyle BootstrapperStyle { get; set; } = BootstrapperStyle.FluentAeroDialog;
        public BootstrapperIcon BootstrapperIcon { get; set; } = BootstrapperIcon.IconOrbitstrap;
        public CleanerOptions CleanerOptions { get; set; } = CleanerOptions.Never;
        public List<string> CleanerDirectories { get; set; } = new List<string>();
        public string BootstrapperTitle { get; set; } = App.ProjectName;
        public string BootstrapperIconCustomLocation { get; set; } = "";
        public Theme Theme2 { get; set; } = Theme.Orbitstrap;
        public string? SelectedCustomTheme { get; set; } = null;
        public string SelectedCpuPriority { get; set; } = "Automatic";
        public int MaxCpuCores { get; set; } = Environment.ProcessorCount;
        public int TotalLogicalCores { get; set; } = Environment.ProcessorCount;
        public int TotalPhysicalCores { get; set; } = Environment.ProcessorCount;
        public bool IsChannelEnabled { get; set; } = false;
        public bool UpdateRoblox { get; set; } = true;
        public bool ForceRobloxReinstallation { get; set; } = false;

        public int CleanRobloxNumber = 0;
        public bool DisableCrash { get; set; } = false;
        public int CpuCoreLimit { get; set; } = Environment.ProcessorCount;
        public string ShiftlockCursorSelectedPath { get; set; } = "";
        public string UseCustomIcon { get; set; } = "";
        public string CustomGameName { get; set; } = "";
        public string PriorityLimit { get; set; } = "Normal";
        public string SelectedStatus { get; set; } = "Gray";
        public string ArrowCursorSelectedPath { get; set; } = "";
        public string ArrowFarCursorSelectedPath { get; set; } = "";
        public string IBeamCursorSelectedPath { get; set; } = "";

        public bool DisableSplashScreen { get; set; } = true;
        public bool EnableAnalytics { get; set; } = true;
        public bool ShouldExportConfig { get; set; } = true;
        public bool ShouldExportLogs { get; set; } = true;
        public bool UseFastFlagManager { get; set; } = true;
        public bool WPFSoftwareRender { get; set; } = false;
        public bool ConfirmLaunches { get; set; } = true;

        public bool SmooothBARRyesirikikthxlucipook { get; set; } = false; // wanna keep this on false so people may not be annoyed by it being on
        public bool HasLaunchedGame { get; set; } = false;
        public bool NotificationWindowShow { get; set; } = true;
        public bool BackgroundWindow { get; set; } = true;
        public bool UsePlaceId { get; set; } = false;
        public bool ClearFont { get; set; } = false;

        public bool Fleasion { get; set; } = false;
        public string PlaceId { get; set; } = "";
        public bool OptimizeRoblox { get; set; } = false;
        public bool BackgroundUpdatesEnabled { get; set; } = true;
        public bool VoidNotify { get; set; } = true;
        public bool ServerPingCounter { get; set; } = false;
        public bool ShowServerDetailsUI { get; set; } = false;
        public bool EnableCustomStatusDisplay { get; set; } = true;
        public bool RenameClientToEuroTrucks2 { get; set; } = false;
        public bool SnowWOWSOCOOLWpfSnowbtw { get; set; } = false;

        public bool MotionBlurOverlay { get; set; } = false;

        /// <summary>Enables the Dark Texture mod.</summary>
        public bool UseDarkTextureMod { get; set; } = false;

        /// <summary>Enables the built-in Black Cursor mod (replaces the cursor set with the bundled black cursor).</summary>
        public bool UseBlackCursorMod { get; set; } = false;

        /// <summary>Replaces the right-leg mesh with the Korblox skeleton leg mesh at next Roblox launch.</summary>
        public bool UseKorbloxRightLeg { get; set; } = false;

        /// <summary>Empties the Roblox heads folder at next launch, making the avatar appear headless.</summary>
        public bool UseHeadlessMod { get; set; } = false;

        /// <summary>Selected emote wheel mod id from the Orbitstrap-things manifest ("None" = disabled).</summary>
        public string SelectedEmoteWheel { get; set; } = "None";
        public string SelectedRegion { get; set; } = "All";
        public bool AllowCookieAccess { get; set; } = false;

        public string ClientPath { get; set; } = Path.Combine(Paths.Base, "Roblox", "Player");

        public string Locale { get; set; } = "nil";
        public string BufferSizeKbte { get; set; } = "1024";
        public string BufferSizeKbtes { get; set; } = "2048";
        public string SkyboxName { get; set; } = "default"; // manifest id from Orbitstrap-things/skyboxes/manifest.json
        public string FontName { get; set; } = "Default";
        public string LastServerSave { get; set; } = "112757576021097";
        public bool SkyBoxDataSending { get; set; } = false;

        public bool FFlagRPCDisplayer { get; set; } = true;

        public bool FPSCounter { get; set; } = false;

        public bool CurrentTimeDisplay { get; set; } = false;
        public bool ExclusiveFullscreen { get; set; } = false;
        public bool Crosshair { get; set; } = false;
        public bool LockDefault { get; set; } = false;
        public bool GameWIP { get; set; } = false;
        public bool ForceRobloxLanguage { get; set; } = true;

        public bool IngameChatDiscord { get; set; } = false;

        // Analytics & Tracking
        public bool DarkTextures { get; set; } = false;
        public bool EnableActivityTracking { get; set; } = true;
        public bool OverClockCPU { get; set; } = false;
        public bool exitondissy { get; set; } = false;
        public bool ServerUptimeBetterBLOXcuzitsbetterXD { get; set; } = true;

        public string DownloadingStringFormat { get; set; } = Strings.Bootstrapper_Status_Downloading + " {0} - {1}MB / {2}MB";
        public bool ConnectCloset { get; set; } = false;

        public bool Fullbright { get; set; } = false;

        public bool GameIconChecked { get; set; } = true;
        public bool ServerLocationGame { get; set; } = false;
        public bool GameNameChecked { get; set; } = true;
        public bool GameCreatorChecked { get; set; } = true;
        public bool GameStatusChecked { get; set; } = true;

        // Rich Presence (Discord Integration)
        public bool UseDiscordRichPresence { get; set; } = true;
        public bool HideRPCButtons { get; set; } = true;
        public bool ShowAccountOnRichPresence { get; set; } = true;
        public bool MultiAccount { get; set; } = false;
        public bool ShowServerDetails { get; set; } = true;

        public bool OverlaysEnabled { get; set; } = false;

        public double Brightness { get; set; } = 50;

        // Mod Settings
        public string CustomFontLocation { get; set; } = string.Empty;
        public CursorType CursorType { get; set; } = CursorType.Default;

        // Custom Integrations
        public ObservableCollection<CustomIntegration> CustomIntegrations { get; set; } = new();

        // Mod Preset Configuration
        public bool UseDisableAppPatch { get; set; } = false;
        public bool AutoRejoin { get; set; } = false;
        public bool MultiInstanceLaunching { get; set; } = false;
        public bool Error773Fix { get; set; } = false;

        // Roblox Deployment Settings
        public string Channel { get; set; } = RobloxInterfaces.Deployment.DefaultChannel;
        public string ChannelHash { get; set; } = "";

        public string LaunchGameID { get; set; } = "";
        public bool IsGameEnabled { get; set; } = false;
        public bool MatchUniverseId { get; set; } = false;
        public long? TargetUniverseId { get; set; }
        public bool IsBetterServersEnabled { get; set; } = false;
        public bool OverClockGPU { get; set; } = false;
        public bool GRADmentFR { get; set; } = false;
        public bool VoidRPC { get; set; } = true;


        public ResolutionSetting? InGameResolution { get; set; }

        /// <summary>
        /// Advanced multi-game resolution rules: lets the user configure a different
        /// in-game resolution per PlaceId/UniverseId, instead of only the single
        /// PlaceId/InGameResolution pair above. Rules are checked in order and the first
        /// match wins; the legacy single-game setting above still works as a fallback if
        /// no rule matches.
        /// </summary>
        public ObservableCollection<GameResolutionRule> GameResolutionRules { get; set; } = new();

        // SwiftTunnel VPN Integration
        public bool SwiftTunnelEnabled { get; set; } = false;
        public bool SwiftTunnelAutoConnect { get; set; } = false;
        public bool SwiftTunnelRememberLogin { get; set; } = false;
        public bool SwiftTunnelSplitTunnel { get; set; } = false;
        public string SwiftTunnelRegion { get; set; } = "auto";



        // Performance (from fflagsfix2)
        /// <summary>Memory cleaner interval in seconds. 0 = disabled.</summary>
        public int MemoryCleanerIntervalSeconds { get; set; } = 0;
        /// <summary>Periodically trims Roblox working set to reduce RAM usage.</summary>
        public bool TrimRobloxMemory { get; set; } = false;
        /// <summary>Enable local bundled skyboxes (in addition to GitHub packs).</summary>
        public bool SkyboxEnabled { get; set; } = false;

        /// <summary>
        /// When true, Orbitstrap re-locks GlobalBasicSettings_13.xml as read-only after each
        /// save, preventing Roblox from silently overwriting the user's settings at next launch.
        /// Set to false if you want Roblox's own in-game settings menu to be able to write back
        /// to the file (at the cost of Roblox potentially reverting your customisations).
        /// Default: true (recommended).
        /// </summary>
        public bool LockGlobalSettingsReadOnly { get; set; } = true;

        public class ResolutionSetting
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int RefreshRate { get; set; }
        }

        /// <summary>A single "when I join this game, use this resolution" rule.</summary>
        public class GameResolutionRule
        {
            /// <summary>Friendly display name shown in the Advanced list (optional).</summary>
            public string Name { get; set; } = "";
            public bool MatchUniverseId { get; set; } = false;
            public string PlaceId { get; set; } = "";
            public long? UniverseId { get; set; }
            public ResolutionSetting Resolution { get; set; } = new();

            [System.Text.Json.Serialization.JsonIgnore]
            public string ResolutionDisplay => $"{Resolution.Width}x{Resolution.Height} @ {Resolution.RefreshRate}Hz";
        }
    }
}