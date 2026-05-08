using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Media;

namespace Orbitstrap.UI.Elements.Overlay;

public partial class DiscordChatOverlayWindow
{
    private void WindowDrag(object s, System.Windows.Input.MouseButtonEventArgs e) { DragMove(); }
    private void Image_MouseLeftButtonDown(object s, System.Windows.Input.MouseButtonEventArgs e) { }
    private void Reaction_Click(object s, RoutedEventArgs e) { }
}