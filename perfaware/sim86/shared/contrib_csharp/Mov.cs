public static class Mov
{
    public static void Handle(this Sim86.Instruction Decoded, int[] registers)
    {
        if (Decoded.Operands[0] is Sim86.RegisterAccess reg)
        {
            var registerName = Sim86.RegisterNameFromOperand(reg);
            var registerId = (RegisterId)Enum.Parse(typeof(RegisterId), registerName);
            Immediate(Decoded, registerName, registerId, registers);
        }
    }

    public static void Immediate(Sim86.Instruction Decoded, string registerName, RegisterId registerId, int[] registers)
    {
        if (Decoded.Operands[1] is Sim86.Immediate imm)
        {
            var register = registers[(int)registerId];
            Console.WriteLine($"{Decoded.Op} {registerName}, {imm.Value} ; {registerName}:0x{register.ToString("X")}->0x{imm.Value.ToString("X")}");
            registers[(int)registerId] = imm.Value;
        }
    }
}