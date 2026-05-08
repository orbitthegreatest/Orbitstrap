using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Wpf.Ui.Controls;

namespace Orbitstrap.UI.Elements.Settings.Pages
{
    public partial class ExtensionPage : UiPage
    {
        private ExtensionViewModel _vm;

        public ExtensionPage()
        {
            InitializeComponent();
            _vm = new ExtensionViewModel();
            DataContext = _vm;
            _vm.OnProgressChanged += Vm_OnProgressChanged;
        }

        private void Vm_OnProgressChanged(string title, bool show)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressTitle.Text = title;
                ProgressOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void CancelDownload_Click(object sender, RoutedEventArgs e)
        {
            _vm?.CancelDownload();
        }

        private void OpenFleasion_Click(object sender, RoutedEventArgs e)
        {
            string fleasionDir = Path.Combine(Paths.Base, "Fleasion");
            string fleasionExe = Path.Combine(fleasionDir, "Fleasion.exe");
            if (Directory.Exists(fleasionDir) && File.Exists(fleasionExe))
            {
                try
                {
                    var running = Process.GetProcessesByName("Fleasion");
                    if (running.Length == 0)
                        Process.Start(fleasionExe);
                }
                catch (Exception ex)
                {
                    Frontend.ShowMessageBox("Failed to open Fleasion: " + ex.Message);
                }
            }
            else
            {
                Frontend.ShowMessageBox("Fleasion is not installed. Enable it with the toggle first.");
            }
        }

        private void OpenAniWatchWindow_Click(object sender, RoutedEventArgs e)
        {
            // AniWatch window — wire up your AnimeWindow here
            Frontend.ShowMessageBox("AniWatch is enabled. Launch it from the system tray.");
        }
    }
}
