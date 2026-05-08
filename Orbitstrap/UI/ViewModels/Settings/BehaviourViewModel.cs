using System.Collections.Generic;
using System.Linq;
using Orbitstrap.Enums;
using Orbitstrap.Extensions;
using Orbitstrap.Models;

namespace Orbitstrap.UI.ViewModels.Settings;

public class BehaviourViewModel : NotifyPropertyChangedViewModel
{
    private List<string> CleanerItems = App.Settings.Prop.CleanerDirectories;

    // ── CPU cores ────────────────────────────────────────────────────────────

    /// <summary>
    /// Options shown in the "Set CPU cores for Roblox" ComboBox.
    /// "Default" keeps all cores; numbered options set CPU affinity.
    /// </summary>
    public IReadOnlyList<string> CpuOptions { get; } = BuildCpuOptions();

    private static IReadOnlyList<string> BuildCpuOptions()
    {
        var list = new List<string> { "Default (All Cores)" };
        int cpuCount = System.Environment.ProcessorCount;
        for (int i = 1; i <= cpuCount; i++)
            list.Add(i == 1 ? "1 Core" : $"{i} Cores");
        return list.AsReadOnly();
    }

    public string SelectedCpuPriority
    {
        get
        {
            int cores = App.Settings.Prop.CpuCores;
            if (cores <= 0) return "Default (All Cores)";
            return cores == 1 ? "1 Core" : $"{cores} Cores";
        }
        set
        {
            if (value == null || value == "Default (All Cores)")
            {
                App.Settings.Prop.CpuCores = 0;
            }
            else
            {
                int parsed = int.TryParse(value.Split(' ')[0], out int n) ? n : 0;
                App.Settings.Prop.CpuCores = parsed;
            }
            OnPropertyChanged(nameof(CpuSummary));
        }
    }

    public string CpuSummary
    {
        get
        {
            int cores = App.Settings.Prop.CpuCores;
            if (cores <= 0) return "Roblox will use all available CPU cores.";
            return $"Roblox will be limited to {cores} CPU core{(cores == 1 ? "" : "s")}.";
        }
    }

    // ── Memory cleaner ───────────────────────────────────────────────────────

    public IReadOnlyList<MemoryCleanerIntervalOption> MemoryCleanerIntervals { get; }
        = MemoryCleanerIntervalOption.All;

    public MemoryCleanerIntervalOption SelectedMemoryCleanerInterval
    {
        get
        {
            int seconds = App.Settings.Prop.MemoryCleanerIntervalSeconds;
            return MemoryCleanerIntervalOption.All
                       .FirstOrDefault(o => o.Seconds == seconds)
                   ?? MemoryCleanerIntervalOption.All[0];
        }
        set
        {
            App.Settings.Prop.MemoryCleanerIntervalSeconds = value?.Seconds ?? 0;
        }
    }

    // ── Memory trim ──────────────────────────────────────────────────────────

    public bool MultiAccount
    {
        get => App.Settings.Prop.TrimRobloxMemory;
        set => App.Settings.Prop.TrimRobloxMemory = value;
    }

    // ── Optimise Roblox ──────────────────────────────────────────────────────

    public bool OptimizeRoblox
    {
        get => App.Settings.Prop.OptimizeRoblox;
        set => App.Settings.Prop.OptimizeRoblox = value;
    }

    // ── Existing properties ──────────────────────────────────────────────────

    public bool IsRobloxInstallationMissing
    {
        get
        {
            if (string.IsNullOrEmpty(App.RobloxState.Prop.Player.VersionGuid))
                return string.IsNullOrEmpty(App.RobloxState.Prop.Studio.VersionGuid);
            return false;
        }
    }

    public bool UpdateRoblox
    {
        get => App.Settings.Prop.UpdateRoblox && !IsRobloxInstallationMissing;
        set => App.Settings.Prop.UpdateRoblox = value;
    }

    public bool ConfirmLaunches
    {
        get => App.Settings.Prop.ConfirmLaunches;
        set => App.Settings.Prop.ConfirmLaunches = value;
    }

    public bool ForceRobloxLanguage
    {
        get => App.Settings.Prop.ForceRobloxLanguage;
        set => App.Settings.Prop.ForceRobloxLanguage = value;
    }

    public bool BackgroundUpdates
    {
        get => App.Settings.Prop.BackgroundUpdatesEnabled;
        set => App.Settings.Prop.BackgroundUpdatesEnabled = value;
    }

    public CleanerOptions SelectedCleanUpMode
    {
        get => App.Settings.Prop.CleanerOptions;
        set => App.Settings.Prop.CleanerOptions = value;
    }

    public IEnumerable<CleanerOptions> CleanerOptions { get; } = CleanerOptionsEx.Selections;

    public CleanerOptions CleanerOption
    {
        get => App.Settings.Prop.CleanerOptions;
        set => App.Settings.Prop.CleanerOptions = value;
    }

    public bool CleanerLogs
    {
        get => CleanerItems.Contains("RobloxLogs");
        set
        {
            if (value) CleanerItems.Add("RobloxLogs");
            else CleanerItems.Remove("RobloxLogs");
        }
    }

    public bool CleanerCache
    {
        get => CleanerItems.Contains("RobloxCache");
        set
        {
            if (value) CleanerItems.Add("RobloxCache");
            else CleanerItems.Remove("RobloxCache");
        }
    }

    public bool CleanerOrbitstrap
    {
        get => CleanerItems.Contains("OrbitstrapLogs");
        set
        {
            if (value) CleanerItems.Add("OrbitstrapLogs");
            else CleanerItems.Remove("OrbitstrapLogs");
        }
    }

    public bool ForceRobloxReinstallation
    {
        get => App.State.Prop.ForceReinstall || IsRobloxInstallationMissing;
        set => App.State.Prop.ForceReinstall = value;
    }
}
