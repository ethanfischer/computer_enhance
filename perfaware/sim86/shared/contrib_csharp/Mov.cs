using static Sim86;

public static class Mov
{
    public static void Handle(Instruction decoded, int[] registers)
    {
        if (decoded.Operands[0] is not RegisterAccess destReg) return;
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
        }
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
}