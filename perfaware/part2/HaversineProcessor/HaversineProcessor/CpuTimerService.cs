using System.Diagnostics;
using System.Runtime.InteropServices;

namespace JsonGenerator;

public class CpuTimerService
{
    public static void Main(string[] args)
    {
        Console.WriteLine("How many milliseconds to wait?");
        var input = Console.ReadLine();
        long millisecondsToWait = 1000;
        if (input != string.Empty)
        {
            millisecondsToWait = long.Parse(input);
        }

        EstimateCpuFrequency(millisecondsToWait);

        Main(null);
    }

    public static long ReadCpuTimer()
    {
        initialize_mach_timebase_info();
        var result = get_time_in_ns();
        return (long)result;
    }

    [DllImport("libtimer.dylib", CallingConvention = CallingConvention.Cdecl)]
    public static extern void initialize_mach_timebase_info();

    [DllImport("libtimer.dylib", CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong get_time_in_ns();

    public static long EstimateCpuFrequency(long millisecondsToWait)
    {
        var osFreq = Stopwatch.Frequency;
        // Console.WriteLine("    OS Freq: {0}", osFreq);

        var cpuStart = ReadCpuTimer();
        var osStart = Stopwatch.GetTimestamp();
        var osEnd = 0L;
        var osElapsed = 0L;
        var osWaitTime = osFreq * millisecondsToWait / 1000;
        while (osElapsed < osWaitTime)
        {
            osEnd = Stopwatch.GetTimestamp();
            osElapsed = osEnd - osStart;
        }

        var cpuEnd = ReadCpuTimer();
        var cpuElapsed = cpuEnd - cpuStart;
        var cpuFreq = 0L;
        if (osElapsed > 0)
        {
            cpuFreq = osFreq * cpuElapsed / osElapsed;
        }

        Console.WriteLine("   OS Timer: {0} -> {1} = {2} elapsed", osStart, osEnd, osElapsed);
        Console.WriteLine(" OS Seconds: {0}", osElapsed / osFreq);
        Console.WriteLine("   CPU Timer: {0} -> {1} = {2} elapsed", cpuStart, cpuEnd, cpuElapsed);
        Console.WriteLine("   CPU Freq: {0} (guessed)", cpuFreq);

        return cpuFreq;
    }
}