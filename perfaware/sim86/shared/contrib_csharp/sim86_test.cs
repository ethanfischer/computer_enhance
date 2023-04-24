// place "sim86_shared_debug.dll" next to all of these three files (sim86.cs, sim86_test.cs, sim86.csproj)
// then run "dotnet.exe run"

internal class Program
{
    private static int[] _registers = new int[8];
    private static void Main(string[] args)
    {
        var exampleDisassembly = File.ReadAllBytes("/home/ethan/repos/computer_enhance/perfaware/part1/listing_0043_immediate_movs");
        Console.WriteLine($"Sim86 Version: {Sim86.GetVersion()}");

        var table = Sim86.Get8086InstructionTable();
        Console.WriteLine($"8086 Instruction Instruction Encoding Count: {table.Encodings.Length}");

        var offset = 0;
        while (offset < exampleDisassembly.Length)
        {
            var decoded = Sim86.Decode8086Instruction(exampleDisassembly.AsSpan().Slice(offset));
            offset += decoded.Size;
            if (decoded.Op == Sim86.OperationType.mov)
            {
                decoded.Handle(_registers);
            }
            else
            {
                Console.WriteLine("unrecognized instruction");
                break;
            }
        }

        for (var i = 0; i < _registers.Length; i++)
        {
            Console.WriteLine($"{(RegisterId)i}: {_registers[i]}");
        }
    }

}

public enum RegisterId
{
    ax,
    bx,
    cx,
    dx,
    sp,
    bp,
    si,
    di
}