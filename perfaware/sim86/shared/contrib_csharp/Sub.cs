using static Sim86;
using static InstructionUtils;

public static class Sub
{
    public static ArithmeticFlags Handle(this Instruction decoded, int[] registers, ArithmeticFlags _arithmeticFlags)
    {
        if (decoded.Operands[0] is RegisterAccess destReg)
        {
            var destRegisterName = Sim86.RegisterNameFromOperand(destReg);
            var destRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), destRegisterName);

            if (decoded.Operands[1] is Immediate imm)
            {
                return ImmediateToRegister(decoded, destRegisterName, destRegisterId, registers, imm, _arithmeticFlags);
            }
            else if (decoded.Operands[1] is RegisterAccess sourceReg)
            {
                return RegisterToRegister(decoded, destRegisterName, destRegisterId, registers, sourceReg, _arithmeticFlags);
            }
        }

        throw new Exception();
    }

    private static ArithmeticFlags RegisterToRegister(Instruction decoded, string destRegisterName, RegisterId destRegisterId, int[] registers, RegisterAccess sourceReg, ArithmeticFlags arithmeticFlags)
    {
        var sourceRegisterName = Sim86.RegisterNameFromOperand(sourceReg);
        var sourceRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), sourceRegisterName);
        var sourceRegister = registers[(int)sourceRegisterId];
        var destRegister = registers[(int)destRegisterId];
        var result = destRegister - sourceRegister;
        registers[(int)destRegisterId] = result;
        var updatedArithmeticFlags = GetUpdatedArithmeticFlags(arithmeticFlags, result);
        var arithmeticFlagUpdateText = GetArithmeticFlagUpdateText(arithmeticFlags, updatedArithmeticFlags);
        Console.WriteLine($"{decoded.Op} {destRegisterName}, {sourceRegisterName} ; {destRegisterName}:0x{destRegister.ToString("x")}->0x{result.ToString("x")} {arithmeticFlagUpdateText}");
        return updatedArithmeticFlags;
    }

    public static ArithmeticFlags ImmediateToRegister(Instruction decoded, string destRegisterName, RegisterId destRegisterId, int[] registers, Immediate imm, ArithmeticFlags arithmeticFlags)
    {
        var destRegister = registers[(int)destRegisterId];
        var result = destRegister - imm.Value;
        registers[(int)destRegisterId] = result;

        var updatedArithmeticFlags = GetUpdatedArithmeticFlags(arithmeticFlags, result);
        var arithmeticFlagUpdateText = GetArithmeticFlagUpdateText(arithmeticFlags, updatedArithmeticFlags);

        Console.WriteLine($"{decoded.Op} {destRegisterName}, {imm.Value} ; {destRegisterName}:0x{destRegister.ToString("x")}->0x{result.ToString("x")} {arithmeticFlagUpdateText}");

        return updatedArithmeticFlags;
    }
}