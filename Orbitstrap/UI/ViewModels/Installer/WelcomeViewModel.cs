using Orbitstrap.Resources;

namespace Orbitstrap.UI.ViewModels.Installer;

public class WelcomeViewModel : NotifyPropertyChangedViewModel
{
	public string MainText => string.Format(Strings.Installer_Welcome_MainText, "[Discord Server](https://discord.gg/hxh4bceKer\n)");

	public bool CanContinue { get; set; }
}
