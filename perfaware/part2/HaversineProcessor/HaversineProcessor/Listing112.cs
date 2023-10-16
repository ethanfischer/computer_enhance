public class Listing112
{
    const int PROT_READ = 0x1;
    const int PROT_WRITE = 0x2;
    const int MAP_ANON = 0x1000;
    const int MAP_PRIVATE = 0x02;

    public static void Main(string[] args)
    {
        //InitializeOSMetrics();

        // if (args.Length == 2)
        // {
        // ulong pageSize = 4096;// NOTE(casey): This may not be the OS page size! It is merely our testing page size.
        ulong pageSize = 16384;
        ulong pageCount = 4096;
        IntPtr totalSize = (IntPtr)(pageSize * pageCount);

        Console.WriteLine("Page Count, Touch Count, Fault Count, Extra Faults\n");

        for (ulong touchCount = 0; touchCount <= pageCount; ++touchCount)
        {
            ulong touchSize = pageSize * touchCount;
            // byte* data = (byte*)VirtualAlloc(0, totalSize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
            IntPtr data = UnixOperations.mmap(0, totalSize, PROT_READ | PROT_WRITE, MAP_ANON | MAP_PRIVATE, -1, 0);
            if (data != IntPtr.Zero)
            {
                var startFaultCount = UnixOperations.GetMemFaults();
                unsafe
                {
                    byte* dataPtr = (byte*)data;
                    for (int index = 0; index < Convert.ToInt32(touchSize); ++index)
                    {
                        dataPtr[index] = (byte)index;
                    }
                }
                var endFaultCount = UnixOperations.GetMemFaults();

                var faultCount = endFaultCount - startFaultCount;

                Console.WriteLine("{0}, {1}, {2}, {3}", pageCount, touchCount, faultCount, (faultCount - touchCount));

                // VirtualFree(data, 0, MEM_RELEASE);
                UnixOperations.munmap(data, totalSize);
            }
            else
            {
                Console.Error.WriteLine("ERROR: Unable to allocate memory");
            }
        }
        // }
        // else
        // {
        //     Console.Error.WriteLine($"Usage: {args[0]} [# of 4k pages to allocate]");
        // }
    }
}