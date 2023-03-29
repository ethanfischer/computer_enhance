using System.IO;

var path = "perfaware/part1/listing_0041_add_sub_cmp_jnz";
var file_name = Path.GetFileNameWithoutExtension(path);
var fileBytes = File.ReadAllBytes(path);
using var writer = new StreamWriter($"{file_name}_decoded.asm");

foreach (var b in fileBytes)
{
    // writer.XWriteLine(b.Binary());
}

var i = 0;
while (i < fileBytes.Length)
{
    var instruction = DecodeInstruction(fileBytes[i]);

    switch (instruction)
    {
        case Instruction.Mov:
        case Instruction.Add:
        case Instruction.Sub:
        case Instruction.Cmp:
            HandleMovAddSubCmp(writer, fileBytes, ref i, instruction);
            break;
        case Instruction.ImmediateToRegister:
            HandleImmediateToRegister(writer, fileBytes, ref i, Instruction.Mov);
            break;
        case Instruction.AddSubCmp_ImmediateToRegister:
            HandleAddSubCmp_ImmediateToRegister(writer, fileBytes, ref i);
            break;
        case Instruction.None:
            i++;
            break;
    }
}

void HandleAddSubCmp_ImmediateToRegister(StreamWriter writer, byte[] fileBytes, ref int i)
{
    var magicOctal = (byte)((fileBytes[i + 1] >> 3) & 0b_111);
    switch (magicOctal)
    {
        case 0b_000:
            HandleImmediateToRegister(writer, fileBytes, ref i, Instruction.Add);
            break;
        case 0b_101:
            HandleImmediateToRegister(writer, fileBytes, ref i, Instruction.Sub);
            break;
        case 0b_111:
            HandleImmediateToRegister(writer, fileBytes, ref i, Instruction.Cmp);
            break;
        default:
            break;
    }
}

static Instruction DecodeInstruction(byte opcode)
{
    if ((opcode >> 4) == 0b_1011)
    {
        return Instruction.ImmediateToRegister;
    }
    return (opcode >> 2) switch
    {
        0b_100010 => Instruction.Mov,
        0b_000000 => Instruction.Add,
        0b_001010 => Instruction.Sub,
        0b_001110 => Instruction.Cmp,
        0b_100000 => Instruction.AddSubCmp_ImmediateToRegister,
        _ => Instruction.None
    };
}

static string DecodeRegisterField(byte register, byte w_flag, byte mod, byte[] fileBytes, int i)
{
    if (mod == 0b_00)
    {
        var rm_mod = (byte)(register << 2 | mod);
        return DecodeEffectiveAddressCalculation(rm_mod, fileBytes, i).decoded;
    }
    else
    {
        return ((register << 1) | w_flag) switch
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
    }
}

static (string decoded, int indexIncrementAmount) DecodeEffectiveAddressCalculation(byte rm_mod, byte[] fileBytes, int i) =>
(rm_mod) switch
{
    0b_000_00 => ("[bx + si]", 2),
    0b_001_00 => ("[bx + di]", 2),
    0b_010_00 => ("[bp + si]", 2),
    0b_011_00 => ("[bp + di]", 2),
    0b_100_00 => ("[si]", 2),
    0b_101_00 => ("[di]", 2),
    0b_110_00 => ("[bp]", 2),
    0b_111_00 => ("[bx]", 2),
    0b_000_01 => ($"[bx + si + {fileBytes[i + 2]}]", 3),
    0b_001_01 => ($"[bx + di + {fileBytes[i + 2]}]", 3),
    0b_010_01 => ($"[bp + si + {fileBytes[i + 2]}]", 3),
    0b_011_01 => ($"[bp + di + {fileBytes[i + 2]}]", 3),
    0b_100_01 => ($"[si + {fileBytes[i + 2]}]", 3),
    0b_101_01 => ($"[di + {fileBytes[i + 2]}]", 3),
    0b_110_01 => ($"[bp{(fileBytes[i + 2] == 0 ? "" : "+ {fileBytes[i+2].ToString()}")}]", 3),
    0b_111_01 => ($"[bx + {fileBytes[i + 2]}]", 3),
    0b_000_10 => ($"[bx + si + {BitConverter.ToInt16(new byte[2] { fileBytes[i + 2], fileBytes[i + 3] })}]", 4),
    0b_001_10 => ($"[bx + di + {BitConverter.ToInt16(new byte[2] { fileBytes[i + 2], fileBytes[i + 3] })}]", 4),
    0b_010_10 => ($"[bp + si + {BitConverter.ToInt16(new byte[2] { fileBytes[i + 2], fileBytes[i + 3] })}]", 4),
    0b_011_10 => ($"[bp + di + {BitConverter.ToInt16(new byte[2] { fileBytes[i + 2], fileBytes[i + 3] })}]", 4),
    0b_100_10 => ($"[si + {BitConverter.ToInt16(new byte[2] { fileBytes[i + 2], fileBytes[i + 3] })}]", 4),
    0b_101_10 => ($"[di + {BitConverter.ToInt16(new byte[2] { fileBytes[i + 2], fileBytes[i + 3] })}]", 4),
    0b_110_10 => ($"[bp + {BitConverter.ToInt16(new byte[2] { fileBytes[i + 2], fileBytes[i + 3] })}]", 4),
    0b_111_10 => ($"[bx + {BitConverter.ToInt16(new byte[2] { fileBytes[i + 2], fileBytes[i + 3] })}]", 4),
    _ => throw new Exception($"{Convert.ToString(rm_mod, 2).PadLeft(3, '0')} register not found"),
};

return 0;

static void HandleMovAddSubCmp(StreamWriter writer, byte[] fileBytes, ref int i, Instruction instruction)
{
    var d_flag = (byte)((fileBytes[i] >> 1) & 1);
    var w_flag = (byte)(fileBytes[i] & 1);
    var mod = (byte)((fileBytes[i + 1] >> 6) & 0b_11);
    var reg = (byte)((fileBytes[i + 1] >> 3) & 0b_111);
    var rm = (byte)(fileBytes[i + 1] & 0b_111);

    if (mod == 0b_11)
    {
        var (destination, source) = d_flag == 1 ? (reg, rm) : (rm, reg);
        writer.XWrite(instruction.ToString());
        writer.XWrite(" ");
        writer.XWrite(DecodeRegisterField(destination, w_flag, mod, fileBytes, i));
        writer.XWrite(", ");
        writer.XWrite(DecodeRegisterField(source, w_flag, mod, fileBytes, i));
        writer.XWriteLine("");
        i += 2;
    }
    else
    {
        writer.XWrite(instruction.ToString());
        writer.XWrite(" ");
        var rm_mod = (byte)(rm << 2 | mod);
        var effectiveAddressCalculation = DecodeEffectiveAddressCalculation(rm_mod, fileBytes, i);
        writer.XWrite(d_flag == 0 ? effectiveAddressCalculation.decoded : DecodeRegisterField(reg, w_flag, mod, fileBytes, i));
        writer.XWrite(", ");
        writer.XWrite(d_flag == 0 ? DecodeRegisterField(reg, w_flag, mod, fileBytes, i) : effectiveAddressCalculation.decoded);
        writer.XWriteLine("");
        i += effectiveAddressCalculation.indexIncrementAmount;
    }
}

static void HandleImmediateToRegister(StreamWriter writer, byte[] fileBytes, ref int i, Instruction instruction)
{
    var w_flag = (byte)(fileBytes[i] & 1);
    var mod = (byte)((fileBytes[i + 1] >> 6) & 0b_11);
    var reg = (byte)(fileBytes[i + 1] & 0b_111);
    var thirdByte = fileBytes[i + 2];
    var data = BitConverter.ToInt16(new byte[2] { thirdByte, 0 });
    if (mod == 0b_10)
    {
        var fourthByte = fileBytes.TryGetByteAtIndex(i + 3);
        var thirdAndFourthByte = new byte[2] { thirdByte, fourthByte };
        data = BitConverter.ToInt16(thirdAndFourthByte, 0);
        i += 4;
    }
    else
    {
        i += 3;
    }

    writer.XWrite(instruction.ToString());
    writer.XWrite(" ");
    writer.XWrite(DecodeRegisterField(reg, w_flag, mod, fileBytes, i));
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

    public static byte TryGetByteAtIndex(this byte[] source, int i)
    {
        if (i < source.Length)
        {
            return source[i];
        }
        else
        {
            return 0;
        }
    }
}

public enum Instruction
{
    Mov,
    ImmediateToRegister,
    Add,
    Sub,
    Cmp,
    AddSubCmp_ImmediateToRegister,
    None
}