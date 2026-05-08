using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Orbitstrap.Enums;
using Orbitstrap.Resources;

namespace Orbitstrap.Models.Persistable;

public class Settings
{
	public BootstrapperStyle BootstrapperStyle { get; set; } = BootstrapperStyle.FluentAeroDialog;

	public BootstrapperIcon BootstrapperIcon { get; set; }

	public string BootstrapperTitle { get; set; } = "Orbitstrap";

	public string BootstrapperIconCustomLocation { get; set; } = "";

	public Theme Theme { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public bool DeveloperMode { get; set; }

	public bool ForceLocalData { get; set; }

	public bool CheckForUpdates { get; set; } = true;

	public bool MultiInstanceLaunching { get; set; }

	public bool ConfirmLaunches { get; set; } = true;

	public string Locale { get; set; } = "nil";

	public bool ForceRobloxLanguage { get; set; }

	public bool UseFastFlagManager { get; set; } = true;

	public bool ModInjectorEnabled { get; set; }

	public bool WPFSoftwareRender { get; set; }

	public bool EnableAnalytics { get; set; }

	public bool UpdateRoblox { get; set; } = true;

	public string Channel { get; set; } = "production";

	public ChannelChangeMode ChannelChangeMode { get; set; }

	public string ChannelHash { get; set; } = "";

	public string DownloadingStringFormat { get; set; } = Strings.Bootstrapper_Status_Downloading + " {0} - {1}MB / {2}MB";

	public string? SelectedCustomTheme { get; set; }

	public bool BackgroundUpdatesEnabled { get; set; }

	public bool DebugDisableVersionPackageCleanup { get; set; }

	public WebEnvironment WebEnvironment { get; set; }

	public CleanerOptions CleanerOptions { get; set; }

	public List<string> CleanerDirectories { get; set; } = new List<string>();

	public bool EnableActivityTracking { get; set; } = true;

	public bool UseDiscordRichPresence { get; set; } = true;

	public bool HideRPCButtons { get; set; } = true;

	public bool ShowAccountOnRichPresence { get; set; }

	public bool ShowServerDetails { get; set; }

	public bool ShowServerUptime { get; set; }

	public ObservableCollection<CustomIntegration> CustomIntegrations { get; set; } = new ObservableCollection<CustomIntegration>();

	public bool UseDisableAppPatch { get; set; }

	// === Orbitstrap Extensions (from Voidstrap) ===
	public bool FleasionEnabled { get; set; }

	public bool AniWatchEnabled { get; set; }

	// === Orbitstrap Custom Skybox (from Voidstrap) ===
	public string SkyboxName { get; set; } = "Default";

	public bool SkyboxEnabled { get; set; }

	// === Orbitstrap Performance Features ===
	/// <summary>Number of CPU cores to assign to Roblox via affinity mask. 0 = all cores (default).</summary>
	public int CpuCores { get; set; } = 0;

	/// <summary>Memory cleaner interval in seconds. 0 = disabled.</summary>
	public int MemoryCleanerIntervalSeconds { get; set; } = 0;

	/// <summary>Periodically trims Roblox working set to reduce RAM usage.</summary>
	public bool TrimRobloxMemory { get; set; } = false;

	/// <summary>Apply CPU/GPU/memory optimisations when Roblox launches.</summary>
	public bool OptimizeRoblox { get; set; } = false;
}
