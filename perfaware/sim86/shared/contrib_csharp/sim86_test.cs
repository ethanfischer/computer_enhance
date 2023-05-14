// place "sim86_shared_debug.dll" next to all of these three files (sim86.cs, sim86_test.cs, sim86.csproj)
// then run "dotnet.exe run"

using sim86;

internal class Program
{
    private static byte[] _memory = new byte[1_048_576]; //1MB
    private static int[] _registers = new int[9];
    private static ArithmeticFlags _arithmeticFlags;
    private static void Main(string[] args)
    {
        var exampleDisassembly = File.ReadAllBytes("/Users/ethanfischer/Repos/computer_enhance/perfaware/part1/listing_0052_memory_add_loop");
        Console.WriteLine($"Sim86 Version: {Sim86.GetVersion()}");

        var table = Sim86.Get8086InstructionTable();
        Console.WriteLine($"8086 Instruction Instruction Encoding Count: {table.Encodings.Length}");

        _registers.SetIP(0);
        while (_registers[Sim86.IP] < exampleDisassembly.Length)
        {
            var decoded = Sim86.Decode8086Instruction(exampleDisassembly.AsSpan().Slice(_registers[Sim86.IP]));
            if (decoded.Op == Sim86.OperationType.mov)
            {
                Mov.Handle(decoded, _registers, _memory);
            }
            else if (decoded.Op == Sim86.OperationType.add)
            {
                _arithmeticFlags = Add.Handle(decoded, _registers, _arithmeticFlags);
            }
            else if (decoded.Op == Sim86.OperationType.sub)
            {
                _arithmeticFlags = Sub.Handle(decoded, _registers, _arithmeticFlags);
            }
            else if (decoded.Op == Sim86.OperationType.cmp)
            {
                _arithmeticFlags = Cmp.Handle(decoded, _registers, _arithmeticFlags);
            }
            else if (decoded.Op == Sim86.OperationType.jne)
            {
                JNE.Handle(decoded, _registers, _arithmeticFlags);
            }
            else
            {
                Console.WriteLine("unrecognized instruction");
                break;
            }
        }

        Console.WriteLine("");
        Console.WriteLine("Final registers:");
        for (var i = 0; i < _registers.Length; i++)
        {
            var value = _registers[i];
            if(value == 0) continue;

            Console.WriteLine($"\t{(RegisterId)i}: 0x{value.Hex()} ({value})");
        }
        Console.WriteLine("");
        Console.WriteLine($"flags: {InstructionUtils.GetZeroFlagText(_arithmeticFlags)}{InstructionUtils.GetSignedFlagText(_arithmeticFlags)}");
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
    di,
    InstructionPointer
}

[Flags]
public enum ArithmeticFlags
{
    None = 0,
    Zero = 1,
    Sign = 2,
    Carry = 4,
}