using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;

namespace Orbitstrap.UI.Elements.Dialogs;

public partial class SwiftTunnelOAuthDialog
{
    private void CancelButton_Click(object s, RoutedEventArgs e) { Close(); }
    private void OAuthWebView_NavigationStarting(object s, CoreWebView2NavigationStartingEventArgs e) { }
    private void OAuthWebView_NavigationCompleted(object s, CoreWebView2NavigationCompletedEventArgs e) { }
}