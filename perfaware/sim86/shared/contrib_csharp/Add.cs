using static Sim86;
using static InstructionUtils;

public static class Add
{
    public static ArithmeticFlags Handle(this Instruction decoded, int[] registers, ArithmeticFlags arithmeticFlags,
        byte[] memory)
    {
        switch (decoded.Operands[0])
        {
            case RegisterAccess destReg:
            {
                var destRegisterName = Sim86.RegisterNameFromOperand(destReg);
                var destRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), destRegisterName);

                if (decoded.Operands[1] is Immediate imm)
                {
                    return ImmediateToRegister(decoded, destRegisterName, destRegisterId, registers, imm,
                        arithmeticFlags);
                }

                if (decoded.Operands[1] is RegisterAccess sourceReg)
                {
                    return RegisterToRegister(decoded, destRegisterName, destRegisterId, registers, sourceReg,
                        arithmeticFlags);
                }

                break;
            }
            case EffectiveAddressExpression eff:
            {
                var oldIp = registers[IP];
                var newIp = registers[IP] + decoded.Size;
                registers[IP] = newIp;
                switch (decoded.Operands[1])
                {
                    case RegisterAccess sourceReg:
                    {
                        var sourceRegisterName = RegisterNameFromOperand(sourceReg);
                        var sourceRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), sourceRegisterName);
                        var registerValue = registers[(int)sourceRegisterId];
                        AddInMemory(memory, eff, registerValue, registers, new DebugInfo
                        {
                            OpName = decoded.Op.ToString(),
                            SourceRegister = sourceRegisterName,
                            OldIP = oldIp,
                            NewIP = newIp
                        });
                        return arithmeticFlags;
                    }
                    case Immediate imm:
                        return arithmeticFlags;
                }

                break;
            }
        }

        throw new Exception();
    }

    private static ArithmeticFlags RegisterToRegister(Instruction decoded, string destRegisterName,
        RegisterId destRegisterId, int[] registers, RegisterAccess sourceReg, ArithmeticFlags arithmeticFlags)
    {
        var sourceRegisterName = Sim86.RegisterNameFromOperand(sourceReg);
        var sourceRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), sourceRegisterName);
        var sourceRegister = registers[(int)sourceRegisterId];
        var destRegister = registers[(int)destRegisterId];

        var result = destRegister + sourceRegister;
        var updatedArithmeticFlags = GetUpdatedArithmeticFlags(arithmeticFlags, result);
        var arithmeticFlagUpdateText = GetArithmeticFlagUpdateText(arithmeticFlags, updatedArithmeticFlags);
        var newIp = registers[IP] + decoded.Size;
        Console.WriteLine(
            $"{decoded.Op} {destRegisterName}, {sourceRegisterName} ;" +
            Sim86.LogClocks(3) +
            $" {destRegisterName}:0x{destRegister.Hex()}->0x{result.Hex()} {IpDebugText(registers, newIp)} {arithmeticFlagUpdateText} ");
        registers[IP] = newIp;
        registers[(int)destRegisterId] = result;
        return updatedArithmeticFlags;
    }

    private static ArithmeticFlags ImmediateToRegister(Instruction decoded, string destRegisterName,
        RegisterId destRegisterId, int[] registers, Immediate imm, ArithmeticFlags arithmeticFlags)
    {
        var destRegister = registers[(int)destRegisterId];
        var result = destRegister + imm.Value;
        var updatedArithmeticFlags = GetUpdatedArithmeticFlags(arithmeticFlags, result);
        var arithmeticFlagUpdateText = GetArithmeticFlagUpdateText(arithmeticFlags, updatedArithmeticFlags);
        var newIp = registers[IP] + decoded.Size;
        Console.WriteLine(
            $"{decoded.Op} {destRegisterName}, {imm.Value} ;" +
            Sim86.LogClocks(4) +
            $" {destRegisterName}:0x{destRegister.Hex()}->0x{result.Hex()} {IpDebugText(registers, newIp)} {arithmeticFlagUpdateText} ");
        registers[IP] = newIp;
        registers[(int)destRegisterId] = result;
        return updatedArithmeticFlags;
    }


    private static void AddInMemory(IList<byte> memory, EffectiveAddressExpression eff, int value, int[] registers,
        DebugInfo debug)
    {
        var reg1 = eff.Term[0].GetRegisterValue(registers);
        var reg2 = eff.Term[1].GetRegisterValue(registers);
        var memoryAddress = eff.Displacement + reg1 + reg2;

        Console.WriteLine(
            $"{debug.OpName} [{eff.GetRegisterNames()}+{eff.Displacement}], {debug.SourceRegister} ;" +
            LogClocks(16 + EffectiveAddressCalcClocks(eff), $"16 + {EffectiveAddressCalcClocks(eff)}ea") +
            $" {IpDebugText(debug.OldIP, debug.NewIP)}");

        memory[memoryAddress] = value.LowByte();
        memory[memoryAddress + 1] = value.HighByte();
    }
}