using System;
using System.Runtime.InteropServices;

public class MacPerformanceMetrics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TimeValue
    {
        public long Sec;
        public long Usec;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RUsage
    {
        public TimeValue ru_utime; /* user time used */
        public TimeValue ru_stime; /* system time used */
        public long ru_maxrss; /* max resident set size */
        public long ru_ixrss; /* integral shared text memory size */
        public long ru_idrss; /* integral unshared data size */
        public long ru_isrss; /* integral unshared stack size */
        public long ru_minflt; /* page reclaims */
        public long ru_majflt; /* page faults */
        public long ru_nswap; /* swaps */
        public long ru_inblock; /* block input operations */
        public long ru_oublock; /* block output operations */
        public long ru_msgsnd; /* messages sent */
        public long ru_msgrcv; /* messages received */
        public long ru_nsignals; /* signals received */
        public long ru_nvcsw; /* voluntary context switches */
        public long ru_nivcsw; /* involuntary context switches */
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