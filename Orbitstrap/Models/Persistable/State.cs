using System.Collections.Generic;

namespace Orbitstrap.Models.Persistable;

public class State
{
	public bool TestModeWarningShown { get; set; }

	public bool IgnoreOutdatedChannel { get; set; }

	public bool WatcherRunning { get; set; }

	public bool PromptWebView2Install { get; set; } = true;

	public string? LastPage { get; set; }

	public bool ForceReinstall { get; set; }

	public WindowState SettingsWindow { get; set; } = new WindowState();

	public AppState? Player { private get; set; }

	public AppState? Studio { private get; set; }

	public List<string>? ModManifest { private get; set; }

	public AppState? GetDeprecatedPlayer()
	{
		return Player;
	}

	public AppState? GetDeprecatedStudio()
	{
		return Studio;
	}

	public List<string>? GetDeprecatedModManifest()
	{
		return ModManifest;
	}
}
