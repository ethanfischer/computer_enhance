using System.Diagnostics;
using System.Runtime.InteropServices;

namespace JsonGenerator;

public static  class TimerService
{
    public static ulong ReadCPUTimer()
    {
        initialize_mach_timebase_info();
        var result = get_time_in_ns();
        return result;
    }

    [DllImport("libtimer.dylib", CallingConvention = CallingConvention.Cdecl)]
    public static extern void initialize_mach_timebase_info();

    [DllImport("libtimer.dylib", CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong get_time_in_ns();

    public static ulong EstimateCPUTimerFreq(ulong millisecondsToWait = 1000)
    {
        var osFreq = (ulong)Stopwatch.Frequency;

        var cpuStart = ReadCPUTimer();
        var osStart = (ulong)Stopwatch.GetTimestamp();
        var osEnd = 0UL;
        var osElapsed = 0UL;
        var osWaitTime = osFreq * millisecondsToWait / 1000;
        while (osElapsed < osWaitTime)
        {
            osEnd = (ulong)Stopwatch.GetTimestamp();
            osElapsed = osEnd - osStart;
        }

        var cpuEnd = ReadCPUTimer();
        var cpuElapsed = cpuEnd - cpuStart;
        var cpuFreq = 0UL;
        if (osElapsed > 0)
        {
            cpuFreq = osFreq * cpuElapsed / osElapsed;
        }

        return cpuFreq;
    }
}