public static class InstructionUtils
{
    public static string GetArithmeticFlagUpdateText(OperationFlags operationFlags, OperationFlags updatedOperationFlags)
    {
        var zeroFlagBefore = GetZeroFlagText(operationFlags);
        string signedFlagBefore = GetSignedFlagText(operationFlags);
        var zerFlagAfter = GetZeroFlagText(updatedOperationFlags);
        var signedFlagAfter = GetSignedFlagText(updatedOperationFlags);
        var beforeText = $"{zeroFlagBefore}{signedFlagBefore}";
        var afterText = $"{zerFlagAfter}{signedFlagAfter}";
        var arithmeticFlagUpdateText = (beforeText == afterText) ? "" : $"flags:{beforeText}->{afterText}";
        return arithmeticFlagUpdateText;
    }

    public static string GetSignedFlagText(OperationFlags operationFlags)
    {
        return operationFlags.HasFlag(OperationFlags.Sign) ? "S" : "";
    }

    public static string GetZeroFlagText(OperationFlags operationFlags)
    {
        return operationFlags.HasFlag(OperationFlags.Zero) ? "PZ" : "";
    }

    public static OperationFlags GetUpdatedArithmeticFlags(OperationFlags operationFlags, int result)
    {
        var updatedArithmeticFlags = new OperationFlags() | operationFlags;
        if (result == 0)
        {
            updatedArithmeticFlags = updatedArithmeticFlags | OperationFlags.Zero;
        }
        else
        {
            updatedArithmeticFlags = updatedArithmeticFlags & ~OperationFlags.Zero;
        }
        if ((result & 0x8000) != 0)
        {
            updatedArithmeticFlags = updatedArithmeticFlags | OperationFlags.Sign;
        }
        else
        {
            updatedArithmeticFlags = updatedArithmeticFlags & ~OperationFlags.Sign;
        }

        return updatedArithmeticFlags;
    }
}