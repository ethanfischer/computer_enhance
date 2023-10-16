using System;
using System.Runtime.InteropServices;

public class UnixOperations
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
    static extern int getrusage(int who, ref RUsage usage);

    static RUsage GetRUsage()
    {
        var usage = new RUsage();
        getrusage(RUSAGE_SELF, ref usage);
        return usage;
    }

    public static ulong GetMemFaults()
    {
        var rUsage = GetRUsage();
        return (ulong)(rUsage.ru_minflt + rUsage.ru_majflt);
    }

    const int RUSAGE_SELF = 0;


    const int PROT_READ = 0x1;
    const int PROT_WRITE = 0x2;
    const int MAP_SHARED = 0x01;
    const int MAP_PRIVATE = 0x02;

    [DllImport("libc", SetLastError = true)]
    public static extern IntPtr mmap(IntPtr addr, IntPtr length, int prot, int flags, int fd, IntPtr offset);
    
    [DllImport("libc", SetLastError = true)]
    public static extern int munmap(IntPtr addr, IntPtr length);

    // public static void Main() {
    //     IntPtr size = (IntPtr)(4 * 1024); // 4K pages
    //     IntPtr result = mmap(IntPtr.Zero, size, PROT_READ | PROT_WRITE, MAP_SHARED, -1, IntPtr.Zero);
    //
    //     if (result.ToInt64() == -1L) {
    //         Console.Error.WriteLine("Error: mmap failed");
    //         return;
    //     }
    //
    //     // Your code here
    //
    //     // Don't forget to unmap when you're done
    // }
}