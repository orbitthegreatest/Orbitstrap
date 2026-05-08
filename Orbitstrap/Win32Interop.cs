// Win32Interop.cs
// Manual P/Invoke declarations replacing Microsoft.Windows.CsWin32 generated code.
// Defines types in the same namespaces so existing using directives work unchanged.

using System;
using System.Runtime.InteropServices;

namespace Windows.Win32.Foundation
{
    internal readonly struct HWND
    {
        internal readonly IntPtr Value;
        internal HWND(IntPtr value) { Value = value; }
        public static explicit operator HWND(IntPtr value) => new HWND(value);
        public static HWND Null => new HWND(IntPtr.Zero);
    }
}

namespace Windows.Win32.UI.Shell
{
    internal enum SHOP_TYPE : uint
    {
        SHOP_PRINTERNAME = 1u,
        SHOP_FILEPATH = 2u,
        SHOP_VOLUMEGUID = 4u
    }
}

namespace Windows.Win32.UI.WindowsAndMessaging
{
    internal enum WINDOW_LONG_PTR_INDEX : int
    {
        GWL_EXSTYLE = -20,
        GWL_HINSTANCE = -6,
        GWL_HWNDPARENT = -8,
        GWL_ID = -12,
        GWL_STYLE = -16,
        GWL_USERDATA = -21,
        GWL_WNDPROC = -4
    }
}

namespace Windows.Win32
{
    using Windows.Win32.Foundation;
    using Windows.Win32.UI.Shell;
    using Windows.Win32.UI.WindowsAndMessaging;

    internal static class PInvoke
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(HWND hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FlashWindow(HWND hWnd, [MarshalAs(UnmanagedType.Bool)] bool bInvert);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        private static extern int GetWindowLong32(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr64(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex);

        internal static int GetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex)
        {
            if (IntPtr.Size == 8)
                return (int)GetWindowLongPtr64(hWnd, nIndex);
            return GetWindowLong32(hWnd, nIndex);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int SetWindowLong32(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, IntPtr dwNewLong);

        internal static int SetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, int dwNewLong)
        {
            if (IntPtr.Size == 8)
                return (int)SetWindowLongPtr64(hWnd, nIndex, new IntPtr(dwNewLong));
            return SetWindowLong32(hWnd, nIndex, dwNewLong);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SHObjectProperties(HWND hwnd, SHOP_TYPE shopObjectType,
            [MarshalAs(UnmanagedType.LPWStr)] string pszObjectName,
            [MarshalAs(UnmanagedType.LPWStr)] string pszPropertyPage);
    }
}
