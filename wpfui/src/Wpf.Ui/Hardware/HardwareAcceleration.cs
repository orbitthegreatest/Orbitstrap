// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Hardware;

/// <summary>
/// Set of tools for hardware acceleration.
/// </summary>
public static class HardwareAcceleration
{
    /// <summary>
    /// Determines whether the provided rendering tier is supported.
    /// </summary>
    /// <param name="tier">Hardware acceleration rendering tier to check.</param>
    /// <returns><see langword="true"/> if tier is supported.</returns>
    public static bool IsSupported(RenderingTier tier)
    {
        return RenderCapability.Tier >> 16 >= (int)tier;
    }

    /// <summary>
    /// Disables all WPF animations globally.
    /// </summary>
    public static void DisableAllAnimations()
    {
        // DefaultValue = 0 throws ArgumentException via ValidateValueCallback.
        // Setting it to null removes the frame-rate cap without crashing.
        try
        {
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = (int?)null }
            );
        }
        catch (InvalidOperationException)
        {
            // OverrideMetadata can only be called once per type; safe to ignore on repeat calls.
        }
    }

    /// <summary>
    /// Disables transparency/blur effects to reduce GPU load.
    /// </summary>
    public static void DisableTransparencyEffects()
    {
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
    }

    /// <summary>
    /// Triggers a GC collection to free managed memory.
    /// </summary>
    public static void FreeMemory()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
    }

    /// <summary>
    /// Reduces the process working set to minimize memory footprint.
    /// </summary>
    public static void MinimizeMemoryFootprint()
    {
        FreeMemory();
        // Trim the process working set
        var handle = System.Diagnostics.Process.GetCurrentProcess().Handle;
        SetProcessWorkingSetSize(handle, -1, -1);
    }

    /// <summary>
    /// Applies rendering hints to optimize visual performance.
    /// </summary>
    public static void OptimizeVisualRendering()
    {
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, nint dwMinimumWorkingSetSize, nint dwMaximumWorkingSetSize);
}
