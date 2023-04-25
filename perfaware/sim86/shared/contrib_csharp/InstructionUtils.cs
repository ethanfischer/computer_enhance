public static class InstructionUtils
{
    public static string GetArithmeticFlagUpdateText(ArithmeticFlags arithmeticFlags, ArithmeticFlags updatedArithmeticFlags)
    {
        var zeroFlagBefore = GetZeroFlagText(arithmeticFlags);
        string signedFlagBefore = GetSignedFlagText(arithmeticFlags);
        var zerFlagAfter = GetZeroFlagText(updatedArithmeticFlags);
        var signedFlagAfter = GetSignedFlagText(updatedArithmeticFlags);
        var beforeText = $"{zeroFlagBefore}{signedFlagBefore}";
        var afterText = $"{zerFlagAfter}{signedFlagAfter}";
        var arithmeticFlagUpdateText = (beforeText == afterText) ? "" : $"flags:{beforeText}->{afterText}";
        return arithmeticFlagUpdateText;
    }

    public static string GetSignedFlagText(ArithmeticFlags arithmeticFlags)
    {
        return arithmeticFlags.HasFlag(ArithmeticFlags.Sign) ? "S" : "";
    }

    public static string GetZeroFlagText(ArithmeticFlags arithmeticFlags)
    {
        return arithmeticFlags.HasFlag(ArithmeticFlags.Zero) ? "PZ" : "";
    }

    public static ArithmeticFlags GetUpdatedArithmeticFlags(ArithmeticFlags arithmeticFlags, int result)
    {
        var updatedArithmeticFlags = new ArithmeticFlags() | arithmeticFlags;
        if (result == 0)
        {
            updatedArithmeticFlags = updatedArithmeticFlags | ArithmeticFlags.Zero;
        }
        else
        {
            updatedArithmeticFlags = updatedArithmeticFlags & ~ArithmeticFlags.Zero;
        }
        if ((result & 0x8000) != 0)
        {
            updatedArithmeticFlags = updatedArithmeticFlags | ArithmeticFlags.Sign;
        }
        else
        {
            updatedArithmeticFlags = updatedArithmeticFlags & ~ArithmeticFlags.Sign;
        }

        return updatedArithmeticFlags;
    }
}