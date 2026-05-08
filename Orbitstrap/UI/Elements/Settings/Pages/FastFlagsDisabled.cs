using System.Windows.Markup;
using Orbitstrap.UI.ViewModels.Settings;
using Wpf.Ui.Controls;

namespace Orbitstrap.UI.Elements.Settings.Pages;

public partial class FastFlagsDisabled : UiPage, IComponentConnector
{
	public FastFlagsDisabled()
	{
		base.DataContext = new FastFlagsDisabledViewModel(this);
		InitializeComponent();
	}
}
