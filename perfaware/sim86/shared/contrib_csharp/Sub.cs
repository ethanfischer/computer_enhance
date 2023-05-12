using static Sim86;
using static InstructionUtils;

public static class Sub
{
    public static OperationFlags Handle(this Instruction decoded, int[] registers, OperationFlags operationFlags)
    {
        if (decoded.Operands[0] is RegisterAccess destReg)
        {
            var destRegisterName = Sim86.RegisterNameFromOperand(destReg);
            var destRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), destRegisterName);

            if (decoded.Operands[1] is Immediate imm)
            {
                return ImmediateToRegister(decoded, destRegisterName, destRegisterId, registers, imm, operationFlags);
            }
            else if (decoded.Operands[1] is RegisterAccess sourceReg)
            {
                return RegisterToRegister(decoded, destRegisterName, destRegisterId, registers, sourceReg, operationFlags);
            }
        }

        throw new Exception();
    }

    private static OperationFlags RegisterToRegister(Instruction decoded, string destRegisterName, RegisterId destRegisterId, int[] registers, RegisterAccess sourceReg, OperationFlags operationFlags)
    {
        var sourceRegisterName = Sim86.RegisterNameFromOperand(sourceReg);
        var sourceRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), sourceRegisterName);
        var sourceRegister = registers[(int)sourceRegisterId];
        var destRegister = registers[(int)destRegisterId];
        var result = destRegister - sourceRegister;
        registers[(int)destRegisterId] = result;
        var updatedArithmeticFlags = GetUpdatedArithmeticFlags(operationFlags, result);
        var arithmeticFlagUpdateText = GetArithmeticFlagUpdateText(operationFlags, updatedArithmeticFlags);
        Console.WriteLine($"{decoded.Op} {destRegisterName}, {sourceRegisterName} ; {destRegisterName}:0x{destRegister.ToString("x")}->0x{result.ToString("x")} {arithmeticFlagUpdateText}");
        return updatedArithmeticFlags;
    }

    public static OperationFlags ImmediateToRegister(Instruction decoded, string destRegisterName, RegisterId destRegisterId, int[] registers, Immediate imm, OperationFlags operationFlags)
    {
        var destRegister = registers[(int)destRegisterId];
        var result = destRegister - imm.Value;
        registers[(int)destRegisterId] = result;

        var updatedArithmeticFlags = GetUpdatedArithmeticFlags(operationFlags, result);
        var arithmeticFlagUpdateText = GetArithmeticFlagUpdateText(operationFlags, updatedArithmeticFlags);

        Console.WriteLine($"{decoded.Op} {destRegisterName}, {imm.Value} ; {destRegisterName}:0x{destRegister.ToString("x")}->0x{result.ToString("x")} {arithmeticFlagUpdateText}");

        return updatedArithmeticFlags;
    }
}