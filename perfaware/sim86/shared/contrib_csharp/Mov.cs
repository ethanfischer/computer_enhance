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
                    var sourceRegisterName = RegisterNameFromOperand(sourceReg);
                    var sourceRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), sourceRegisterName);
                    var registerValue = registers[(int)sourceRegisterId];
                    StoreInMemory(memory, eff, registerValue, registers, new DebugInfo
                    {
                        OpName = decoded.Op.ToString(),
                        SourceRegister = sourceRegisterName,
                        OldIP = oldIp,
                        NewIP = newIp
                    });
                    break;
                }
                case Immediate imm:
                    StoreInMemory(memory, eff, imm.Value, registers, new DebugInfo
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

    private static void StoreInMemory(IList<byte> memory, EffectiveAddressExpression eff, int value, int[] registers,
        DebugInfo debug)
    {
        var reg1 = eff.Term[0].GetRegisterValue(registers);
        var reg2 = eff.Term[1].GetRegisterValue(registers);
        var memoryAddress = eff.Displacement + reg1 + reg2;

        Console.WriteLine(
            $"{debug.OpName} [{eff.GetRegisterNames()}+{eff.Displacement}], {debug.SourceRegister} ;" +
            LogClocks(9 + EffectiveAddressCalcClocks(eff), $"9 + {EffectiveAddressCalcClocks(eff)}ea") +
            $" {IpDebugText(debug.OldIP, debug.NewIP)}");

        memory[memoryAddress] = value.LowByte();
        memory[memoryAddress + 1] = value.HighByte();
    }

    private static void LoadFromMemory(Instruction decoded, string destRegisterName, RegisterId destRegisterId,
        int[] registers, EffectiveAddressExpression eff, IReadOnlyList<byte> memory)
    {
        var reg1 = eff.Term[0].GetRegisterValue(registers);
        var reg2 = eff.Term[1].GetRegisterValue(registers);
        var memoryAddress = eff.Displacement + reg1 + reg2;
        var destRegister = registers[(int)destRegisterId];
        var newIp = registers[IP] + decoded.Size;
        var value = memory[memoryAddress] | (memory[memoryAddress + 1] << 8);
        Console.WriteLine(
            $"{decoded.Op} {destRegisterName}, {eff.GetRegisterNames()}+{eff.Displacement} ;" +
            LogClocks(8 + EffectiveAddressCalcClocks(eff), $"(8 + {EffectiveAddressCalcClocks(eff)}ea)") +
            $" {destRegisterName}:0x{destRegister.Hex()}->0x{value.Hex()} {IpDebugText(registers, newIp)}");
        registers[IP] = newIp;
        registers[(int)destRegisterId] = value;
    }

    private static void RegisterToRegister(Instruction decoded, string destRegisterName, RegisterId destRegisterId,
        int[] registers, RegisterAccess sourceReg)
    {
        var sourceRegisterName = RegisterNameFromOperand(sourceReg);
        var sourceRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), sourceRegisterName);
        var sourceRegister = registers[(int)sourceRegisterId];
        var destRegister = registers[(int)destRegisterId];
        var newIp = registers[IP] + decoded.Size;
        Console.WriteLine(
            $"{decoded.Op} {destRegisterName}, {sourceRegisterName} ;" +
            LogClocks(2) +
            $" {destRegisterName}:0x{destRegister.Hex()}->0x{sourceRegister.Hex()} {IpDebugText(registers, newIp)}");
        registers[IP] = newIp;
        registers[(int)destRegisterId] = sourceRegister;
    }

    private static void ImmediateToRegister(Instruction decoded, string destRegisterName, RegisterId destRegisterId,
        int[] registers, Immediate imm)
    {
        var destRegister = registers[(int)destRegisterId];
        var newIp = registers[IP] + decoded.Size;
        Console.WriteLine(
            $"{decoded.Op} {destRegisterName}, {imm.Value} ;" +
            LogClocks(4) +
            $" {destRegisterName}:0x{destRegister.Hex()}->0x{imm.Value.Hex()} {IpDebugText(registers, newIp)}");
        registers[IP] = newIp;
        registers[(int)destRegisterId] = imm.Value;
    }

}