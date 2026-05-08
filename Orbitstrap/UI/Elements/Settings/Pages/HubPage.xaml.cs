using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Media;

namespace Orbitstrap.UI.Elements.Settings.Pages;

public partial class HubPage
{
    private void SearchBox_TextChanged(object s, System.Windows.Controls.TextChangedEventArgs e) { }
    private void Hyperlink_RequestNavigate(object s, System.Windows.Navigation.RequestNavigateEventArgs e) { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true }); e.Handled = true; }
}