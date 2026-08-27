using System;
using System.Runtime.InteropServices;
using System.Windows;
using static Orbitstrap.Models.Persistable.AppSettings;

namespace Orbitstrap.Integrations
{
    public static class InGameResolutionApplier
    {
        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int CDS_UPDATEREGISTRY = 0x01;
        private const int DISP_CHANGE_SUCCESSFUL = 0;

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public ushort dmSpecVersion;
            public ushort dmDriverVersion;
            public ushort dmSize;
            public ushort dmDriverExtra;
            public uint dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public uint dmDisplayOrientation;
            public uint dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public ushort dmLogPixels;
            public uint dmBitsPerPel;
            public uint dmPelsWidth;
            public uint dmPelsHeight;
            public uint dmDisplayFlags;
            public uint dmDisplayFrequency;
            public uint dmICMMethod;
            public uint dmICMIntent;
            public uint dmMediaType;
            public uint dmDitherType;
            public uint dmReserved1;
            public uint dmReserved2;
            public uint dmPanningWidth;
            public uint dmPanningHeight;
        }

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        /// <summary>
        /// Reads the display's current resolution/refresh rate directly from Windows.
        /// This is the single source of truth for "what resolution are we on right now" -
        /// callers should not maintain their own duplicate DEVMODE P/Invoke definitions,
        /// since a mismatched/truncated struct layout will corrupt the marshaled buffer
        /// and can silently crash whatever loop called it.
        /// </summary>
        public static ResolutionSetting? GetCurrent()
        {
            try
            {
                DEVMODE dm = new();
                dm.dmSize = (ushort)Marshal.SizeOf(typeof(DEVMODE));

                if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
                    return null;

                return new ResolutionSetting
                {
                    Width = (int)dm.dmPelsWidth,
                    Height = (int)dm.dmPelsHeight,
                    RefreshRate = (int)dm.dmDisplayFrequency
                };
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("InGameResolution", $"Failed to read current resolution: {ex.Message}");
                return null;
            }
        }

        public static void Apply(ResolutionSetting res)
        {
            try
            {
                DEVMODE dm = new();
                dm.dmSize = (ushort)Marshal.SizeOf(typeof(DEVMODE));

                if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
                {
                    App.Logger.WriteLine("InGameResolution", "EnumDisplaySettings failed, cannot apply resolution");
                    return;
                }

                dm.dmPelsWidth = (uint)res.Width;
                dm.dmPelsHeight = (uint)res.Height;
                dm.dmDisplayFrequency = (uint)res.RefreshRate;
                dm.dmBitsPerPel = 32;
                dm.dmFields = 0x80000 | 0x100000 | 0x400000 | 0x40000; // DM_PELSWIDTH | DM_PELSHEIGHT | DM_DISPLAYFREQUENCY | DM_BITSPERPEL

                // CDS_UPDATEREGISTRY alone both persists the setting AND applies it to the
                // current session. CDS_RESET (0x40000000) is added explicitly to force an
                // immediate mode switch - without it some drivers only queue the change,
                // which is why the resolution appeared to "not work" while already in-game.
                const int CDS_RESET = 0x40000000;
                int result = ChangeDisplaySettings(ref dm, CDS_UPDATEREGISTRY | CDS_RESET);

                if (result != DISP_CHANGE_SUCCESSFUL)
                {
                    App.Logger.WriteLine(
                        "InGameResolution",
                        $"Failed to apply in-game resolution ({res.Width}x{res.Height}@{res.RefreshRate}), result code {result}"
                    );
                }
                else
                {
                    App.Logger.WriteLine(
                        "InGameResolution",
                        $"Applied resolution ({res.Width}x{res.Height}@{res.RefreshRate})"
                    );
                }
            }
            catch (Exception ex)
            {
                // Never let a resolution-change failure bubble up and kill the caller's
                // loop (e.g. the log-reading loop in ActivityWatcher) - just log it.
                App.Logger.WriteLine("InGameResolution", $"Exception while applying resolution: {ex.Message}");
            }
        }
    }
}
