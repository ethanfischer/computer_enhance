internal class Program
{
    private static void Main(string[] args)
    {
        var bytes = File.ReadAllBytes("/home/ethan/repos/computer_enhance/perfaware/part1/listing_0038_many_register_mov");
        for(var i = 0; i < bytes.Length - 1; i+=2)
        {
            DecodeBytes(bytes[i], bytes[i+1]);
        }

        string GetOpCode(string firstByte)
        {
            if (firstByte.StartsWith("100010"))
            {
                return "mov";
            }
            else
            {
                return "unknown";
            }
        }

        string GetFirstRegister(byte firstByte, byte secondByte)
        {
            var firstRegisterCode = GetFirstRegisterCode(secondByte);

            if (IsW(firstByte))
            {
                switch (firstRegisterCode)
                {
                    case 0b_00_000_000:
                        return "ax";
                    case 0b_00_001_000:
                        return "cx";
                    case 0b_00_010_000:
                        return "dx";
                    case 0b_00_011_000:
                        return "bx";
                    case 0b_00_100_000:
                        return "sp";
                    case 0b_00_101_000:
                        return "bp";
                    case 0b_00_110_000:
                        return "si";
                    case 0b_00_111_000:
                        return "di";
                    default:
                        return "";
                }
            }
            else
            {
                switch (firstRegisterCode)
                {
                    case 0b_00_000_000:
                        return "al";
                    case 0b_00_001_000:
                        return "cl";
                    case 0b_00_010_000:
                        return "dl";
                    case 0b_00_011_000:
                        return "bl";
                    case 0b_00_100_000:
                        return "ah";
                    case 0b_00_101_000:
                        return "ch";
                    case 0b_00_110_000:
                        return "dh";
                    case 0b_00_111_000:
                        return "bh";
                    default:
                        return "";
                }
            }
        }


        string GetSecondRegister(byte firstByte, byte secondByte)
        {
            var secondRegisterCode = GetSecondRegisterCode(secondByte);

            if (IsW(firstByte))
            {
                switch (secondRegisterCode)
                {
                    case 0b_00_000_000:
                        return "ax";
                    case 0b_00_000_001:
                        return "cx";
                    case 0b_00_000_010:
                        return "dx";
                    case 0b_00_000_011:
                        return "bx";
                    case 0b_00_000_100:
                        return "sp";
                    case 0b_00_000_101:
                        return "bp";
                    case 0b_00_000_110:
                        return "si";
                    case 0b_00_000_111:
                        return "di";
                    default:
                        return "ERROR";
                }
            }
            else
            {
                switch (secondRegisterCode)
                {
                    case 0b_00_000_000:
                        return "al";
                    case 0b_00_000_001:
                        return "cl";
                    case 0b_00_000_010:
                        return "dl";
                    case 0b_00_000_011:
                        return "bl";
                    case 0b_00_000_100:
                        return "ah";
                    case 0b_00_000_101:
                        return "ch";
                    case 0b_00_000_110:
                        return "dh";
                    case 0b_00_000_111:
                        return "bh";
                    default:
                        return "ERROR";
                }
            }
        }

        void DecodeBytes(byte firstByte, byte secondByte)
        {
            var binary = Convert.ToString(firstByte, toBase: 2);
            var opCode = GetOpCode(binary);
            var isDBit = IsDBit(firstByte);

            var firstRegister = GetFirstRegister(firstByte, secondByte);
            var secondRegister = GetSecondRegister(firstByte, secondByte);

            if (isDBit)
            {
                Console.WriteLine($"{opCode} {firstRegister}, {secondRegister}");
            }
            else
            {
                Console.WriteLine($"{opCode} {secondRegister}, {firstRegister}");
            }
        }
    }

    private static bool IsDBit(byte firstByte)
    {
        var mask = (byte)0b_0000_0010;
        return (firstByte & mask) == 0b_0000_0010;
    }

    private static bool IsW(byte firstByte)
    {
        var mask = (byte)0b_0000_0001;
        var w = (byte)(firstByte & mask);
        var isW = w == 0b_0000_0001;
        return isW;
    }

    private static byte GetFirstRegisterCode(byte secondByte)
    {
        var mask = (byte)0b_00_111_000;
        var maskedRegister = (byte)(secondByte & mask);
        return maskedRegister;
    }

    private static byte GetSecondRegisterCode(byte secondByte)
    {
        var mask = (byte)0b_00_000_111;
        var maskedRegister = (byte)(secondByte & mask);
        return maskedRegister;
    }
}

public static class Extensions
{
    public static string Binary(this byte inputByte)
    {
        return $"{Convert.ToString(inputByte, toBase: 2).PadLeft(8, '0')}";
    }
}