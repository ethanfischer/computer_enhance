using static Sim86;

public static class Cmp
{
    public static void Handle(this Instruction decoded, int[] registers, ArithmeticFlags _arithmeticFlags)
    {
        if (decoded.Operands[0] is RegisterAccess destReg)
        {
            var destRegisterName = Sim86.RegisterNameFromOperand(destReg);
            var destRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), destRegisterName);

            if (decoded.Operands[1] is Immediate imm)
            {
                ImmediateToRegister(decoded, destRegisterName, destRegisterId, registers, imm);
            }
            else if (decoded.Operands[1] is RegisterAccess sourceReg)
            {
                RegisterToRegister(decoded, destRegisterName, destRegisterId, registers, sourceReg);
            }
        }
    }

    private static void RegisterToRegister(Instruction decoded, string destRegisterName, RegisterId destRegisterId, int[] registers, RegisterAccess sourceReg)
    {
        var sourceRegisterName = Sim86.RegisterNameFromOperand(sourceReg);
        var sourceRegisterId = (RegisterId)Enum.Parse(typeof(RegisterId), sourceRegisterName);
        var sourceRegister = registers[(int)sourceRegisterId];
        var destRegister = registers[(int)destRegisterId];
        Console.WriteLine($"{decoded.Op} {destRegisterName}, {sourceRegisterName} ; {destRegisterName}:0x{destRegister.ToString("X")}->0x{sourceRegister.ToString("X")}");
        registers[(int)destRegisterId] = sourceRegister;
    }

    public static void ImmediateToRegister(Instruction decoded, string destRegisterName, RegisterId destRegisterId, int[] registers, Immediate imm)
    {
        var destRegister = registers[(int)destRegisterId];
        Console.WriteLine($"{decoded.Op} {destRegisterName}, {imm.Value} ; {destRegisterName}:0x{destRegister.ToString("X")}->0x{imm.Value.ToString("X")}");
        registers[(int)destRegisterId] = imm.Value;
    }
}