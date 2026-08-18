using System.Windows.Input;
using System.Windows;
using System.Text.Json.Nodes;
using CommunityToolkit.Mvvm.Input;
using Orbitstrap.UI.ViewModels;
using Orbitstrap;
using Orbitstrap.UI.Elements.Dialogs;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;

namespace Orbitstrap.UI.ViewModels.Settings
{
    public class OrbitstrapViewModel : NotifyPropertyChangedViewModel
    {
        public bool ShouldExportConfig { get; set; } = true;

        public bool HWAsselEnabled
        {
            get => App.Settings.Prop.WPFSoftwareRender;
            set => App.Settings.Prop.WPFSoftwareRender = value;
        }

        public bool ShouldExportLogs { get; set; } = true;

        public ICommand ExportDataCommand => new RelayCommand(ExportData);

        public ICommand ImportDataCommand => new RelayCommand(ImportData);

        /// <summary>
        /// Device-specific settings that are never exported or imported, because
        /// they depend on the hardware of the machine they were configured on.
        /// </summary>
        private static readonly HashSet<string> CpuCoreSettings = new()
        {
            nameof(AppSettings.CpuCoreLimit),
            nameof(AppSettings.TotalLogicalCores),
            nameof(AppSettings.TotalPhysicalCores),
            nameof(AppSettings.MaxCpuCores)
        };

        /// <summary>
        /// AppSettings properties shown on the Deployment, Integration, Appearance
        /// and Settings tabs. Only these are exported by the "Settings" option.
        /// </summary>
        private static readonly HashSet<string> SettingsExportProperties = new()
        {
            // Settings tab (ChannelViewModel)
            nameof(AppSettings.BufferSizeKbte),
            nameof(AppSettings.BufferSizeKbtes),
            nameof(AppSettings.Channel),
            nameof(AppSettings.ChannelHash),
            nameof(AppSettings.ForceRobloxReinstallation),
            nameof(AppSettings.GameResolutionRules),
            nameof(AppSettings.InGameResolution),
            nameof(AppSettings.IsChannelEnabled),
            nameof(AppSettings.PlaceId),
            nameof(AppSettings.PriorityLimit),
            nameof(AppSettings.UpdateRoblox),
            nameof(AppSettings.UsePlaceId),
            nameof(AppSettings.VoidNotify),
            nameof(AppSettings.VoidRPC),
            nameof(AppSettings.WPFSoftwareRender),
            // Deployment tab (BehaviourViewModel)
            nameof(AppSettings.BackgroundWindow),
            nameof(AppSettings.CleanerDirectories),
            nameof(AppSettings.CleanerOptions),
            nameof(AppSettings.ConfirmLaunches),
            nameof(AppSettings.DisableCrash),
            nameof(AppSettings.Error773Fix),
            nameof(AppSettings.ExclusiveFullscreen),
            nameof(AppSettings.ForceRobloxLanguage),
            nameof(AppSettings.IsBetterServersEnabled),
            nameof(AppSettings.IsGameEnabled),
            nameof(AppSettings.MemoryCleanerIntervalSeconds),
            nameof(AppSettings.MultiAccount),
            nameof(AppSettings.MultiInstanceLaunching),
            nameof(AppSettings.OptimizeRoblox),
            nameof(AppSettings.OverClockCPU),
            nameof(AppSettings.OverClockGPU),
            nameof(AppSettings.RenameClientToEuroTrucks2),
            nameof(AppSettings.SelectedCpuPriority),
            // Integration tab (IntegrationsViewModel + SwiftTunnelViewModel)
            nameof(AppSettings.AutoRejoin),
            nameof(AppSettings.CustomGameName),
            nameof(AppSettings.CustomIntegrations),
            nameof(AppSettings.EnableActivityTracking),
            nameof(AppSettings.exitondissy),
            nameof(AppSettings.FFlagRPCDisplayer),
            nameof(AppSettings.GameCreatorChecked),
            nameof(AppSettings.GameIconChecked),
            nameof(AppSettings.GameNameChecked),
            nameof(AppSettings.GameStatusChecked),
            nameof(AppSettings.GameWIP),
            nameof(AppSettings.HideRPCButtons),
            nameof(AppSettings.IngameChatDiscord),
            nameof(AppSettings.NotificationWindowShow),
            nameof(AppSettings.ServerLocationGame),
            nameof(AppSettings.ServerUptimeBetterBLOXcuzitsbetterXD),
            nameof(AppSettings.ShowAccountOnRichPresence),
            nameof(AppSettings.ShowServerDetails),
            nameof(AppSettings.UseCustomIcon),
            nameof(AppSettings.UseDisableAppPatch),
            nameof(AppSettings.UseDiscordRichPresence),
            nameof(AppSettings.SwiftTunnelAutoConnect),
            nameof(AppSettings.SwiftTunnelEnabled),
            nameof(AppSettings.SwiftTunnelRegion),
            nameof(AppSettings.SwiftTunnelRememberLogin),
            nameof(AppSettings.SwiftTunnelSplitTunnel),
            // Appearance tab (AppearanceViewModel)
            nameof(AppSettings.BootstrapperIcon),
            nameof(AppSettings.BootstrapperIconCustomLocation),
            nameof(AppSettings.BootstrapperStyle),
            nameof(AppSettings.BootstrapperTitle),
            nameof(AppSettings.ClearFont),
            nameof(AppSettings.GRADmentFR),
            nameof(AppSettings.Locale),
            nameof(AppSettings.SelectedCustomTheme),
            nameof(AppSettings.SmooothBARRyesirikikthxlucipook),
            nameof(AppSettings.SnowWOWSOCOOLWpfSnowbtw),
            nameof(AppSettings.Theme2),
        };

        public void ExportData()
        {
            var transferDialog = new DataTransferDialog
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            transferDialog.ShowDialog();

            if (transferDialog.Result != MessageBoxResult.OK)
                return;

            bool exportMods = transferDialog.ExportMods;
            bool exportFastFlags = transferDialog.ExportFastFlags;
            bool exportSettings = transferDialog.ExportSettings;
            bool exportGlobalSettings = transferDialog.ExportGlobalSettings;

            if (!exportMods && !exportFastFlags && !exportSettings && !exportGlobalSettings)
            {
                Frontend.ShowMessageBox(Strings.Dialog_DataTransfer_NoSelection, MessageBoxImage.Warning);
                return;
            }

            string downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            Directory.CreateDirectory(downloadsPath);

            string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

            var dialog = new SaveFileDialog
            {
                FileName = $"Orbitstrap-data-{timestamp}.zip",
                Filter = $"{Strings.FileTypes_ZipArchive}|*.zip",
                InitialDirectory = downloadsPath,
                AddExtension = true
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                using var outputStream = File.Create(dialog.FileName);
                using (var zipStream = new ZipOutputStream(outputStream))
                {
                    zipStream.SetLevel(6);

                    if (transferDialog.ExportAllData)
                    {
                        AddAllDataConfigToZip(zipStream);
                        AddGlobalSettingsToZip(zipStream);
                    }
                    else
                    {
                        if (exportSettings)
                            AddSettingsToZip(zipStream);

                        if (exportGlobalSettings)
                            AddGlobalSettingsToZip(zipStream);
                    }

                    if (exportFastFlags)
                        AddFastFlagsToZip(zipStream);

                    if (exportMods)
                        AddModsToZip(zipStream);

                    if (transferDialog.ExportAllData)
                    {
                        AddFolderToZip(zipStream, Paths.CustomThemes, "Themes/");
                        AddFolderToZip(zipStream, Paths.CustomCursors, "Cursors/");
                    }

                    zipStream.Finish();
                }

                Frontend.ShowMessageBox(
                    string.Format(Strings.Dialog_DataTransfer_ExportComplete, dialog.FileName),
                    MessageBoxImage.Information
                );

                Process.Start("explorer.exe", $"/select,\"{dialog.FileName}\"");
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("OrbitstrapViewModel::ExportData", ex);
                Frontend.ShowMessageBox(
                    string.Format(Strings.Dialog_DataTransfer_ExportFailed, ex.Message),
                    MessageBoxImage.Error
                );
            }
        }

        public void ImportData()
        {
            var dialog = new OpenFileDialog
            {
                Title = Strings.Dialog_DataTransfer_ImportTitle,
                Filter = $"{Strings.FileTypes_ZipArchive}|*.zip",
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
                return;

            if (Frontend.ShowMessageBox(
                    Strings.Dialog_DataTransfer_ImportConfirm,
                    MessageBoxImage.Warning,
                    MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            string tempPath = Path.Combine(Path.GetTempPath(), $"OrbitstrapImport-{Guid.NewGuid():N}");

            try
            {
                Directory.CreateDirectory(tempPath);
                ExtractZipToDirectory(dialog.FileName, tempPath);

                RestoreConfigFiles(tempPath);
                RestoreFastFlags(tempPath);
                RestoreMods(tempPath);
                RestoreThemes(tempPath);
                RestoreCursors(tempPath);
                RestoreGlobalSettings(tempPath);

                // Refresh in-memory state so the UI reflects the imported data.
                App.Settings.Load(false);
                App.State.Load(false);
                App.FastFlags.Load(false);
                App.GlobalSettings.Load();

                Frontend.ShowMessageBox(
                    Strings.Dialog_DataTransfer_ImportComplete,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("OrbitstrapViewModel::ImportData", ex);
                Frontend.ShowMessageBox(
                    string.Format(Strings.Dialog_DataTransfer_ImportFailed, ex.Message),
                    MessageBoxImage.Error
                );
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempPath))
                        Directory.Delete(tempPath, recursive: true);
                }
                catch
                {
                }
            }
        }

        private void AddAllDataConfigToZip(ZipOutputStream zipStream)
        {
            JsonObject? settingsNode = JsonSerializer.SerializeToNode(App.Settings.Prop)?.AsObject();
            if (settingsNode is not null)
            {
                foreach (string cpuCoreSetting in CpuCoreSettings)
                    settingsNode.Remove(cpuCoreSetting);

                WriteJsonToZip(zipStream, "Config/AppSettings.json", settingsNode);
            }

            var files = new List<string>
            {
                App.State.FileLocation,
                App.DownloadStats.FileLocation,
                App.RobloxState.FileLocation,
                Path.Combine(Paths.Base, "OrbitstrapRobloxSaves.json")
            };

            AddFilesToZipStream(zipStream, files, "Config/");
        }

        private void AddSettingsToZip(ZipOutputStream zipStream)
        {
            JsonObject? settingsNode = JsonSerializer.SerializeToNode(App.Settings.Prop)?.AsObject();
            if (settingsNode is null)
                return;

            var filteredNode = new JsonObject();

            foreach (string propertyName in SettingsExportProperties)
            {
                if (settingsNode.TryGetPropertyValue(propertyName, out JsonNode? value) && value is not null)
                    filteredNode[propertyName] = value.DeepClone();
            }

            WriteJsonToZip(zipStream, "Config/AppSettings.json", filteredNode);
        }

        private void WriteJsonToZip(ZipOutputStream zipStream, string entryName, JsonObject json)
        {
            var entry = new ZipEntry(entryName)
            {
                DateTime = DateTime.Now
            };

            zipStream.PutNextEntry(entry);

            byte[] bytes = Encoding.UTF8.GetBytes(json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            zipStream.Write(bytes, 0, bytes.Length);

            zipStream.CloseEntry();
        }

        private void AddGlobalSettingsToZip(ZipOutputStream zipStream)
        {
            if (!File.Exists(App.GlobalSettings.FileLocation))
                return;

            var entry = new ZipEntry("GlobalSettings/GlobalBasicSettings_13.xml")
            {
                DateTime = DateTime.Now
            };

            zipStream.PutNextEntry(entry);

            using var fileStream = File.OpenRead(App.GlobalSettings.FileLocation);
            fileStream.CopyTo(zipStream);

            zipStream.CloseEntry();
        }

        private void AddFastFlagsToZip(ZipOutputStream zipStream)
        {
            if (!File.Exists(App.FastFlags.FileLocation))
                return;

            var entry = new ZipEntry("FFlags/ClientAppSettings.json")
            {
                DateTime = DateTime.Now
            };

            zipStream.PutNextEntry(entry);

            using var fileStream = File.OpenRead(App.FastFlags.FileLocation);
            fileStream.CopyTo(zipStream);

            zipStream.CloseEntry();
        }

        private void AddModsToZip(ZipOutputStream zipStream)
        {
            if (!Directory.Exists(Paths.Mods))
                return;

            string clientSettingsDir = Path.Combine(Paths.Mods, "ClientSettings");

            foreach (string filePath in Directory.EnumerateFiles(Paths.Mods, "*.*", SearchOption.AllDirectories))
            {
                // Fast flags are exported separately in their own FFlags/ folder.
                if (filePath.StartsWith(clientSettingsDir, StringComparison.OrdinalIgnoreCase))
                    continue;

                string relativePath = "Mods/" + filePath[(Paths.Mods.Length + 1)..].Replace('\\', '/');

                var entry = new ZipEntry(relativePath)
                {
                    DateTime = DateTime.Now
                };

                zipStream.PutNextEntry(entry);

                using var fileStream = File.OpenRead(filePath);
                fileStream.CopyTo(zipStream);

                zipStream.CloseEntry();
            }
        }

        private void AddFilesToZipStream(ZipOutputStream zipStream, IEnumerable<string> files, string directory)
        {
            foreach (string file in files)
            {
                if (!File.Exists(file))
                    continue;

                var entry = new ZipEntry(directory + Path.GetFileName(file));
                entry.DateTime = DateTime.Now;

                zipStream.PutNextEntry(entry);

                using var fileStream = File.OpenRead(file);
                fileStream.CopyTo(zipStream);

                zipStream.CloseEntry();
            }
        }

        private void AddFolderToZip(ZipOutputStream zipStream, string folderPath, string directory)
        {
            if (!Directory.Exists(folderPath))
                return;

            foreach (string filePath in Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories))
            {
                string relativePath = directory + filePath[(folderPath.Length + 1)..].Replace('\\', '/');

                var entry = new ZipEntry(relativePath)
                {
                    DateTime = DateTime.Now
                };

                zipStream.PutNextEntry(entry);

                using var fileStream = File.OpenRead(filePath);
                fileStream.CopyTo(zipStream);

                zipStream.CloseEntry();
            }
        }

        private void ExtractZipToDirectory(string zipFilePath, string extractPath)
        {
            using var zipInputStream = new ZipInputStream(File.OpenRead(zipFilePath));

            ZipEntry? entry;
            while ((entry = zipInputStream.GetNextEntry()) != null)
            {
                if (entry.IsDirectory)
                    continue;

                string filePath = Path.Combine(extractPath, entry.Name);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using var outputStream = File.Create(filePath);
                zipInputStream.CopyTo(outputStream);
            }
        }

        private static void CopyFileOverwrite(string source, string destination)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(source, destination, overwrite: true);
        }

        private static void CopyDirectoryOverwrite(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite: true);

            foreach (var subDir in Directory.GetDirectories(sourceDir))
                CopyDirectoryOverwrite(subDir, Path.Combine(destDir, Path.GetFileName(subDir)));
        }

        private void RestoreConfigFiles(string tempPath)
        {
            string configDir = Path.Combine(tempPath, "Config");
            if (!Directory.Exists(configDir))
                return;

            foreach (var file in Directory.GetFiles(configDir))
            {
                // AppSettings is merged so only the imported properties are applied
                // and device-specific settings (CPU cores) are never overwritten.
                if (Path.GetFileName(file) == "AppSettings.json")
                {
                    MergeAppSettings(file);
                    continue;
                }

                CopyFileOverwrite(file, Path.Combine(Paths.Base, Path.GetFileName(file)));
            }
        }

        private void MergeAppSettings(string importedPath)
        {
            JsonObject? imported;
            try
            {
                imported = JsonSerializer.Deserialize<JsonObject>(File.ReadAllText(importedPath));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("OrbitstrapViewModel::MergeAppSettings", ex);
                return;
            }

            if (imported is null)
                return;

            // Never apply device-specific CPU core settings.
            foreach (string cpuCoreSetting in CpuCoreSettings)
                imported.Remove(cpuCoreSetting);

            JsonObject current = JsonSerializer.SerializeToNode(App.Settings.Prop)!.AsObject();

            foreach (var pair in imported)
            {
                if (pair.Value is not null && current.ContainsKey(pair.Key))
                    current[pair.Key] = pair.Value.DeepClone();
            }

            try
            {
                App.Settings.Prop = JsonSerializer.Deserialize<AppSettings>(current.ToJsonString()) ?? App.Settings.Prop;
                App.Settings.Save();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("OrbitstrapViewModel::MergeAppSettings", ex);
            }
        }

        private void RestoreGlobalSettings(string tempPath)
        {
            string gbsPath = Path.Combine(tempPath, "GlobalSettings", "GlobalBasicSettings_13.xml");
            if (!File.Exists(gbsPath))
                return;

            App.GlobalSettings.SetReadOnly(false);
            CopyFileOverwrite(gbsPath, App.GlobalSettings.FileLocation);
        }

        private void RestoreFastFlags(string tempPath)
        {
            string fflagsDir = Path.Combine(tempPath, "FFlags");
            if (!Directory.Exists(fflagsDir))
                return;

            string clientSettingsDir = Path.Combine(Paths.Mods, "ClientSettings");
            Directory.CreateDirectory(clientSettingsDir);

            foreach (var file in Directory.GetFiles(fflagsDir))
                CopyFileOverwrite(file, Path.Combine(clientSettingsDir, Path.GetFileName(file)));
        }

        private void RestoreMods(string tempPath)
        {
            string modsDir = Path.Combine(tempPath, "Mods");
            if (!Directory.Exists(modsDir))
                return;

            CopyDirectoryOverwrite(modsDir, Paths.Mods);
        }

        private void RestoreThemes(string tempPath)
        {
            string themesDir = Path.Combine(tempPath, "Themes");
            if (!Directory.Exists(themesDir))
                return;

            CopyDirectoryOverwrite(themesDir, Paths.CustomThemes);
        }

        private void RestoreCursors(string tempPath)
        {
            string cursorsDir = Path.Combine(tempPath, "Cursors");
            if (!Directory.Exists(cursorsDir))
                return;

            CopyDirectoryOverwrite(cursorsDir, Paths.CustomCursors);
        }
    }
}