using System.Diagnostics;
using System.Runtime.InteropServices;

namespace JsonGenerator;

public static  class TimerService
{
    // public static void Main(string[] args)
    // {
    //     Console.WriteLine("How many milliseconds to wait?");
    //     var input = Console.ReadLine();
    //     long millisecondsToWait = 1000;
    //     if (input != string.Empty)
    //     {
    //         millisecondsToWait = long.Parse(input);
    //     }
    //
    //     EstimateCpuFrequency(millisecondsToWait);
    //
    //     Main(null);
    // }

    public static ulong ReadCPUTimer()
    {
        return (ulong)Stopwatch.GetTimestamp();
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