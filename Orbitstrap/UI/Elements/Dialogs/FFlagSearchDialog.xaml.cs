using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Media;

namespace Orbitstrap.UI.Elements.Dialogs;

public partial class FFlagSearchDialog
{
    private void SearchTextBox_TextChanged(object s, System.Windows.Controls.TextChangedEventArgs e) { }
    private void CloseButton_Click(object s, RoutedEventArgs e) { Close(); }
    private void FetchRecentButton_Click(object s, RoutedEventArgs e) { }
    private void LoadFileButton_Click(object s, RoutedEventArgs e) { }
    private void DownloadAllRecentButton_Click(object s, RoutedEventArgs e) { }
    private void DownloadTrueRecentButton_Click(object s, RoutedEventArgs e) { }
    private void DownloadFalseRecentButton_Click(object s, RoutedEventArgs e) { }
    private void ExportSearchResultsButton_Click(object s, RoutedEventArgs e) { }
    private void ExportValidResultsButton_Click(object s, RoutedEventArgs e) { }
    private void ValidateButton_Click(object s, RoutedEventArgs e) { }
    private void ClearValidationButton_Click(object s, RoutedEventArgs e) { }
    private void TrueFlagsOnlyCheckBox_CheckedChanged(object s, RoutedEventArgs e) { }
    private void FalseFlagsOnlyCheckBox_CheckedChanged(object s, RoutedEventArgs e) { }
    private void ClearMenuItem_Click(object s, RoutedEventArgs e) { }
    private void PasteMenuItem_Click(object s, RoutedEventArgs e) { }
    private void SelectAllMenuItem_Click(object s, RoutedEventArgs e) { }
}