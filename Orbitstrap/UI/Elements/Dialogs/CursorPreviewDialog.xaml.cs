using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Media;

namespace Orbitstrap.UI.Elements.Dialogs;

public partial class CursorPreviewDialog
{
    private void CancelButton_Click(object s, RoutedEventArgs e) { Close(); }
}