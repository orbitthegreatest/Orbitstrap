using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Media;

namespace Orbitstrap.UI.Elements.Dialogs;

public partial class ChannelListsDialog
{
    private void ChannelDataGrid_PreviewKeyDown(object s, System.Windows.Input.KeyEventArgs e) { }
    private void Close_Click(object s, RoutedEventArgs e) { Close(); }
}