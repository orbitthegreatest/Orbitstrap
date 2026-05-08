using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Orbitstrap.UI.Elements.Settings.Pages
{
    public class ExtensionViewModel
    {
        private static readonly HttpClient _client = new();
        private static readonly string _fleasionDir = Path.Combine(Paths.Base, "Fleasion");
        private static readonly SemaphoreSlim _lock = new(1, 1);
        private CancellationTokenSource? _cts;

        public event Action<string, bool>? OnProgressChanged;

        public ExtensionViewModel()
        {
            if (App.Settings.Prop.FleasionEnabled)
            {
                string exe = Path.Combine(_fleasionDir, "Fleasion.exe");
                if (!File.Exists(exe))
                    _ = Task.Run(DownloadFleasionAsync);
            }
            else
            {
                _ = Task.Run(UninstallFleasionAsync);
            }
        }

        public bool FleasionEnabled
        {
            get => App.Settings.Prop.FleasionEnabled;
            set
            {
                if (App.Settings.Prop.FleasionEnabled == value) return;
                App.Settings.Prop.FleasionEnabled = value;
                _ = Task.Run(value ? DownloadFleasionAsync : UninstallFleasionAsync);
            }
        }

        public bool AniWatchEnabled
        {
            get => App.Settings.Prop.AniWatchEnabled;
            set => App.Settings.Prop.AniWatchEnabled = value;
        }

        public void CancelDownload() => _cts?.Cancel();

        private async Task DownloadFleasionAsync()
        {
            if (!await _lock.WaitAsync(0)) return;
            try
            {
                OnProgressChanged?.Invoke("Downloading Fleasion...", true);
                Directory.CreateDirectory(_fleasionDir);
                const string url = "https://github.com/Fleasion/Fleasion/releases/latest/download/Fleasion.exe";
                _cts = new CancellationTokenSource();
                using var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
                response.EnsureSuccessStatusCode();
                string dest = Path.Combine(_fleasionDir, "Fleasion.exe");
                await using var fs = File.Create(dest);
                await response.Content.CopyToAsync(fs, _cts.Token);
                OnProgressChanged?.Invoke("Fleasion installed.", false);
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("Download cancelled.", false);
            }
            catch (Exception ex)
            {
                OnProgressChanged?.Invoke($"Download failed: {ex.Message}", false);
            }
            finally
            {
                _lock.Release();
            }
        }

        private Task UninstallFleasionAsync()
        {
            try
            {
                if (Directory.Exists(_fleasionDir))
                    Directory.Delete(_fleasionDir, true);
            }
            catch { /* ignore */ }
            return Task.CompletedTask;
        }
    }
}
