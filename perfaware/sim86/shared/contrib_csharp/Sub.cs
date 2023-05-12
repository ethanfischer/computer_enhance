using static Sim86;
using static InstructionUtils;

public static class Sub
{
    public static ArithmeticFlags Handle(this Instruction decoded, int[] registers, ArithmeticFlags _arithmeticFlags)
    {
        if (decoded.Operands[0] is not RegisterAccess destReg) throw new Exception();
        
        var destRegisterName = Sim86.RegisterNameFromOperand(destReg);
        var destRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), destRegisterName);

        return decoded.Operands[1] switch
        {
            Immediate imm => ImmediateToRegister(decoded, destRegisterName, destRegisterId, registers, imm, _arithmeticFlags),
            RegisterAccess sourceReg => RegisterToRegister(decoded, destRegisterName, destRegisterId, registers, sourceReg, _arithmeticFlags),
            _ => throw new Exception()
        };
    }

    private static ArithmeticFlags RegisterToRegister(Instruction decoded, string destRegisterName, RegisterId destRegisterId, int[] registers, RegisterAccess sourceReg, ArithmeticFlags arithmeticFlags)
    {
        var sourceRegisterName = Sim86.RegisterNameFromOperand(sourceReg);
        var sourceRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), sourceRegisterName);
        var sourceRegister = registers[(int)sourceRegisterId];
        var destRegister = registers[(int)destRegisterId];
        var oldId = registers[IP];
        var newIp = registers[IP] + decoded.Size;
        registers[IP] = newIp;
        var result = destRegister - sourceRegister;
        registers[(int)destRegisterId] = result;
        var updatedArithmeticFlags = GetUpdatedArithmeticFlags(arithmeticFlags, result);
        var arithmeticFlagUpdateText = GetArithmeticFlagUpdateText(arithmeticFlags, updatedArithmeticFlags);
        Console.WriteLine($"{decoded.Op} {destRegisterName}, {sourceRegisterName} ; {destRegisterName}:0x{destRegister.Hex()}->0x{result.Hex()} {IpDebugText(oldId, newIp)} {arithmeticFlagUpdateText}");
        return updatedArithmeticFlags;
    }

    private static ArithmeticFlags ImmediateToRegister(Instruction decoded, string destRegisterName, RegisterId destRegisterId, int[] registers, Immediate imm, ArithmeticFlags arithmeticFlags)
    {
        var destRegister = registers[(int)destRegisterId];
        var result = destRegister - imm.Value;
        var oldId = registers[IP];
        var newIp = registers[IP] + decoded.Size;
        registers[IP] = newIp;
        registers[(int)destRegisterId] = result;

        var updatedArithmeticFlags = GetUpdatedArithmeticFlags(arithmeticFlags, result);
        var arithmeticFlagUpdateText = GetArithmeticFlagUpdateText(arithmeticFlags, updatedArithmeticFlags);

        Console.WriteLine($"{decoded.Op} {destRegisterName}, {imm.Value} ; {destRegisterName}:0x{destRegister.Hex()}->0x{result.Hex()} {IpDebugText(oldId, newIp)} {arithmeticFlagUpdateText} ");

        return updatedArithmeticFlags;
    }
}