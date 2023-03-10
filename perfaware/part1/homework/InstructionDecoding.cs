using System.IO;
var path = "perfaware/part1/listing_0039_more_movs";

// var path = "perfaware/part1/listing_0039_more_movs";
var file_name = Path.GetFileNameWithoutExtension(path);

using var fileStream = File.Open(path, FileMode.Open);
using var reader = new BinaryReader(fileStream);
using var writer = new StreamWriter($"{file_name}_decoded.asm");
var buffer = new byte[2];

while (reader.Read(buffer) != 0)
{
    //mov cl, 12
    var instruction = DecodeInstruction(buffer[0]);

    switch (instruction)
    {
        case Instruction.Mov:
            HandleMov(writer, buffer);
            break;
        case Instruction.Immediate_to_register:
            HandleImmediateToRegister(reader, writer, buffer);
            break;
    }
}

static Instruction DecodeInstruction(byte opcode)
{
    if ((opcode >> 2) == 0b_100010)
    {
        return Instruction.Mov;
    }
    else if ((opcode >> 4) == 0b_1011)
    {
        return Instruction.Immediate_to_register;
    }
    else
    {
        return Instruction.Unknown;
    }
    // return opcode switch
    // {
    //     //Add new opcode here
    //     0b_100010 => Instruction.Mov,
    //     0b_1011 => Instruction.Immediate_to_register,
    //     _ => throw new Exception($"{Convert.ToString(opcode, 2).PadLeft(6, '0')} opcode not found"),
    // };
}

static string DecodeRegisterField(byte register, byte w_flag) =>
    ((register << 1) | w_flag) switch
    {
        0b_000_0 => "al",
        0b_001_0 => "cl",
        0b_010_0 => "dl",
        0b_011_0 => "bl",
        0b_100_0 => "ah",
        0b_101_0 => "ch",
        0b_110_0 => "dh",
        0b_111_0 => "bh",
        0b_000_1 => "ax",
        0b_001_1 => "cx",
        0b_010_1 => "dx",
        0b_011_1 => "bx",
        0b_100_1 => "sp",
        0b_101_1 => "bp",
        0b_110_1 => "si",
        0b_111_1 => "di",
        _ => throw new Exception($"{Convert.ToString(register, 2).PadLeft(3, '0')} register not found"),
    };

return 0;

static void HandleMov(StreamWriter writer, byte[] buffer)
{
    var d_flag = (byte)((buffer[0] >> 1) & 1);
    var w_flag = (byte)(buffer[0] & 1);
    var mod = (byte)((buffer[1] >> 6) & 0b_11);
    var reg = (byte)((buffer[1] >> 3) & 0b_111);
    var rm = (byte)(buffer[1] & 0b_111);

    var (destination, source) = d_flag == 1 ? (reg, rm) : (rm, reg);

    writer.Write(DecodeInstruction(buffer[0]));
    writer.Write(' ');
    writer.Write(DecodeRegisterField(destination, w_flag));
    writer.Write(", ");
    writer.Write(DecodeRegisterField(source, w_flag));
    writer.WriteLine();
}

static void HandleImmediateToRegister(BinaryReader reader, StreamWriter writer, byte[] buffer)
{
    var w_flag = (byte)(buffer[0] >> 3 & 1);
    var reg = (byte)(buffer[0] & 0b_111);
    var data = buffer[1];
    if (w_flag == 1)
    {
        var thirdByte = (byte)reader.ReadByte();
        data += thirdByte;
    }

    writer.XWrite("mov");
    writer.XWrite(" ");
    writer.XWrite(DecodeRegisterField(reg, w_flag));
    writer.XWrite(", ");
    writer.XWrite(data.ToString());
    writer.XWriteLine("");
}


public static class Extensions
{
    public static string Binary(this byte input, int bitCount = 8)
    {
        return Convert.ToString(input, toBase: 2).PadLeft(bitCount, '0');
    }
public static void XWrite(this StreamWriter writer, string input)
{
    writer.Write(input);
    Console.Write(input);
}

public static void XWriteLine(this StreamWriter writer, string input)
{
    writer.WriteLine(input);
    Console.WriteLine(input);
}
}

public enum Instruction
{
    Mov,
    Immediate_to_register,
    Unknown
}