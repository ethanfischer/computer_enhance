namespace sim86;

public static class JumpNotEqual
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
}