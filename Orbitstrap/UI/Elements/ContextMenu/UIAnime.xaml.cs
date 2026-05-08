using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Media;

namespace Orbitstrap.UI.Elements.Overlay;

public partial class AnimeWindow
{
    private void Watch2GetherButton_Click(object s, RoutedEventArgs e) { }
    private void RandomButton_Click(object s, RoutedEventArgs e) { }
    private void CommunityButton_Click(object s, RoutedEventArgs e) { }
    private void FullscreenButton_Click(object s, RoutedEventArgs e) { }
    private void ExitButton_Click(object s, RoutedEventArgs e) { Close(); }
    private void Grid_MouseLeftButtonDown(object s, System.Windows.Input.MouseButtonEventArgs e) { DragMove(); }
}