using static Sim86;

public static class Sub
{
    public static void Handle(this Instruction decoded, int[] registers, ArithmeticFlags _arithmeticFlags)
    {
        if (decoded.Operands[0] is RegisterAccess destReg)
        {
            var destRegisterName = Sim86.RegisterNameFromOperand(destReg);
            var destRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), destRegisterName);

            if (decoded.Operands[1] is Immediate imm)
            {
                ImmediateToRegister(decoded, destRegisterName, destRegisterId, registers, imm, _arithmeticFlags);
            }
            else if (decoded.Operands[1] is RegisterAccess sourceReg)
            {
                RegisterToRegister(decoded, destRegisterName, destRegisterId, registers, sourceReg, _arithmeticFlags);
            }
        }
    }

    private static void RegisterToRegister(Instruction decoded, string destRegisterName, RegisterId destRegisterId, int[] registers, RegisterAccess sourceReg, ArithmeticFlags arithmeticFlags)
    {
        var sourceRegisterName = Sim86.RegisterNameFromOperand(sourceReg);
        var sourceRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), sourceRegisterName);
        var sourceRegister = registers[(int)sourceRegisterId];
        var destRegister = registers[(int)destRegisterId];
        var result = destRegister - sourceRegister;
        registers[(int)destRegisterId] = sourceRegister;
        var updatedArithmeticFlags = GetUpdatedArithmeticFlags(arithmeticFlags, result);
        var arithmeticFlagUpdateText = GetArithmeticFlagUpdateText(arithmeticFlags, updatedArithmeticFlags);
        Console.WriteLine($"{decoded.Op} {destRegisterName}, {sourceRegisterName} ; {destRegisterName}:0x{destRegister.ToString("x")}->0x{result.ToString("x")} {arithmeticFlagUpdateText}");
    }

    public static void ImmediateToRegister(Instruction decoded, string destRegisterName, RegisterId destRegisterId, int[] registers, Immediate imm, ArithmeticFlags arithmeticFlags)
    {
        var destRegister = registers[(int)destRegisterId];
        var result = destRegister - imm.Value;
        registers[(int)destRegisterId] = result;

        var updatedArithmeticFlags = GetUpdatedArithmeticFlags(arithmeticFlags, result);
        var arithmeticFlagUpdateText = GetArithmeticFlagUpdateText(arithmeticFlags, updatedArithmeticFlags);

        Console.WriteLine($"{decoded.Op} {destRegisterName}, {imm.Value} ; {destRegisterName}:0x{destRegister.ToString("x")}->0x{result.ToString("x")} {arithmeticFlagUpdateText}");
    }

    private static string GetArithmeticFlagUpdateText(ArithmeticFlags arithmeticFlags, ArithmeticFlags updatedArithmeticFlags)
    {
        var zeroFlagBefore = arithmeticFlags.HasFlag(ArithmeticFlags.Zero) ? "Z" : "";
        var signedFlagBefore = arithmeticFlags.HasFlag(ArithmeticFlags.Sign) ? "S" : "";
        var zerFlagAfter = updatedArithmeticFlags.HasFlag(ArithmeticFlags.Zero) ? "Z" : "";
        var signedFlagAfter = updatedArithmeticFlags.HasFlag(ArithmeticFlags.Sign) ? "S" : "";
        var beforeText = $"{zeroFlagBefore}{signedFlagBefore}";
        var afterText = $"{zerFlagAfter}{signedFlagAfter}";
        var arithmeticFlagUpdateText = (beforeText == afterText) ? "" : $"flags:{beforeText}->{afterText}";
        return arithmeticFlagUpdateText;
    }

    private static ArithmeticFlags GetUpdatedArithmeticFlags(ArithmeticFlags _arithmeticFlags, int result)
    {
        var flagsResult = new ArithmeticFlags();
        if (result == 0)
        {
            flagsResult = _arithmeticFlags | ArithmeticFlags.Zero;
        }
        else
        {
            flagsResult = _arithmeticFlags & ~ArithmeticFlags.Zero;
        }
        if ((result & 0x8000) != 0)
        {
            flagsResult = _arithmeticFlags | ArithmeticFlags.Sign;
        }
        else
        {
            flagsResult = _arithmeticFlags & ~ArithmeticFlags.Sign;
        }

        return flagsResult;
    }
}