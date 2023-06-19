using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

public static class Rdtsc
{
    [SuppressUnmanagedCodeSecurity]
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate ulong FuncUInt64();

    private const uint PAGE_READWRITE = 0x04;
    private const uint PAGE_EXECUTE = 0x10;
    private const uint PAGE_EXECUTE_READWRITE = 0x40;
    private const uint MEM_COMMIT = 0x1000;
    private const uint MEM_RELEASE = 0x8000;
    public static readonly FuncUInt64 Timestamp;
    public static readonly FuncUInt64 TimestampP;
    public static readonly bool IsRdtscSupported;
    public static readonly bool IsRdtscPSupported;

    static Rdtsc()
    {
        SystemInfo systemInfo;
        GetNativeSystemInfo(out systemInfo);
        if (systemInfo.wProcessorArchitecture != 0 && systemInfo.wProcessorArchitecture != 9)
        {
            Timestamp = StopwatchGetTimestamp;
            TimestampP = StopwatchGetTimestamp;
            IsRdtscSupported = false;
            IsRdtscPSupported = false;
            return;
        }

        byte[] cpuid;
        byte[] rdtsc;
        byte[] rdtscp;
        byte[] rdtsccpuid;
        IsRdtscSupported = true;
        if (Is64Bit())
        {
            cpuid = new byte[]
            {
                0x53, 0xB8, 0x00, 0x00, 0x00, 0x80, 0x0F, 0xA2, 0xBB, 0x01, 0x00, 0x00, 0x80, 0x39, 0xD8, 0x72, 0x16, 0x89, 0xD8,
                0x48, 0xC7, 0xC2, 0xFF, 0xFF, 0xFF, 0xFF, 0x0F, 0xA2, 0x89, 0xC8, 0x48, 0xC1, 0xE0, 0x20, 0x48, 0x09, 0xD0, 0xEB,
                0x03, 0x48, 0x31, 0xC0, 0x5B, 0xC3
            };
            rdtsc = new byte[] { 0x0F, 0x31, 0x48, 0xC1, 0xE2, 0x20, 0x48, 0x09, 0xD0, 0xC3 };
            rdtscp = new byte[] { 0x0F, 0x01, 0xF9, 0x48, 0xC1, 0xE2, 0x20, 0x48, 0x09, 0xD0, 0xC3 };
            rdtsccpuid = new byte[]
                { 0x53, 0x31, 0xC0, 0x0F, 0xA2, 0x0F, 0x31, 0x48, 0xC1, 0xE2, 0x20, 0x48, 0x09, 0xD0, 0x5B, 0xC3 };
        }
        else
        {
            cpuid = new byte[]
            {
                0x53, 0xB8, 0x00, 0x00, 0x00, 0x80, 0x0F, 0xA2, 0xBB, 0x01, 0x00, 0x00, 0x80, 0x39, 0xD8, 0x72, 0x0A, 0x89, 0xD8,
                0x0F, 0xA2, 0x89, 0xD0, 0x89, 0xCA, 0xEB, 0x04, 0x31, 0xC0, 0x31, 0xD2, 0x5B, 0xC3
            };
            rdtsc = new byte[] { 0x0F, 0x31, 0xC3 };
            rdtscp = new byte[] { 0x0F, 0x01, 0xF9, 0xC3 };
            rdtsccpuid = new byte[] { 0x53, 0x31, 0xC0, 0x0F, 0xA2, 0x0F, 0x31, 0x5B, 0xC3 };
        }

        var buf = IntPtr.Zero;
        try
        {
            var cpuidLength = (cpuid.Length & 63) != 0 ? (cpuid.Length | 63) + 1 : cpuid.Length;
            var rdtscLength = (rdtsc.Length & 63) != 0 ? (rdtsc.Length | 63) + 1 : rdtsc.Length;
            var rdtscpLength = (rdtscp.Length & 63) != 0 ? (rdtscp.Length | 63) + 1 : rdtscp.Length;
            var rdtsccpuidLength = (rdtsccpuid.Length & 63) != 0 ? (rdtsccpuid.Length | 63) + 1 : rdtsccpuid.Length;
            var totalLength = cpuidLength + rdtscLength + Math.Max(rdtscpLength, rdtsccpuidLength);
            buf = VirtualAlloc(IntPtr.Zero, (IntPtr)totalLength, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            if (buf != IntPtr.Zero)
            {
                Marshal.Copy(cpuid, 0, buf, cpuid.Length);
                for (var i = cpuid.Length; i < cpuidLength; i++)
                    Marshal.WriteByte(buf, i, 0x90);
                Marshal.Copy(rdtsc, 0, buf + cpuidLength, rdtsc.Length);
                for (var i = rdtsc.Length; i < rdtscLength; i++)
                    Marshal.WriteByte(buf, cpuidLength + i, 0x90);
                var cpuidFunc = (FuncUInt64)Marshal.GetDelegateForFunctionPointer(buf, typeof(FuncUInt64));
                var supportedFeatures = cpuidFunc();
                byte[] rdtscpSelected;
                int rdtscpSelectedLength;
                if ((supportedFeatures & (1L << 27)) != 0)
                {
                    rdtscpSelected = rdtscp;
                    rdtscpSelectedLength = rdtscpLength;
                    IsRdtscPSupported = true;
                }
                else
                {
                    rdtscpSelected = rdtsccpuid;
                    rdtscpSelectedLength = rdtsccpuidLength;
                    IsRdtscPSupported = false;
                }

                Marshal.Copy(rdtscpSelected, 0, buf + cpuidLength + rdtscLength, rdtscpSelected.Length);
                for (var i = rdtscpSelected.Length; i < rdtscpSelectedLength; i++)
                    Marshal.WriteByte(buf, cpuidLength + rdtscLength + i, 0x90);
                var result = VirtualProtect(buf, (IntPtr)totalLength, PAGE_EXECUTE, out var oldProtection);
                if (result)
                {
                    Timestamp = (FuncUInt64)Marshal.GetDelegateForFunctionPointer(buf + cpuidLength, typeof(FuncUInt64));
                    TimestampP = (FuncUInt64)Marshal.GetDelegateForFunctionPointer(buf + cpuidLength + rdtscLength,
                        typeof(FuncUInt64));
                    buf = IntPtr.Zero;
                }
            }
        }
        finally
        {
            if (buf != IntPtr.Zero)
                VirtualFree(buf, IntPtr.Zero, MEM_RELEASE);
        }
    }

    private static bool Is64Bit()
    {
        return Marshal.SizeOf(typeof(IntPtr)) == 8;
    }

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern void GetNativeSystemInfo(out SystemInfo lpSystemInfo);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr VirtualAlloc(IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool VirtualProtect(IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, out uint lpflOldProtect);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool VirtualFree(IntPtr lpAddress, IntPtr dwSize, uint dwFreeType);

    private static ulong StopwatchGetTimestamp()
    {
        return unchecked((ulong)Stopwatch.GetTimestamp());
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SystemInfo
    {
        public readonly ushort wProcessorArchitecture;
        public readonly ushort wReserved;
        public readonly uint dwPageSize;
        public readonly IntPtr lpMinimumApplicationAddress;
        public readonly IntPtr lpMaximumApplicationAddress;
        public readonly IntPtr dwActiveProcessorMask;
        public readonly uint dwNumberOfProcessors;
        public readonly uint dwProcessorType;
        public readonly uint dwAllocationGranularity;
        public readonly ushort wProcessorLevel;
        public readonly ushort wProcessorRevision;
    }
}