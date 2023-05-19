using System.Diagnostics;
using System.Runtime.InteropServices;

#pragma warning disable CS8603

public static class Sim86
{
    private static int _clocksCount;

    public enum OperationType : uint
    {
        None,
        mov,
        push,
        pop,
        xchg,
        @in,
        @out,
        xlat,
        lea,
        lds,
        les,
        lahf,
        sahf,
        pushf,
        popf,
        add,
        adc,
        inc,
        aaa,
        daa,
        sub,
        sbb,
        dec,
        neg,
        cmp,
        aas,
        das,
        mul,
        imul,
        aam,
        div,
        idiv,
        aad,
        cbw,
        cwd,
        not,
        shl,
        shr,
        sar,
        rol,
        ror,
        rcl,
        rcr,
        and,
        test,
        or,
        xor,
        rep,
        movs,
        cmps,
        scas,
        lods,
        stos,
        call,
        jmp,
        ret,
        retf,
        je,
        jl,
        jle,
        jb,
        jbe,
        jp,
        jo,
        js,
        jne,
        jnl,
        jg,
        jnb,
        ja,
        jnp,
        jno,
        jns,
        loop,
        loopz,
        loopnz,
        jcxz,
        @int,
        int3,
        into,
        iret,
        clc,
        cmc,
        stc,
        cld,
        std,
        cli,
        sti,
        hlt,
        wait,
        esc,
        @lock,
        segment,
    }

    [Flags]
    public enum InstructionFlag : uint
    {
        Lock = 0x1,
        Rep = 0x2,
        Segment = 0x4,
        Wide = 0x8,
        Far = 0x10,
    };

    [Flags]
    public enum EffectiveAddressFlag : uint
    {
        ExplicitSegment = 0x1,
    };

    [Flags]
    public enum ImmediateFlag : uint
    {
        RelativeJumpDisplacement = 0x1,
    };

    public enum InstructionBitsUsage : byte
    {
        End,
        Literal,
        D,
        S,
        W,
        V,
        Z,
        MOD,
        REG,
        RM,
        SR,
        Disp,
        Data,
        DispAlwaysW,
        WMakesDataW,
        RMRegAlwaysW,
        RelJMPDisp,
        Far,
        Count,
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct RegisterAccess
    {
        public uint Index;
        public uint Offset;
        public uint Count;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EffectiveAddressTerm
    {
        public RegisterAccess Register;
        public int Scale;
    };

    public struct EffectiveAddressExpression
    {
        public EffectiveAddressTerm[] Term;
        public uint ExplicitSegment;
        public int Displacement;
        public EffectiveAddressFlag Flags;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Immediate
    {
        public int Value;
        public ImmediateFlag Flags;
    };

    public struct Instruction
    {
        public uint Address;
        public int Size;
        public OperationType Op;
        public InstructionFlag Flags;
        public object[] Operands;
        public uint SegmentOverride;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct InstructionBits
    {
        public InstructionBitsUsage Usage;
        public byte BitCount;
        public byte Shift;
        public byte Value;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct InstructionEncoding
    {
        public OperationType Op;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public InstructionBits[] Bits;
    };

    public struct InstructionTable
    {
        public InstructionEncoding[] Encodings;
        public uint MaxInstructionByteCount;
    };

    private static class Native
    {
        const string dll = "sim86_lib";

        public enum OperandType : uint
        {
            None,
            Register,
            Memory,
            Immediate,
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct EffectiveAddressExpression
        {
            public EffectiveAddressTerm Term0;
            public EffectiveAddressTerm Term1;
            public uint ExplicitSegment;
            public int Displacement;
            public EffectiveAddressFlag Flags;
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct InstructionOperand
        {
            [FieldOffset(0)] public OperandType OpType;

            [FieldOffset(4)] public EffectiveAddressExpression Address;

            [FieldOffset(4)] public RegisterAccess Register;

            [FieldOffset(4)] public Immediate Immediate;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct Instruction
        {
            public uint Address;
            public uint Size;
            public OperationType Op;
            public InstructionFlag Flags;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public InstructionOperand[] Operands;

            public uint SegmentOverride;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct InstructionTable
        {
            public IntPtr Encodings;
            public int EncodingCount;
            public uint MaxInstructionByteCount;
        }

        [DllImport(dll)]
        public static extern uint Sim86_GetVersion();

        [DllImport(dll)]
        public static extern void Sim86_Decode8086Instruction(uint SourceSize, [In] ref byte Source,
            out Instruction Dest);

        [DllImport(dll)]
        public static extern IntPtr Sim86_RegisterNameFromOperand([In] ref RegisterAccess RegAccess);

        [DllImport(dll)]
        public static extern IntPtr Sim86_MnemonicFromOperationType(OperationType Type);

        [DllImport(dll)]
        public static extern void Sim86_Get8086InstructionTable(out InstructionTable Dest);
    }

    public const int Version = 3;

    public static uint GetVersion()
    {
        return Native.Sim86_GetVersion();
    }

    public static Instruction Decode8086Instruction(Span<byte> Source)
    {
        Native.Instruction NativeInstruction;
        Native.Sim86_Decode8086Instruction((uint)Source.Length, ref MemoryMarshal.AsRef<byte>(Source),
            out NativeInstruction);

        return new Instruction()
        {
            Address = NativeInstruction.Address,
            Size = (int)NativeInstruction.Size,
            Op = NativeInstruction.Op,
            Flags = NativeInstruction.Flags,
            Operands = NativeInstruction.Operands
                .Where(o => o.OpType != Native.OperandType.None)
                .Select<Native.InstructionOperand, object>(o =>
                {
                    if (o.OpType == Native.OperandType.Register)
                    {
                        return o.Register;
                    }
                    else if (o.OpType == Native.OperandType.Memory)
                    {
                        return new EffectiveAddressExpression()
                        {
                            Term = new[] { o.Address.Term0, o.Address.Term1 },
                            ExplicitSegment = o.Address.ExplicitSegment,
                            Displacement = o.Address.Displacement,
                            Flags = o.Address.Flags,
                        };
                    }
                    else
                    {
                        Debug.Assert(o.OpType == Native.OperandType.Immediate);
                        return o.Immediate;
                    }
                })
                .ToArray(),
            SegmentOverride = NativeInstruction.SegmentOverride,
        };
    }

    public static string RegisterNameFromOperand(RegisterAccess RegAccess)
    {
        return Marshal.PtrToStringAnsi(Native.Sim86_RegisterNameFromOperand(ref RegAccess));
    }

    public static string MnemonicFromOperationType(OperationType Type)
    {
        return Marshal.PtrToStringAnsi(Native.Sim86_MnemonicFromOperationType(Type));
    }

    public static InstructionTable Get8086InstructionTable()
    {
        Native.InstructionTable NativeTable;
        Native.Sim86_Get8086InstructionTable(out NativeTable);

        return new InstructionTable()
        {
            Encodings = Enumerable
                .Range(0, NativeTable.EncodingCount)
                .Select(n =>
                    Marshal.PtrToStructure<InstructionEncoding>(NativeTable.Encodings +
                                                                n * Marshal.SizeOf<InstructionEncoding>()))
                .ToArray(),
            MaxInstructionByteCount = NativeTable.MaxInstructionByteCount,
        };
    }

    public static void SetIP(this int[] registers, int value)
    {
        registers[(int)RegisterId.InstructionPointer] = value;
    }

    public static int IP => (int)RegisterId.InstructionPointer;

    public static string Hex(this int value)
    {
        return value.ToString("x");
    }

    public static string IpDebugText(int[] registers, int newIp)
    {
        return $"ip: 0x{registers[IP].Hex()}->0x{newIp.Hex()}";
    }
    
    public static string IpDebugText(int oldIp, int newIp)
    {
        return $"ip: 0x{oldIp.Hex()}->0x{newIp.Hex()}";
    }

    public struct DebugInfo
    {
        public string OpName;
        public string SourceRegister;
        public int NewIP;
        public int OldIP;
    }

    public static string LogClocks(int clocks, string calculation = "")
    {
        _clocksCount += clocks;
        return $"Clocks: +{clocks} = {_clocksCount} {calculation} | ";
    }

    public static int EffectiveAddressCalcClocks(EffectiveAddressExpression eff)
    {
        var reg1 = RegisterNameFromOperand(eff.Term[0].Register).ToLower();
        var reg2 = RegisterNameFromOperand(eff.Term[1].Register);
        if((reg1 == "" && reg2 == "") && eff.Displacement != 0) // Displacement only
            return 6;
        if(reg1 is "bx" or "bp" or "si" or "di" && eff.Displacement == 0) //Base or index only
            return 5;
        if(eff.Displacement != 0 && (reg1 != "" || reg2 != "")) //Displacement + Base or index
            return 9;
        if((reg1 is "bp" && reg2 is "di") || (reg1 is "bx" && reg2 is "si")) //Base + index
            return 7;
        if((reg1 is "bp" && reg2 is "si") || (reg1 is "bx" && reg2 is "di")) //Base + index
            return 8;
        if(eff.Displacement != 0 && reg1 is "bp" && reg2 is "di" || (reg1 is "bx" && reg2 is "si")) //Displacement + Base + index
            return 11;
        if(eff.Displacement != 0 && reg1 is "bp" && reg2 is "si" || (reg1 is "bx" && reg2 is "di")) //Displacement + Base + index
            return 12;
        throw new NotImplementedException();
    }
    
    public static int GetRegisterValue(this EffectiveAddressTerm term, int[] registers)
    {
        var registerName = Sim86.RegisterNameFromOperand(term.Register);
        if (registerName == "") return 0;
        var registerId = (RegisterId)Enum.Parse(typeof(RegisterId), registerName);
        var registerValue = registers[(int)registerId];
        return registerValue;
    }

    public static string GetRegisterNames(this EffectiveAddressExpression eff)
    {
        var result = "";
        var reg1Name = RegisterNameFromOperand(eff.Term[0].Register);
        var reg2Name = RegisterNameFromOperand(eff.Term[1].Register);
        if (reg1Name != "") result += reg1Name;
        if (reg2Name != "") result += $" + {reg2Name}";
        return result;
    }
    
    public static byte LowByte(this int value)
    {
        return (byte)(value & 0xFF);
    }

    public static byte HighByte(this int value)
    {
        return (byte)((value >> 8) & 0xFF);
    }
}