using static Sim86;

public static class Mov
{
    public static void Handle(Instruction decoded, int[] registers, byte[] memory)
    {
        if (decoded.Operands[0] is RegisterAccess destReg)
        {
            var destRegisterName = Sim86.RegisterNameFromOperand(destReg);
            var destRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), destRegisterName);

            switch (decoded.Operands[1])
            {
                case Immediate imm:
                    ImmediateToRegister(decoded, destRegisterName, destRegisterId, registers, imm);
                    break;
                case RegisterAccess sourceReg:
                    RegisterToRegister(decoded, destRegisterName, destRegisterId, registers, sourceReg);
                    break;
                case EffectiveAddressExpression eff:
                    LoadFromMemory(decoded, destRegisterName, destRegisterId, registers, eff, memory);
                    break;
            }
        }
        else if (decoded.Operands[0] is EffectiveAddressExpression eff)
        {
            var oldIp = registers[IP];
            var newIp = registers[IP] + decoded.Size;
            registers[IP] = newIp;
            switch (decoded.Operands[1])
            {
                case RegisterAccess sourceReg:
                {
                    var sourceRegisterName = Sim86.RegisterNameFromOperand(sourceReg);
                    var sourceRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), sourceRegisterName);
                    var registerValue = registers[(int)sourceRegisterId];
                    StoreInMemory(memory, eff, registerValue, registers, new DebugInfo()
                    {
                        OpName = decoded.Op.ToString(),
                        SourceRegister = sourceRegisterName,
                        OldIP = oldIp,
                        NewIP = newIp
                    });
                    break;
                }
                case Immediate imm:
                    StoreInMemory(memory, eff, imm.Value, registers, new DebugInfo()
                    {
                        OpName = decoded.Op.ToString(),
                        SourceRegister = imm.Value.ToString(),
                        OldIP = oldIp,
                        NewIP = newIp
                    });
                    break;
            }
        }
    }

    private static void StoreInMemory(byte[] memory, EffectiveAddressExpression eff, int value, int[] registers,
        DebugInfo debug)
    {
        var registerName = Sim86.RegisterNameFromOperand(eff.Term[0].Register);
        if (registerName != "")
        {
            var registerId = (RegisterId)Enum.Parse(typeof(RegisterId), registerName);
            var registerValue = registers[(int)registerId];
            Console.WriteLine(
                $"{debug.OpName} [{registerName}+{eff.Displacement}], {debug.SourceRegister} ; {IpDebugText(debug.OldIP, debug.NewIP)}");
            memory[registerValue + eff.Displacement] = value.LowByte();
            memory[registerValue + eff.Displacement + 1] = value.HighByte();
        }
        else
        {
            Console.WriteLine(
                $"{debug.OpName} [{eff.Displacement}], {debug.SourceRegister} ; {IpDebugText(debug.OldIP, debug.NewIP)}");
            memory[eff.Displacement] = value.LowByte();
            memory[eff.Displacement + 1] = value.HighByte();
        }
    }

    private static void LoadFromMemory(Instruction decoded, string destRegisterName, RegisterId destRegisterId,
        int[] registers, EffectiveAddressExpression eae, byte[] memory)
    {
        var destRegister = registers[(int)destRegisterId];
        var newIp = registers[IP] + decoded.Size;
        var value = memory[eae.Displacement] | (memory[eae.Displacement + 1] << 8);
        Console.WriteLine(
            $"{decoded.Op} {destRegisterName}, {eae.Displacement} ; {destRegisterName}:0x{destRegister.Hex()}->0x{value.Hex()} {IpDebugText(registers, newIp)}");
        registers[IP] = newIp;
        registers[(int)destRegisterId] = value;
    }

    private static void RegisterToRegister(Instruction decoded, string destRegisterName, RegisterId destRegisterId,
        int[] registers, RegisterAccess sourceReg)
    {
        var sourceRegisterName = Sim86.RegisterNameFromOperand(sourceReg);
        var sourceRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), sourceRegisterName);
        var sourceRegister = registers[(int)sourceRegisterId];
        var destRegister = registers[(int)destRegisterId];
        var newIp = registers[IP] + decoded.Size;
        Console.WriteLine(
            $"{decoded.Op} {destRegisterName}, {sourceRegisterName} ; {destRegisterName}:0x{destRegister.Hex()}->0x{sourceRegister.Hex()} {IpDebugText(registers, newIp)}");
        registers[IP] = newIp;
        registers[(int)destRegisterId] = sourceRegister;
    }

    private static void ImmediateToRegister(Instruction decoded, string destRegisterName, RegisterId destRegisterId,
        int[] registers, Immediate imm)
    {
        var destRegister = registers[(int)destRegisterId];
        var newIp = registers[IP] + decoded.Size;
        Console.WriteLine(
            $"{decoded.Op} {destRegisterName}, {imm.Value} ; {destRegisterName}:0x{destRegister.Hex()}->0x{imm.Value.Hex()} {IpDebugText(registers, newIp)}");
        registers[IP] = newIp;
        registers[(int)destRegisterId] = imm.Value;
    }

    private static byte LowByte(this int value)
    {
        return (byte)(value & 0xFF);
    }

    private static byte HighByte(this int value)
    {
        return (byte)((value >> 8) & 0xFF);
    }
}