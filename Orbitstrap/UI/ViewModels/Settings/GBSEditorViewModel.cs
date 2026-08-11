using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;
using Orbitstrap.UI.ViewModels;
using Orbitstrap.UI.ViewModels.ContextMenu;

namespace Orbitstrap.UI.ViewModels.Settings
{
    public class GBSEditorViewModel : NotifyPropertyChangedViewModel
    {
        private readonly string _settingsPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Roblox", "GlobalBasicSettings_13.xml");

        private XDocument? _doc;
        private XElement? _props;
        public ICommand ResetToDefaultsCommand { get; }

        public GBSEditorViewModel()
        {
            ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (!File.Exists(_settingsPath))
            {
                // Roblox hasn't created GlobalBasicSettings_13.xml yet (fresh install, or the
                // user has never launched Roblox on this machine). Previously _doc/_props were
                // left null in this case, and every SetValue() call is a silent no-op when
                // _props == null -- so any change made on this page before Roblox's first launch
                // would appear to "apply" in the UI but never actually persist. Build the same
                // skeleton document ResetToDefaults() creates and write it out immediately so
                // subsequent Set* calls have somewhere to save to.
                CreateSkeletonDocument();
                SaveSettings();
                return;
            }

            try
            {
                _doc = XDocument.Load(_settingsPath);
                _props = _doc.Descendants("Properties").FirstOrDefault();

                // Malformed/unexpected file (e.g. missing the Properties node): fall back to a
                // fresh skeleton rather than leaving _props null and silently dropping saves.
                if (_props == null)
                {
                    CreateSkeletonDocument();
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                // Corrupt XML: same story -- don't leave _props null, rebuild instead.
                System.Diagnostics.Debug.WriteLine($"Failed to load global settings, rebuilding: {ex}");
                CreateSkeletonDocument();
                SaveSettings();
            }
        }

        private void CreateSkeletonDocument()
        {
            _doc = new XDocument(
                new XElement("roblox",
                    new XElement("Item",
                        new XAttribute("class", "UserGameSettings"),
                        new XElement("Properties")
                    )
                )
            );

            _props = _doc.Descendants("Properties").FirstOrDefault();
        }

        private void SaveSettings()
        {
            try
            {
                // The "Roblox" folder under LocalAppData may not exist yet on a machine where
                // Roblox itself has never been launched -- Directory creation is required here
                // too, otherwise Save() throws DirectoryNotFoundException and is swallowed below,
                // which looked identical to "settings don't apply" from the UI's perspective.
                var directory = Path.GetDirectoryName(_settingsPath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                // The real culprit behind "nothing I change here ever sticks": Roblox itself
                // sets GlobalBasicSettings_13.xml read-only once it has started up, and will
                // silently regenerate it with its own defaults on the next launch if it's left
                // writable. Either way this Save() was a no-op -- it either threw straight into
                // the catch below (file already read-only) or wrote successfully only for Roblox
                // to blow the changes away moments later at the next launch.
                //
                // Always clear read-only before writing (so the save never fails on a locked
                // file), then conditionally restore it based on the user's preference.
                if (File.Exists(_settingsPath))
                {
                    var existing = File.GetAttributes(_settingsPath);
                    if (existing.HasFlag(FileAttributes.ReadOnly))
                        File.SetAttributes(_settingsPath, existing & ~FileAttributes.ReadOnly);
                }

                _doc?.Save(_settingsPath);

                // Re-lock as read-only only when the user wants Orbitstrap to protect the file
                // from being overwritten by Roblox on the next launch. When LockGlobalSettingsReadOnly
                // is false the file is left writable, so Roblox's own in-game settings menu can
                // still write back to it (at the cost of losing Orbitstrap-set values).
                if (App.Settings.Prop.LockGlobalSettingsReadOnly)
                {
                    var updated = File.GetAttributes(_settingsPath);
                    File.SetAttributes(_settingsPath, updated | FileAttributes.ReadOnly);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save global settings: {ex}");
            }
        }

        /// <summary>
        /// Mirrors <see cref="Models.Persistable.AppSettings.LockGlobalSettingsReadOnly"/>.
        /// When true, Orbitstrap re-locks the settings file as read-only after every save so
        /// Roblox cannot revert the user's customisations at next launch.
        /// </summary>
        public bool LockReadOnly
        {
            get => App.Settings.Prop.LockGlobalSettingsReadOnly;
            set
            {
                if (App.Settings.Prop.LockGlobalSettingsReadOnly == value) return;
                App.Settings.Prop.LockGlobalSettingsReadOnly = value;
                App.Settings.Save();

                // If the user just turned locking OFF, immediately remove the read-only flag
                // from the file (if it is currently set) so the change takes effect right away
                // rather than only at the next save cycle.
                if (!value && File.Exists(_settingsPath))
                {
                    try
                    {
                        var attrs = File.GetAttributes(_settingsPath);
                        if (attrs.HasFlag(FileAttributes.ReadOnly))
                            File.SetAttributes(_settingsPath, attrs & ~FileAttributes.ReadOnly);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to clear read-only flag: {ex}");
                    }
                }

                // If the user just turned locking ON, protect the file immediately.
                if (value && File.Exists(_settingsPath))
                {
                    try
                    {
                        var attrs = File.GetAttributes(_settingsPath);
                        if (!attrs.HasFlag(FileAttributes.ReadOnly))
                            File.SetAttributes(_settingsPath, attrs | FileAttributes.ReadOnly);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to set read-only flag: {ex}");
                    }
                }

                OnPropertyChanged();
            }
        }

        private string GetValue(string name, string defaultValue)
        {
            if (_props == null) return defaultValue;

            var element = _props.Elements().FirstOrDefault(e => e.Attribute("name")?.Value == name);
            if (element == null)
            {
                element = new XElement("string", defaultValue);
                element.SetAttributeValue("name", name);
                _props.Add(element);
                SaveSettings();
                return defaultValue;
            }

            return element.Value;
        }

        private void SetValue(string name, string value, string? type = null)
        {
            if (_props == null) return;

            var element = _props.Elements().FirstOrDefault(e => e.Attribute("name")?.Value == name);
            if (element == null)
            {
                element = new XElement(type ?? "string", value);
                element.SetAttributeValue("name", name);
                _props.Add(element);
            }
            else
            {
                element.Value = value;
            }

            SaveSettings();
        }

        private bool GetBool(string name, bool defaultValue = false)
        {
            var val = GetValue(name, defaultValue ? "true" : "false");
            return val.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private void SetBool(string name, bool value)
        {
            SetValue(name, value.ToString().ToLower(), "bool");
        }

        private int GetInt(string name, int defaultValue)
        {
            var val = GetValue(name, defaultValue.ToString());
            return int.TryParse(val, out var result) ? result : defaultValue;
        }

        private void SetInt(string name, int value)
        {
            SetValue(name, value.ToString(), "int");
        }

        private float GetFloat(string name, float defaultValue)
        {
            var val = GetValue(name, defaultValue.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
            return float.TryParse(val, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var result)
                ? result : defaultValue;
        }

        private void SetFloat(string name, float value)
        {
            SetValue(name, value.ToString("G", System.Globalization.CultureInfo.InvariantCulture), "float");
        }

        public float UITransparency
        {
            get => GetFloat("PreferredTransparency", 1f);
            set { SetFloat("PreferredTransparency", value); OnPropertyChanged(); }
        }

        public int PreferredTextSize
        {
            get => GetInt("PreferredTextSize", 1);
            set { SetInt("PreferredTextSize", value); OnPropertyChanged(); }
        }

        public bool ReducedMotion
        {
            get => GetBool("ReducedMotion", true);
            set { SetBool("ReducedMotion", value); OnPropertyChanged(); }
        }

        public bool HudVisible
        {
            get => GetBool("UsedHideHudShortcut", false) == false;
            set { SetBool("UsedHideHudShortcut", !value); OnPropertyChanged(); }
        }

        public int FramerateCap
        {
            get => GetInt("FramerateCap", 60);
            set { SetInt("FramerateCap", value); OnPropertyChanged(); }
        }

        public bool VignetteEnabled
        {
            get => GetBool("VignetteEnabled", true);
            set { SetBool("VignetteEnabled", value); OnPropertyChanged(); }
        }

        public int GraphicsQuality
        {
            get => GetInt("SavedQualityLevel", 10);
            set { SetInt("SavedQualityLevel", value); OnPropertyChanged(); }
        }

        public bool Fullscreen
        {
            get => GetBool("Fullscreen", true);
            set { SetBool("Fullscreen", value); OnPropertyChanged(); }
        }

        public bool VSyncEnabled
        {
            get => GetBool("VSyncEnabled", false);
            set { SetBool("VSyncEnabled", value); OnPropertyChanged(); }
        }

        public float MasterVolume
        {
            get => GetFloat("MasterVolume", 1f);
            set { SetFloat("MasterVolume", value); OnPropertyChanged(); }
        }

        public float VoiceChatVolume
        {
            get => GetFloat("PartyVoiceVolume", 1f);
            set { SetFloat("PartyVoiceVolume", value); OnPropertyChanged(); }
        }

        public float PartyVoiceVolume
        {
            get => GetFloat("PartyVoiceVolume", 1f);
            set { SetFloat("PartyVoiceVolume", value); OnPropertyChanged(); }
        }

        public float MouseSensitivity
        {
            get => GetFloat("MouseSensitivity", 1f);
            set { SetFloat("MouseSensitivity", value); OnPropertyChanged(); }
        }

        public bool CameraYInverted
        {
            get => GetBool("CameraYInverted", false);
            set { SetBool("CameraYInverted", value); OnPropertyChanged(); }
        }

        public float GamepadSensitivity
        {
            get => GetFloat("GamepadCameraSensitivity", 0.2f);
            set { SetFloat("GamepadCameraSensitivity", value); OnPropertyChanged(); }
        }

        public bool ControllerVibration
        {
            get => GetBool("HapticStrength", true);
            set { SetBool("HapticStrength", value); OnPropertyChanged(); }
        }

        public bool VREnabled
        {
            get => GetBool("VREnabled", false);
            set { SetBool("VREnabled", value); OnPropertyChanged(); }
        }

        public int VRComfortSetting
        {
            get => GetInt("VRComfortSetting", 2);
            set { SetInt("VRComfortSetting", value); OnPropertyChanged(); }
        }

        public bool NetworkStatsVisible
        {
            get => GetBool("PerformanceStatsVisible", false);
            set { SetBool("PerformanceStatsVisible", value); OnPropertyChanged(); }
        }

        public bool ChatTranslationEnabled
        {
            get => GetBool("ChatTranslationEnabled", true);
            set { SetBool("ChatTranslationEnabled", value); OnPropertyChanged(); }
        }

        public bool MicroProfilerWebServerEnabled
        {
            get => GetBool("MicroProfilerWebServerEnabled", false);
            set { SetBool("MicroProfilerWebServerEnabled", value); OnPropertyChanged(); }
        }

        public bool OnScreenProfilerEnabled
        {
            get => GetBool("OnScreenProfilerEnabled", false);
            set { SetBool("OnScreenProfilerEnabled", value); OnPropertyChanged(); }
        }

        public bool PerformanceStatsVisible
        {
            get => GetBool("PerformanceStatsVisible", false);
            set { SetBool("PerformanceStatsVisible", value); OnPropertyChanged(); }
        }

        public bool PlayerNamesEnabled
        {
            get => GetBool("PlayerNamesEnabled", true);
            set { SetBool("PlayerNamesEnabled", value); OnPropertyChanged(); }
        }

        public bool BadgeVisible
        {
            get => GetBool("BadgeVisible", true);
            set { SetBool("BadgeVisible", value); OnPropertyChanged(); }
        }

        public bool ChatVisible
        {
            get => GetBool("ChatVisible", true);
            set { SetBool("ChatVisible", value); OnPropertyChanged(); }
        }

        public void ResetToDefaults()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    File.Delete(_settingsPath);
                }

                CreateSkeletonDocument();
                SaveSettings();
                OnPropertyChanged(string.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to reset settings: {ex}");
            }
        }
    }
}
