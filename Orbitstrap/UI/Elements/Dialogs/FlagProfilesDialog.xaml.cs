using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Media;

namespace Orbitstrap.UI.Elements.Dialogs;

public partial class FlagProfilesDialog
{
    private void OKButton_Click(object s, RoutedEventArgs e) { DialogResult = true; Close(); }
    private void DeleteButton_Click(object s, RoutedEventArgs e) { }
}