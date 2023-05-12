namespace sim86;

public static class JNE
{
    public static void Handle(this Sim86.Instruction decoded, int[] registers, ArithmeticFlags arithmeticFlags)
    {
        var oldIp = registers[Sim86.IP];
        registers[Sim86.IP] += decoded.Size;

        if (decoded.Operands[0] is Sim86.Immediate imm)
        {
            if ((arithmeticFlags & ArithmeticFlags.Zero) == 0)
            {
                registers[Sim86.IP] += imm.Value;
            }


            Console.WriteLine(
                $"{decoded.Op} ${decoded.Size + imm.Value} ; {Sim86.IpDebugText(oldIp, registers[Sim86.IP])}");
        }
    }
}