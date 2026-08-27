using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Orbitstrap.Enums;

namespace Orbitstrap.Integrations
{
    public static class DpiApplier
    {
        #region Win32 Constants

        private const int SPI_SETMOUSESPEED = 0x0071;
        private const int SPI_GETMOUSESPEED = 0x0070;
        private const int MOUSE_SPEED_MIN = 1;
        private const int MOUSE_SPEED_MAX = 20;

        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

        private const int DIGCF_PRESENT = 0x02;
        private const int DIGCF_DEVICEINTERFACE = 0x10;

        private const uint IOCTL_HID_SET_FEATURE = 0xB0190;
        private const uint IOCTL_HID_GET_FEATURE = 0xB01A0;
        private const uint IOCTL_HID_GET_COLLECTION_DESCRIPTOR = 0xB0100;

        #endregion

        #region Win32 P/Invoke

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(int uAction, int uParam, ref int lParam, int fwWinIni);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern void HidD_GetHidGuid(out Guid hidGuid);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern bool HidD_GetAttributes(IntPtr HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern bool HidD_SetFeature(IntPtr HidDeviceObject, byte[] ReportBuffer, int ReportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern bool HidD_GetFeature(IntPtr HidDeviceObject, byte[] ReportBuffer, int ReportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern bool HidD_GetPreparsedData(IntPtr HidDeviceObject, out IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern bool HidD_FreePreparsedData(IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        private static extern int HidP_GetCaps(IntPtr PreparsedData, out HIDP_CAPS Capabilities);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, int Flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref Guid InterfaceClassGuid, int MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize, ref int RequiredSize, IntPtr DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, byte[] lpInBuffer, uint nInBufferSize, byte[] lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        #endregion

        #region Win32 Structs

        [StructLayout(LayoutKind.Sequential)]
        private struct HIDD_ATTRIBUTES
        {
            public int Size;
            public ushort VendorID;
            public ushort ProductID;
            public ushort VersionNumber;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HIDP_CAPS
        {
            public ushort Usage;
            public ushort UsagePage;
            public ushort InputReportByteLength;
            public ushort OutputReportByteLength;
            public ushort FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public ushort[] Reserved;
            public ushort NumberLinkCollectionNodes;
            public ushort NumberInputButtonCaps;
            public ushort NumberInputValueCaps;
            public ushort NumberInputDataIndices;
            public ushort NumberOutputButtonCaps;
            public ushort NumberOutputValueCaps;
            public ushort NumberOutputDataIndices;
            public ushort NumberFeatureButtonCaps;
            public ushort NumberFeatureValueCaps;
            public ushort NumberFeatureDataIndices;
        }

        #endregion

        #region Attack Shark HID Protocol

        private const ushort ATTACK_SHARK_VID = 0x1D57;
        private const int ATTACK_SHARK_INTERFACE = 2;
        private const int DPI_BUFFER_SIZE = 56;

        private static readonly Dictionary<int, byte> DpiStepMap = new()
        {
            { 50, 0x01 }, { 100, 0x02 }, { 150, 0x03 }, { 200, 0x04 }, { 250, 0x05 },
            { 300, 0x06 }, { 350, 0x08 }, { 400, 0x09 }, { 450, 0x0A }, { 500, 0x0B },
            { 550, 0x0C }, { 600, 0x0E }, { 650, 0x0F }, { 700, 0x10 }, { 750, 0x11 },
            { 800, 0x12 }, { 850, 0x13 }, { 900, 0x15 }, { 950, 0x16 }, { 1000, 0x17 },
            { 1050, 0x18 }, { 1100, 0x19 }, { 1150, 0x1B }, { 1200, 0x1C }, { 1250, 0x1D },
            { 1300, 0x1E }, { 1350, 0x1F }, { 1400, 0x20 }, { 1450, 0x22 }, { 1500, 0x23 },
            { 1550, 0x24 }, { 1600, 0x25 }, { 1650, 0x26 }, { 1700, 0x27 }, { 1750, 0x29 },
            { 1800, 0x2A }, { 1850, 0x2B }, { 1900, 0x2C }, { 1950, 0x2D }, { 2000, 0x2F },
            { 2050, 0x30 }, { 2100, 0x31 }, { 2150, 0x32 }, { 2200, 0x33 }, { 2250, 0x34 },
            { 2300, 0x36 }, { 2350, 0x37 }, { 2400, 0x38 }, { 2450, 0x39 }, { 2500, 0x3A },
            { 2550, 0x3B }, { 2600, 0x3D }, { 2650, 0x3E }, { 2700, 0x3F }, { 2750, 0x40 },
            { 2800, 0x41 }, { 2850, 0x43 }, { 2900, 0x44 }, { 2950, 0x45 }, { 3000, 0x46 },
            { 3050, 0x47 }, { 3100, 0x48 }, { 3150, 0x4A }, { 3200, 0x4B }, { 3250, 0x4C },
            { 3300, 0x4D }, { 3350, 0x4E }, { 3400, 0x4F }, { 3450, 0x51 }, { 3500, 0x52 },
            { 3550, 0x53 }, { 3600, 0x54 }, { 3650, 0x55 }, { 3700, 0x57 }, { 3750, 0x58 },
            { 3800, 0x59 }, { 3850, 0x5A }, { 3900, 0x5B }, { 3950, 0x5C }, { 4000, 0x5E },
            { 4050, 0x5F }, { 4100, 0x60 }, { 4150, 0x61 }, { 4200, 0x62 }, { 4250, 0x63 },
            { 4300, 0x65 }, { 4350, 0x66 }, { 4400, 0x67 }, { 4450, 0x68 }, { 4500, 0x69 },
            { 4550, 0x6B }, { 4600, 0x6C }, { 4650, 0x6D }, { 4700, 0x6E }, { 4750, 0x6F },
            { 4800, 0x70 }, { 4850, 0x72 }, { 4900, 0x73 }, { 4950, 0x74 }, { 5000, 0x75 },
            { 5050, 0x76 }, { 5100, 0x77 }, { 5150, 0x79 }, { 5200, 0x7A }, { 5250, 0x7B },
            { 5300, 0x7C }, { 5350, 0x7D }, { 5400, 0x7F }, { 5450, 0x80 }, { 5500, 0x81 },
            { 5550, 0x82 }, { 5600, 0x83 }, { 5650, 0x84 }, { 5700, 0x86 }, { 5750, 0x87 },
            { 5800, 0x88 }, { 5850, 0x89 }, { 5900, 0x8A }, { 5950, 0x8B }, { 6000, 0x8D },
            { 6050, 0x8E }, { 6100, 0x8F }, { 6150, 0x90 }, { 6200, 0x91 }, { 6250, 0x93 },
            { 6300, 0x94 }, { 6350, 0x95 }, { 6400, 0x96 }, { 6450, 0x97 }, { 6500, 0x98 },
            { 6550, 0x9A }, { 6600, 0x9B }, { 6650, 0x9C }, { 6700, 0x9D }, { 6750, 0x9E },
            { 6800, 0x9F }, { 6850, 0xA1 }, { 6900, 0xA2 }, { 6950, 0xA3 }, { 7000, 0xA4 },
            { 7050, 0xA5 }, { 7100, 0xA7 }, { 7150, 0xA8 }, { 7200, 0xA9 }, { 7250, 0xAA },
            { 7300, 0xAB }, { 7350, 0xAC }, { 7400, 0xAE }, { 7450, 0xAF }, { 7500, 0xB0 },
            { 7550, 0xB1 }, { 7600, 0xB2 }, { 7650, 0xB3 }, { 7700, 0xB5 }, { 7750, 0xB6 },
            { 7800, 0xB7 }, { 7850, 0xB8 }, { 7900, 0xB9 }, { 7950, 0xBB }, { 8000, 0xBC },
            { 8050, 0xBD }, { 8100, 0xBE }, { 8150, 0xBF }, { 8200, 0xC0 }, { 8250, 0xC2 },
            { 8300, 0xC3 }, { 8350, 0xC4 }, { 8400, 0xC5 }, { 8450, 0xC6 }, { 8500, 0xC7 },
            { 8550, 0xC9 }, { 8600, 0xCA }, { 8650, 0xCB }, { 8700, 0xCC }, { 8750, 0xCD },
            { 8800, 0xCF }, { 8850, 0xD0 }, { 8900, 0xD1 }, { 8950, 0xD2 }, { 9000, 0xD3 },
            { 9050, 0xD4 }, { 9100, 0xD6 }, { 9150, 0xD7 }, { 9200, 0xD8 }, { 9250, 0xD9 },
            { 9300, 0xDA }, { 9350, 0xDB }, { 9400, 0xDD }, { 9450, 0xDE }, { 9500, 0xDF },
            { 9550, 0xE0 }, { 9600, 0xE1 }, { 9650, 0xE3 }, { 9700, 0xE4 }, { 9750, 0xE5 },
            { 9800, 0xE6 }, { 9850, 0xE7 }, { 9900, 0xE8 }, { 9950, 0xEA }, { 10000, 0xEB },
            { 10100, 0x76 }, { 10200, 0x77 }, { 10300, 0x79 }, { 10400, 0x7A }, { 10500, 0x7B },
            { 10600, 0x7C }, { 10700, 0x7D }, { 10800, 0x7F }, { 10900, 0x80 }, { 11000, 0x81 },
            { 11100, 0x82 }, { 11200, 0x83 }, { 11300, 0x84 }, { 11400, 0x86 }, { 11500, 0x87 },
            { 11600, 0x88 }, { 11700, 0x89 }, { 11800, 0x8A }, { 11900, 0x8B }, { 12000, 0x8D },
            { 12100, 0x8E }, { 12200, 0x8F }, { 12300, 0x90 }, { 12400, 0x91 }, { 12500, 0x93 },
            { 12600, 0x94 }, { 12700, 0x95 }, { 12800, 0x96 }, { 12900, 0x97 }, { 13000, 0x98 },
            { 13100, 0x9A }, { 13200, 0x9B }, { 13300, 0x9C }, { 13400, 0x9D }, { 13500, 0x9E },
            { 13600, 0x9F }, { 13700, 0xA1 }, { 13800, 0xA2 }, { 13900, 0xA3 }, { 14000, 0xA4 },
            { 14100, 0xA5 }, { 14200, 0xA7 }, { 14300, 0xA8 }, { 14400, 0xA9 }, { 14500, 0xAA },
            { 14600, 0xAB }, { 14700, 0xAC }, { 14800, 0xAE }, { 14900, 0xAF }, { 15000, 0xB0 },
            { 15100, 0xB1 }, { 15200, 0xB2 }, { 15300, 0xB3 }, { 15400, 0xB5 }, { 15500, 0xB6 },
            { 15600, 0xB7 }, { 15700, 0xB8 }, { 15800, 0xB9 }, { 15900, 0xBB }, { 16000, 0xBC },
            { 16100, 0xBD }, { 16200, 0xBE }, { 16300, 0xBF }, { 16400, 0xC0 }, { 16500, 0xC2 },
            { 16600, 0xC3 }, { 16700, 0xC4 }, { 16800, 0xC5 }, { 16900, 0xC6 }, { 17000, 0xC7 },
            { 17100, 0xC9 }, { 17200, 0xCA }, { 17300, 0xCB }, { 17400, 0xCC }, { 17500, 0xCD },
            { 17600, 0xCF }, { 17700, 0xD0 }, { 17800, 0xD1 }, { 17900, 0xD2 }, { 18000, 0xD3 },
            { 18100, 0xD4 }, { 18200, 0xD6 }, { 18300, 0xD7 }, { 18400, 0xD8 }, { 18500, 0xD9 },
            { 18600, 0xDA }, { 18700, 0xDB }, { 18800, 0xDD }, { 18900, 0xDE }, { 19000, 0xDF },
            { 19100, 0xE0 }, { 19200, 0xE1 }, { 19300, 0xE3 }, { 19400, 0xE4 }, { 19500, 0xE5 },
            { 19600, 0xE6 }, { 19700, 0xE7 }, { 19800, 0xE8 }, { 19900, 0xEA }, { 20000, 0xEB },
            { 22000, 0x81 }
        };

        private static byte FindClosestDpiStep(int dpi)
        {
            if (DpiStepMap.TryGetValue(dpi, out byte exact))
                return exact;

            int closest = DpiStepMap.Keys.Aggregate((x, y) =>
                Math.Abs(x - dpi) < Math.Abs(y - dpi) ? x : y);

            App.Logger.WriteLine("DpiApplier", $"DPI {dpi} not in map, using closest: {closest}");
            return DpiStepMap[closest];
        }

        private static bool SendHidFeatureReport(IntPtr handle, byte[] buffer, int reportLength)
        {
            // Try HidD_SetFeature first (simpler wrapper)
            bool result = HidD_SetFeature(handle, buffer, reportLength);
            if (result)
                return true;

            int err1 = Marshal.GetLastWin32Error();
            App.Logger.WriteLine("DpiApplier", $"HidD_SetFeature failed (err={err1}), trying DeviceIoControl");

            // Fallback: DeviceIoControl with IOCTL_HID_SET_FEATURE
            // HID_XFER_INFO: report ID at start of output buffer
            byte[] outBuffer = new byte[reportLength];
            Array.Copy(buffer, outBuffer, reportLength);

            result = DeviceIoControl(
                handle,
                IOCTL_HID_SET_FEATURE,
                buffer, (uint)reportLength,
                outBuffer, (uint)outBuffer.Length,
                out _,
                IntPtr.Zero);

            if (!result)
            {
                int err2 = Marshal.GetLastWin32Error();
                App.Logger.WriteLine("DpiApplier", $"DeviceIoControl IOCTL_HID_SET_FEATURE also failed (err={err2})");
            }

            return result;
        }

        private static bool ApplyAttackSharkDpi(int dpi)
        {
            try
            {
                List<string> devicePaths = FindAllAttackSharkDevices();
                if (devicePaths.Count == 0)
                {
                    App.Logger.WriteLine("DpiApplier", "No Attack Shark device found (VID 0x1D57)");
                    return false;
                }

                App.Logger.WriteLine("DpiApplier", $"Found {devicePaths.Count} Attack Shark HID path(s), trying each...");

                byte dpiStep = FindClosestDpiStep(dpi);
                byte[] reportData = BuildAttackSharkDpiBuffer(dpi, dpiStep);

                foreach (string devicePath in devicePaths)
                {
                    App.Logger.WriteLine("DpiApplier", $"Trying path: {devicePath}");

                    IntPtr handle = CreateFile(
                        devicePath,
                        GENERIC_READ | GENERIC_WRITE,
                        FILE_SHARE_READ | FILE_SHARE_WRITE,
                        IntPtr.Zero,
                        OPEN_EXISTING,
                        0,
                        IntPtr.Zero);

                    if (handle == INVALID_HANDLE_VALUE)
                    {
                        App.Logger.WriteLine("DpiApplier", $"  Cannot open (err={Marshal.GetLastWin32Error()}), skipping");
                        continue;
                    }

                    try
                    {
                        int featureReportLen = DPI_BUFFER_SIZE;
                        if (HidD_GetPreparsedData(handle, out IntPtr preparsedData))
                        {
                            try
                            {
                                if (HidP_GetCaps(preparsedData, out HIDP_CAPS caps) == 0 && caps.FeatureReportByteLength > 0)
                                {
                                    featureReportLen = caps.FeatureReportByteLength;
                                    App.Logger.WriteLine("DpiApplier", $"  HID caps: FeatureReportByteLength={featureReportLen}");
                                }
                            }
                            finally
                            {
                                HidD_FreePreparsedData(preparsedData);
                            }
                        }

                        byte[] buffer = new byte[featureReportLen];
                        Array.Copy(reportData, buffer, Math.Min(reportData.Length, buffer.Length));

                        App.Logger.WriteLine("DpiApplier", $"  Buffer hex: {BitConverter.ToString(buffer).Replace("-", " ")}");

                        Thread.Sleep(300);

                        bool sent = SendHidFeatureReport(handle, buffer, featureReportLen);
                        if (sent)
                        {
                            App.Logger.WriteLine("DpiApplier", $"Attack Shark DPI set to {dpi} (step=0x{dpiStep:X2}) via {devicePath}");
                            return true;
                        }

                        App.Logger.WriteLine("DpiApplier", $"  Feature report rejected on this collection");
                    }
                    finally
                    {
                        CloseHandle(handle);
                    }
                }

                App.Logger.WriteLine("DpiApplier", "Attack Shark: no HID collection accepted the feature report");
                return false;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("DpiApplier", $"Attack Shark HID error: {ex.Message}");
                return false;
            }
        }

        private static byte[] BuildAttackSharkDpiBuffer(int dpi, byte dpiStep)
        {
            byte[] buffer = new byte[DPI_BUFFER_SIZE];

            buffer[0] = 0x04;   // Report ID
            buffer[1] = 0x38;   // Header 2
            buffer[2] = 0x01;   // Header 3
            // buffer[3] = Angle Snap (0x00 = disabled)
            // buffer[4] = Ripple Control (0x00 = disabled)
            buffer[5] = 0x3F;   // Fixed value

            byte stageMask = 0;
            if (dpi > 12000)
                stageMask = 0x01;
            buffer[6] = stageMask;  // Stage Mask A
            buffer[7] = stageMask;  // Stage Mask B

            for (int i = 8; i <= 13; i++)
                buffer[i] = dpiStep; // Stages 1-6 DPI

            // buffer[14] = Fixed 0x00
            // buffer[15] = Fixed 0x00

            byte highFlag = (byte)(dpi > 10000 ? 0x01 : 0x00);
            for (int i = 16; i <= 21; i++)
                buffer[i] = highFlag; // High Flags 1-6

            // buffer[22] = Fixed 0x00
            // buffer[23] = Fixed 0x00
            buffer[24] = 0x01; // Active Stage = 1

            // buffer[25]-[49] = Fixed Data (zeros)

            int sum = 0;
            for (int i = 3; i <= 49; i++)
                sum += buffer[i];

            buffer[50] = (byte)((sum >> 8) & 0xFF);
            buffer[51] = (byte)(sum & 0xFF);

            return buffer;
        }

        #endregion

        #region HID Device Enumeration

        private static List<string> FindAllAttackSharkDevices()
        {
            return FindAllHidDevicesByVid(ATTACK_SHARK_VID);
        }

        private static string? FindAttackSharkDevice()
        {
            return FindHidDeviceByVid(ATTACK_SHARK_VID, ATTACK_SHARK_INTERFACE);
        }

        private static List<string> FindAllHidDevicesByVid(ushort vendorId)
        {
            var results = new List<string>();
            HidD_GetHidGuid(out Guid hidGuid);

            IntPtr devInfo = SetupDiGetClassDevs(ref hidGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
            if (devInfo == INVALID_HANDLE_VALUE)
                return results;

            try
            {
                SP_DEVICE_INTERFACE_DATA interfaceData = new();
                interfaceData.cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DATA));

                for (int i = 0; SetupDiEnumDeviceInterfaces(devInfo, IntPtr.Zero, ref hidGuid, i, ref interfaceData); i++)
                {
                    int requiredSize = 0;
                    SetupDiGetDeviceInterfaceDetail(devInfo, ref interfaceData, IntPtr.Zero, 0, ref requiredSize, IntPtr.Zero);

                    IntPtr detailData = Marshal.AllocHGlobal(requiredSize);
                    try
                    {
                        Marshal.WriteInt32(detailData, 8);
                        if (SetupDiGetDeviceInterfaceDetail(devInfo, ref interfaceData, detailData, requiredSize, ref requiredSize, IntPtr.Zero))
                        {
                            string devicePath = Marshal.PtrToStringAuto(detailData + 4);
                            if (devicePath is null)
                                continue;

                            IntPtr handle = CreateFile(devicePath, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                            if (handle == INVALID_HANDLE_VALUE)
                                continue;

                            try
                            {
                                HIDD_ATTRIBUTES attrs = new() { Size = Marshal.SizeOf(typeof(HIDD_ATTRIBUTES)) };
                                if (HidD_GetAttributes(handle, ref attrs) && attrs.VendorID == vendorId)
                                {
                                    results.Add(devicePath);
                                    App.Logger.WriteLine("DpiApplier", $"Found HID device VID=0x{vendorId:X4} PID=0x{attrs.ProductID:X4}: {devicePath}");
                                }
                            }
                            finally
                            {
                                CloseHandle(handle);
                            }
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(detailData);
                    }
                }
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(devInfo);
            }

            return results;
        }

        private static string? FindHidDeviceByVid(ushort vendorId, int? requiredInterface = null)
        {
            HidD_GetHidGuid(out Guid hidGuid);

            IntPtr devInfo = SetupDiGetClassDevs(ref hidGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
            if (devInfo == INVALID_HANDLE_VALUE)
                return null;

            try
            {
                SP_DEVICE_INTERFACE_DATA interfaceData = new();
                interfaceData.cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DATA));

                for (int i = 0; SetupDiEnumDeviceInterfaces(devInfo, IntPtr.Zero, ref hidGuid, i, ref interfaceData); i++)
                {
                    int requiredSize = 0;
                    SetupDiGetDeviceInterfaceDetail(devInfo, ref interfaceData, IntPtr.Zero, 0, ref requiredSize, IntPtr.Zero);

                    IntPtr detailData = Marshal.AllocHGlobal(requiredSize);
                    try
                    {
                        Marshal.WriteInt32(detailData, 8);
                        if (SetupDiGetDeviceInterfaceDetail(devInfo, ref interfaceData, detailData, requiredSize, ref requiredSize, IntPtr.Zero))
                        {
                            string devicePath = Marshal.PtrToStringAuto(detailData + 4);
                            if (devicePath is null)
                                continue;

                            if (requiredInterface.HasValue)
                            {
                                string miPattern = $"&mi_{requiredInterface.Value:X2}";
                                if (!devicePath.Contains(miPattern, StringComparison.OrdinalIgnoreCase))
                                    continue;
                            }

                            IntPtr handle = CreateFile(devicePath, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                            if (handle == INVALID_HANDLE_VALUE)
                                continue;

                            try
                            {
                                HIDD_ATTRIBUTES attrs = new() { Size = Marshal.SizeOf(typeof(HIDD_ATTRIBUTES)) };
                                if (HidD_GetAttributes(handle, ref attrs) && attrs.VendorID == vendorId)
                                {
                                    App.Logger.WriteLine("DpiApplier", $"Found HID device VID=0x{vendorId:X4} PID=0x{attrs.ProductID:X4} interface={requiredInterface}");
                                    return devicePath;
                                }
                            }
                            finally
                            {
                                CloseHandle(handle);
                            }
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(detailData);
                    }
                }
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(devInfo);
            }

            return null;
        }

        #endregion

        #region Windows Fallback

        private static int DpiToMouseSpeed(int dpi)
        {
            double normalized = (double)(dpi - 50) / (20000 - 50);
            int speed = (int)(normalized * (MOUSE_SPEED_MAX - MOUSE_SPEED_MIN) + MOUSE_SPEED_MIN);
            return Math.Clamp(speed, MOUSE_SPEED_MIN, MOUSE_SPEED_MAX);
        }

        public static int GetCurrentSpeed()
        {
            int speed = 10;
            SystemParametersInfo(SPI_GETMOUSESPEED, 0, ref speed, 0);
            return speed;
        }

        private static bool ApplyWindowsDpi(int dpi)
        {
            int speed = DpiToMouseSpeed(dpi);
            bool result = SystemParametersInfo(SPI_SETMOUSESPEED, speed, ref speed, 0);
            App.Logger.WriteLine("DpiApplier", $"Windows mouse speed set to {speed} (DPI={dpi})");
            return result;
        }

        #endregion

        #region Brand Dispatch

        public static void Apply(int dpi, MouseBrand brand = MouseBrand.Generic)
        {
            try
            {
                bool success = false;

                switch (brand)
                {
                    case MouseBrand.AttackShark:
                        success = ApplyAttackSharkDpi(dpi);
                        break;

                    case MouseBrand.Logitech:
                        success = ApplyHidMouseDpi(dpi, 0x046D, "Logitech");
                        break;

                    case MouseBrand.Razer:
                    case MouseBrand.Viper:
                        success = ApplyRazerDpi(dpi);
                        break;

                    case MouseBrand.SteelSeries:
                        success = ApplyHidMouseDpi(dpi, 0x1038, "SteelSeries");
                        break;

                    case MouseBrand.Corsair:
                        success = ApplyHidMouseDpi(dpi, 0x1B1C, "Corsair");
                        break;

                    case MouseBrand.HyperX:
                        success = ApplyHidMouseDpi(dpi, 0x0951, "HyperX");
                        break;

                    case MouseBrand.Zowie:
                    case MouseBrand.Vaxee:
                    case MouseBrand.Xtrfy:
                        App.Logger.WriteLine("DpiApplier", $"{brand}: hardware DPI buttons only, no software control");
                        break;

                    case MouseBrand.Glorious:
                    case MouseBrand.FinalMouse:
                    case MouseBrand.Pulsar:
                    case MouseBrand.Lamzu:
                    case MouseBrand.EndgameGear:
                        App.Logger.WriteLine("DpiApplier", $"{brand}: no known HID protocol, using Windows fallback");
                        break;

                    case MouseBrand.Roccat:
                    case MouseBrand.Roccat2:
                    case MouseBrand.AsusRog:
                    case MouseBrand.CoolerMaster:
                    case MouseBrand.Msi:
                    case MouseBrand.Gigabyte:
                    case MouseBrand.Dareu:
                        App.Logger.WriteLine("DpiApplier", $"{brand}: requires companion software, using Windows fallback");
                        break;

                    default:
                        success = ApplyWindowsDpi(dpi);
                        break;
                }

                if (success)
                {
                    App.Logger.WriteLine("DpiApplier", $"Applied DPI {dpi} (brand: {brand})");
                }
                else
                {
                    App.Logger.WriteLine("DpiApplier", $"Brand-specific DPI failed for {brand}, falling back to Windows mouse speed");
                    ApplyWindowsDpi(dpi);
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("DpiApplier", $"Exception while applying DPI: {ex.Message}");
                try { ApplyWindowsDpi(dpi); } catch { }
            }
        }

        private static bool ApplyHidMouseDpi(int dpi, ushort vendorId, string brandName)
        {
            try
            {
                string? devicePath = FindHidDeviceByVid(vendorId, null);
                if (devicePath is null)
                {
                    App.Logger.WriteLine("DpiApplier", $"{brandName}: device not found via HID");
                    return false;
                }

                App.Logger.WriteLine("DpiApplier", $"{brandName}: device found but no HID DPI protocol implemented yet");
                return false;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("DpiApplier", $"{brandName} HID lookup error: {ex.Message}");
                return false;
            }
        }

        private static readonly HttpClient _razerHttpClient = new() { Timeout = TimeSpan.FromSeconds(2) };

        private static bool ApplyRazerDpi(int dpi)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:54231/razer/synapse/basemouse/dpi");
                request.Content = new StringContent(
                    $"{{\"dpiX\":{dpi},\"dpiY\":{dpi}}}",
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = _razerHttpClient.SendAsync(request).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                    return true;

                App.Logger.WriteLine("DpiApplier", "Razer Synapse REST returned non-success, trying HID");
            }
            catch
            {
                App.Logger.WriteLine("DpiApplier", "Razer Synapse not detected");
            }

            return ApplyHidMouseDpi(dpi, 0x1532, "Razer");
        }

        #endregion

        #region Public API

        public static void ApplyInGameDpi(int? dpiValue, MouseBrand brand = MouseBrand.Generic)
        {
            if (dpiValue.HasValue)
                Apply(dpiValue.Value, brand);
        }

        public static void RestoreOriginal(MouseBrand brand = MouseBrand.Generic)
        {
            int restoreDpi = App.Settings.Prop.DpiValue ?? 800;
            Apply(restoreDpi, brand);
        }

        #endregion
    }
}
