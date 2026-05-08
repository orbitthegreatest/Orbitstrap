using Orbitstrap.Models.SettingTasks;
using Orbitstrap.Resources;

namespace Orbitstrap.UI.ViewModels.Settings;

public class ShortcutsViewModel : NotifyPropertyChangedViewModel
{
	public bool IsStudioOptionVisible => App.IsStudioVisible;

	public ShortcutTask DesktopIconTask { get; } = new ShortcutTask("Desktop", Paths.Desktop, "Orbitstrap.lnk");

	public ShortcutTask StartMenuIconTask { get; } = new ShortcutTask("StartMenu", Paths.WindowsStartMenu, "Orbitstrap.lnk");

	public ShortcutTask PlayerIconTask { get; } = new ShortcutTask("RobloxPlayer", Paths.Desktop, Strings.LaunchMenu_LaunchRoblox + ".lnk", "-player");

	public ShortcutTask StudioIconTask { get; } = new ShortcutTask("RobloxStudio", Paths.Desktop, Strings.LaunchMenu_LaunchRobloxStudio + ".lnk", "-studio");

	public ShortcutTask SettingsIconTask { get; } = new ShortcutTask("Settings", Paths.Desktop, Strings.Menu_Title + ".lnk", "-settings");

	public ExtractIconsTask ExtractIconsTask { get; } = new ExtractIconsTask();
}
