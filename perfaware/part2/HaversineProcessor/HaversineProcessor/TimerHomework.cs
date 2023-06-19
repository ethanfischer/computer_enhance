using System.Diagnostics;
using System.Runtime.InteropServices;

namespace JsonGenerator;

public class TimerHomework
{
    [DllImport("kernel32.dll")]
    public static extern bool QueryPerformanceFrequency(out ulong frequency);

    [DllImport("kernel32.dll")]
    public static extern bool QueryPerformanceCounter(out ulong counter);

    public static void Main(string[] args)
    {
        Console.WriteLine("How many milliseconds to wait?");
        var input = Console.ReadLine();
        ulong MillisecondsToWait = 1000;
        if (input != string.Empty)
        {
            MillisecondsToWait = ulong.Parse(input);
        }

        var OSFreq = GetOSTimerFreq();
        Console.WriteLine("    OS Freq: {0}", OSFreq);

        var CPUStart = ReadCPUTimer();
        var OSStart = ReadOSTimer();
        ulong OSEnd = 0;
        ulong OSElapsed = 0;
        ulong CPUEnd = 0;
        ulong CPUElapsed = 0;
        ulong OSWaitTime = OSFreq * MillisecondsToWait / 1000;
        while (OSElapsed < OSWaitTime)
        {
            OSEnd = ReadOSTimer();
            OSElapsed = OSEnd - OSStart;
        }

        CPUEnd = ReadCPUTimer();
        CPUElapsed = CPUEnd - CPUStart;
        ulong CPUFreq = 0;
        if (OSElapsed > 0)
        {
            CPUFreq = OSFreq * CPUElapsed / OSElapsed;
        }

        Console.WriteLine("   OS Timer: {0} -> {1} = {2} elapsed", OSStart, OSEnd, OSElapsed);
        Console.WriteLine(" OS Seconds: {0}", (ulong)OSElapsed / (ulong)OSFreq);
        Console.WriteLine("   CPU Timer: {0} -> {1} = {2} elapsed", CPUStart, CPUEnd, CPUElapsed);
        Console.WriteLine("   CPU Freq: {0} (guessed)", CPUFreq);

        Main(null);
    }

    static ulong GetOSTimerFreq()
    {
        if (QueryPerformanceFrequency(out var frequency))
        {
            return frequency;
        }

        Console.WriteLine("ERROR: QueryPerformanceFrequency failed!");
        return 0;
    }

    static ulong ReadOSTimer()
    {
        if (QueryPerformanceCounter(out var counter))
        {
            return counter;
        }

        Console.WriteLine("ERROR: QueryPerformanceCounter failed!");
        return 0;
    }

    public static ulong ReadCPUTimer()
    {
        var result = rdtsc();
        return result;
    }

    [DllImport("Rdtsc.dll")]
    private static extern ulong rdtsc();
}