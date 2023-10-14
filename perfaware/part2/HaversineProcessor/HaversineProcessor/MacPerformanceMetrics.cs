using System;
using System.Runtime.InteropServices;

public class MacPerformanceMetrics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TimeValue
    {
        public int Sec;
        public int Usec;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RUsage
    {
        public TimeValue Utime;
        public TimeValue Stime;
        public long Maxrss;
        public long Ixrss;
        public long Idrss;
        public long Isrss;
        public long Minflt; // Page reclaims
        public long Majflt; // Page faults
        // ... (other fields)
    }

    [DllImport("libc", SetLastError = true)]
    public static extern int getrusage(int who, ref RUsage usage);

    public static RUsage GetRUsage()
    {
        var usage = new RUsage();
        getrusage(RUSAGE_SELF, ref usage);
        return usage;
    }

    const int RUSAGE_SELF = 0;
}