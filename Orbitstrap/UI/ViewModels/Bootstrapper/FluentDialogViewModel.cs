using System.Windows.Media;
using Orbitstrap.Enums;
using Orbitstrap.Extensions;
using Orbitstrap.Resources;
using Wpf.Ui.Appearance;

namespace Orbitstrap.UI.ViewModels.Bootstrapper;

public class FluentDialogViewModel : BootstrapperDialogViewModel
{
	public BackgroundType WindowBackdropType { get; set; } = BackgroundType.Mica;

	public SolidColorBrush BackgroundColourBrush { get; set; } = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

	public string VersionText { get; init; } = "None";

	public string ChannelText { get; init; } = "production";

	public FluentDialogViewModel(IBootstrapperDialog dialog, bool aero, string version, string channel)
		: base(dialog)
	{
		WindowBackdropType = (aero ? BackgroundType.Acrylic : BackgroundType.Mica);
		if (aero)
		{
			BackgroundColourBrush = ((App.Settings.Prop.Theme.GetFinal() == Orbitstrap.Enums.Theme.Light) ? new SolidColorBrush(Color.FromArgb(128, 225, 225, 225)) : new SolidColorBrush(Color.FromArgb(128, 30, 30, 30)));
		}
		VersionText = Strings.Common_Version + ": " + version;
		ChannelText = Strings.Common_Channel + ": " + channel;
	}
}
