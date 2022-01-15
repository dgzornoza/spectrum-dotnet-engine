﻿namespace SpectrumEngine.Emu;

/// <summary>
/// This class implements the emulation of the Z80 CPU.
/// </summary>
/// <remarks>
/// This partition contains the code for processing standard Z80 instructions (with no prefix).
/// </remarks>
public partial class Z80Cpu
{
    /// <summary>
    /// This array contains the 256 function references, each executing a  standard Z80 instruction.
    /// </summary>
    private Action[]? _standardInstrs;

    /// <summary>
    /// Initialize the table of standard instructions.
    /// </summary>
    private void InitializeStandardInstructionsTable()
    {
        _standardInstrs = new Action[]
        {
            Nop,        LdBCNN,     LdBCiA,     IncBC,      IncB,       DecB,       LdBN,       Rlca,       // 00-07
            ExAF,       AddHLBC,    LdABCi,     DecBC,      IncC,       DecC,       LdCN,       Rrca,       // 08-0f
            Djnz,       LdDENN,     LdDEiA,     IncDE,      IncD,       DecD,       LdDN,       Rla,        // 10-17
            JrE,        AddHLDE,    LdADEi,     DecDE,      IncE,       DecE,       LdEN,       Rra,        // 18-1f
            JrNZ,       LdHLNN,     LdNNiHL,    Nop,        Nop,        Nop,        Nop,        Nop,        // 20-27
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 28-2f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Scf,        // 30-37
            Nop,        Nop,        Nop,        Nop,        Nop,        DecA,       LdAN,       Nop,        // 38-3f

            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 40-47
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 48-4f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 50-57
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 58-5f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 60-67
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 68-6f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 70-77
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 78-7f

            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 80-87
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 88-8f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 90-97
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // 98-9f
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // a0-a7
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // a8-af
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // b0-b7
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // b8-bf

            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // c0-c7
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // c8-cf
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // d0-d7
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // d8-df
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // e0-e7
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // e8-ef
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // f0-f7
            Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        Nop,        // f8-ff
        };
    }

    /// <summary>
    /// "nop" instruction (0x00)
    /// </summary>
    /// <remarks>
    /// The CPU performs no operation during this machine cycle.
    /// This instruction does not affect any flag.
    /// 
    /// T-states: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Nop() { }

    /// <summary>
    /// "ld bc,NN" instruction (0x01, N-LSB, N-MSB)
    /// </summary>
    /// <remarks>
    /// The 16-bit integer value is loaded to the BC register pair.
    /// This instruction does not affect any flag.
    /// 
    /// T-states: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void LdBCNN()
    {
        Regs.C = ReadCodeMemory();
        Regs.B = ReadCodeMemory();
    }

    /// <summary>
    /// "ld (bc),a" operation (0x02)
    /// </summary>
    /// <remarks>
    /// The contents of the A are loaded to the memory location specified by the contents of the register pair BC.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,bc:3
    /// </remarks>
    private void LdBCiA()
    {
        WriteMemory(Regs.BC, Regs.A);
        Regs.WH = Regs.A;
    }

    /// <summary>
    /// "inc bc" operation (0x03)
    /// </summary>
    /// <remarks>
    /// The contents of register pair BC are incremented.
    ///
    /// T-States: 6 (4, 2)
    /// Contention breakdown: pc:6
    /// </remarks>
    private void IncBC()
    {
        Regs.BC++;
        TactPlus2(Regs.IR);
    }

    /// <summary>
    /// "inc b" operation (0x04)
    /// </summary>
    /// <remarks>
    /// Register B is incremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if r was 7Fh before operation; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void IncB()
    {
        Regs.F = (byte)(s_8BitIncFlags[Regs.B++] | (Regs.F & FlagsSetMask.C));
    }

    /// <summary>
    /// "dec b" operation (0x05)
    /// </summary>
    /// <remarks>
    /// Register B is decremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4, otherwise, it is reset.
    /// P/V is set if m was 80h before operation; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void DecB()
    {
        Regs.F = (byte)(s_8BitDecFlags[Regs.B--] | (Regs.F & FlagsSetMask.C));
    }

    /// <summary>
    /// "ld b,N" operation (0x06)
    /// </summary>
    /// <remarks>
    /// The 8-bit integer N is loaded to B.
    /// 
    /// T-States: 7, (4, 3)
    /// Contention breakdown: pc:4,pc+1:3
    /// </remarks>
    private void LdBN()
    {
        Regs.B = ReadCodeMemory();
    }

    /// <summary>
    /// "rlca" operation (0x07)
    /// </summary>
    /// <remarks>
    /// The contents of  A are rotated left 1 bit position. The sign bit (bit 7) is copied to the Carry flag and also
    /// to bit 0.
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of A.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Rlca()
    {
        int rlcaVal = Regs.A;
        rlcaVal <<= 1;
        var cf = (byte)((rlcaVal & 0x100) != 0 ? FlagsSetMask.C : 0);
        if (cf != 0)
        {
            rlcaVal = (rlcaVal | 0x01) & 0xFF;
        }
        Regs.A = (byte)rlcaVal;
        Regs.F = (byte)(cf | (Regs.F & FlagsSetMask.SZPV) | (Regs.A & FlagsSetMask.R3R5));
        F53Updated = true;
    }

    /// <summary>
    /// "ex af,af'" operation (0x08)
    /// </summary>
    /// <remarks>
    ///  The 2-byte contents of the register pairs AF and AF' are exchanged.
    ///  
    ///  T-States: 4
    ///  Contention breakdown: pc:4
    /// </remarks>
    private void ExAF()
    {
        Regs.ExchangeAfSet();
    }

    /// <summary>
    /// "add hl,bc" operation (0x09)
    /// </summary>
    /// <remarks>
    /// The contents of BC are added to the contents of HL and the result is stored in HL.
    /// S, Z, P/V are not affected.
    /// H is set if carry from bit 11; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 15; otherwise, it is reset.
    /// 
    /// T-States: 11 (4, 4, 3)
    /// Contention breakdown: pc:11
    /// </remarks>
    private void AddHLBC()
    {
        Regs.WZ = (ushort)(Regs.HL + 1);
        Regs.HL = AluAddHL(Regs.HL, Regs.BC);
        TactPlus7(Regs.IR);
    }

    /// <summary>
    /// "ld a,(bc)" operation (0x0A)
    /// </summary>
    /// <remarks>
    /// The contents of the memory location specified by BC are loaded to A.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,bc:3
    /// </remarks>
    private void LdABCi()
    {
        Regs.WZ = (ushort)(Regs.BC + 1);
        Regs.A = ReadMemory(Regs.BC);
    }

    /// <summary>
    /// "dec bc" operation (0x0B)
    /// </summary>
    /// <remarks>
    /// The contents of register pair BC are decremented.
    /// 
    /// T-States: 6 (4, 2)
    /// Contention breakdown: pc:6
    /// </remarks>
    private void DecBC()
    {
        Regs.BC--;
        TactPlus2(Regs.IR);
    }

    /// <summary>
    /// "inc c" operation (0x0c)
    /// </summary>
    /// <remarks>
    /// Register C is incremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if r was 7Fh before operation; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void IncC()
    {
        Regs.F = (byte)(s_8BitIncFlags[Regs.C++] | (Regs.F & FlagsSetMask.C));
    }

    /// <summary>
    /// "dec c" operation (0x0D)
    /// </summary>
    /// <remarks>
    /// Register C is decremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4, otherwise, it is reset.
    /// P/V is set if m was 80h before operation; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// </remarks>
    private void DecC()
    {
        Regs.F = (byte)(s_8BitDecFlags[Regs.C--] | (Regs.F & FlagsSetMask.C));
    }

    /// <summary>
    /// "ld c,N" operation (0x0E)
    /// </summary>
    /// <remarks>
    /// The 8-bit integer N is loaded to C.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdCN()
    {
        Regs.C = ReadCodeMemory();
    }

    /// <summary>
    /// "rrca" operation (0x0F)
    /// </summary>
    /// <remarks>
    /// The contents of A are rotated right 1 bit position. Bit 0 is copied to the Carry flag and also to bit 7.
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of A.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Rrca()
    {
        int rrcaVal = Regs.A;
        var cf = (byte)((rrcaVal & 0x01) != 0 ? FlagsSetMask.C : 0);
        if ((rrcaVal & 0x01) != 0)
        {
            rrcaVal = (rrcaVal >> 1) | 0x80;
        }
        else
        {
            rrcaVal >>= 1;
        }
        Regs.A = (byte)rrcaVal;
        Regs.F = (byte)(cf | (Regs.F & FlagsSetMask.SZPV) | (Regs.A & FlagsSetMask.R3R5));
        F53Updated = true;
    }

    /// <summary>
    /// "djnz E" operation (0x10)
    /// </summary>
    /// <remarks>
    /// This instruction is similar to the conditional jump instructions except that value of B is used to determine
    /// branching. B is decremented, and if a nonzero value remains, the value of displacement E is added to PC. The
    /// next instruction is fetched from the location designated by the new contents of the PC. The jump is measured
    /// from the address of the instruction opcode and contains a range of –126 to +129 bytes. The assembler
    /// automatically adjusts for the twice incremented PC. If the result of decrementing leaves B with a zero value,
    /// the next instruction executed is taken from the location following this instruction.
    /// 
    /// T-States:
    ///   B!=0: 13 (5, 3, 5)
    ///   B=0:  8 (5, 3)
    /// Contention breakdown: pc:5,pc+1:3,[pc+1:1 x 5]
    /// Gate array contention breakdown: pc:5,pc+1:3,[5]
    /// </remarks>
    private void Djnz()
    {
        TactPlus1(Regs.IR);
        var e = ReadCodeMemory();
        if (--Regs.B != 0)
        {
            RelativeJump(e);
        }
    }

    /// <summary>
    /// "ld de,NN" instruction (0x11, N-LSB, N-MSB)
    /// </summary>
    /// <remarks>
    /// The 16-bit integer value is loaded to the DE register pair.
    /// This instruction does not affect any flag.
    /// 
    /// T-states: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void LdDENN()
    {
        Regs.E = ReadCodeMemory();
        Regs.D = ReadCodeMemory();
    }

    /// <summary>
    /// "ld (de),a" operation (0x12)
    /// </summary>
    /// <remarks>
    /// The contents of the A are loaded to the memory location specified by the contents of the register pair DE.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,bc:3
    /// </remarks>
    private void LdDEiA()
    {
        WriteMemory(Regs.DE, Regs.A);
        Regs.WH = Regs.A;
    }

    /// <summary>
    /// "inc de" operation (0x13)
    /// </summary>
    /// <remarks>
    /// The contents of register pair DE are incremented.
    ///
    /// T-States: 6 (4, 2)
    /// Contention breakdown: pc:6
    /// </remarks>
    private void IncDE()
    {
        Regs.DE++;
        TactPlus2(Regs.IR);
    }

    /// <summary>
    /// "inc d" operation (0x14)
    /// </summary>
    /// <remarks>
    /// Register D is incremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if r was 7Fh before operation; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void IncD()
    {
        Regs.F = (byte)(s_8BitIncFlags[Regs.D++] | (Regs.F & FlagsSetMask.C));
    }

    /// <summary>
    /// "dec d" operation (0x15)
    /// </summary>
    /// <remarks>
    /// Register D is decremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4, otherwise, it is reset.
    /// P/V is set if m was 80h before operation; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void DecD()
    {
        Regs.F = (byte)(s_8BitDecFlags[Regs.D--] | (Regs.F & FlagsSetMask.C));
    }

    /// <summary>
    /// "ld d,N" operation (0x16)
    /// </summary>
    /// <remarks>
    /// The 8-bit integer N is loaded to D.
    /// 
    /// T-States: 7, (4, 3)
    /// Contention breakdown: pc:4,pc+1:3
    /// </remarks>
    private void LdDN()
    {
        Regs.D = ReadCodeMemory();
    }

    /// <summary>
    /// "rla" operation (0x17)
    /// </summary>
    /// <remarks>
    /// The contents of A are rotated left 1 bit position through the Carry flag. The previous contents of the Carry
    /// flag are copied to bit 0.
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 7 of A.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Rla()
    {
        var rlaVal = Regs.A;
        var newCF = (rlaVal & 0x80) != 0 ? FlagsSetMask.C : 0;
        rlaVal <<= 1;
        if (Regs.CFlag)
        {
            rlaVal |= 0x01;
        }
        Regs.A = rlaVal;
        Regs.F = (byte)(newCF | (Regs.F & FlagsSetMask.SZPV) | (Regs.A & FlagsSetMask.R3R5));
        F53Updated = true;
    }

    /// <summary>
    /// "jr e" operation (0x18)
    /// </summary>
    /// <remarks>
    /// This instruction provides for unconditional branching to other segments of a program. The value of displacement
    /// E is added to PC and the next instruction is fetched from the location designated by the new contents of the
    /// PC. This jump is measured from the address of the instruction op code and contains a range of –126 to +129
    /// bytes. The assembler automatically adjusts for the twice incremented PC.
    /// 
    /// T-States: 12 (4, 3, 5)
    /// Contention breakdown: pc:4,pc+1:3
    /// </remarks>
    private void JrE()
    {
        RelativeJump(ReadCodeMemory());
    }

    /// <summary>
    /// "add hl,de" operation (0x19)
    /// </summary>
    /// <remarks>
    /// The contents of DE are added to the contents of HL and the result is stored in HL.
    /// S, Z, P/V are not affected.
    /// H is set if carry from bit 11; otherwise, it is reset.
    /// N is reset.
    /// C is set if carry from bit 15; otherwise, it is reset.
    /// 
    /// T-States: 11 (4, 4, 3)
    /// Contention breakdown: pc:11
    /// </remarks>
    private void AddHLDE()
    {
        Regs.WZ = (ushort)(Regs.HL + 1);
        Regs.HL = AluAddHL(Regs.HL, Regs.DE);
        TactPlus7(Regs.IR);
    }

    /// <summary>
    /// "ld a,(de)" operation (0x1A)
    /// </summary>
    /// <remarks>
    /// The contents of the memory location specified by DE are loaded to A.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,bc:3
    /// </remarks>
    private void LdADEi()
    {
        Regs.WZ = (ushort)(Regs.DE + 1);
        Regs.A = ReadMemory(Regs.DE);
    }

    /// <summary>
    /// "dec de" operation (0x1B)
    /// </summary>
    /// <remarks>
    /// The contents of register pair DE are decremented.
    /// 
    /// T-States: 6 (4, 2)
    /// Contention breakdown: pc:6
    /// </remarks>
    private void DecDE()
    {
        Regs.DE--;
        TactPlus2(Regs.IR);
    }

    /// <summary>
    /// "inc e" operation (0x1C)
    /// </summary>
    /// <remarks>
    /// Register E is incremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if carry from bit 3; otherwise, it is reset.
    /// P/V is set if r was 7Fh before operation; otherwise, it is reset.
    /// N is reset.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void IncE()
    {
        Regs.F = (byte)(s_8BitIncFlags[Regs.E++] | (Regs.F & FlagsSetMask.C));
    }

    /// <summary>
    /// "dec e" operation (0x1D)
    /// </summary>
    /// <remarks>
    /// Register E is decremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4, otherwise, it is reset.
    /// P/V is set if m was 80h before operation; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// </remarks>
    private void DecE()
    {
        Regs.F = (byte)(s_8BitDecFlags[Regs.E--] | (Regs.F & FlagsSetMask.C));
    }

    /// <summary>
    /// "ld e,N" operation (0x1E)
    /// </summary>
    /// <remarks>
    /// The 8-bit integer N is loaded to E.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4
    /// </remarks>
    private void LdEN()
    {
        Regs.E = ReadCodeMemory();
    }

    /// <summary>
    /// "rra" operation (0x1F)
    /// </summary>
    /// <remarks>
    /// The contents of A are rotated right 1 bit position through the Carry flag. The previous contents of the Carry
    /// flag are copied to bit 7.
    /// S, Z, P/V are not affected.
    /// H, N are reset.
    /// C is data from bit 0 of A.
    ///     
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Rra()
    {
        var rraVal = Regs.A;
        var newCF = (rraVal & 0x01) != 0 ? FlagsSetMask.C : 0;
        rraVal >>= 1;
        if (Regs.CFlag)
        {
            rraVal |= 0x80;
        }
        Regs.A = rraVal;
        Regs.F = (byte)(newCF | (Regs.F & FlagsSetMask.SZPV) | (Regs.A & FlagsSetMask.R3R5));
        F53Updated = true;
    }

    /// <summary>
    /// "JR NZ,E" operation (0x20)
    /// </summary>
    /// <remarks>
    /// This instruction provides for conditional branching to other segments of a program depending on the results of
    /// a test (Z flag is not set). If the test evaluates to *true*, the value of displacement E is added to PC and
    /// the next instruction is fetched from the location designated by the new contents of the PC. The jump is
    /// measured from the address of the instruction op code and contains a range of –126 to +129 bytes. The assembler
    /// automatically adjusts for the twice incremented PC.
    /// 
    /// T-States:
    ///   Condition is met: 12 (4, 3, 5)
    ///   Condition is not met: 7 (4, 3)
    /// Contention breakdown: pc:4,pc+1:3,[pc+1:1 ×5]
    /// Gate array contention breakdown: pc:4,pc+1:3,[5]
    /// </remarks>
    private void JrNZ()
    {
        var e = ReadCodeMemory();
        if ((Regs.F & FlagsSetMask.Z) == 0)
        {
            RelativeJump(e);
        }
    }

    /// <summary>
    /// "ld hl,NN" operation (0x21)
    /// </summary>
    /// <remarks>
    /// The 16-bit integer value is loaded to the HL register pair.
    /// 
    /// T-States: 10 (4, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3
    /// </remarks>
    private void LdHLNN()
    {
        Regs.L = ReadCodeMemory();
        Regs.H = ReadCodeMemory();
    }

    /// <summary>
    /// "ld (NN),hl" operation (0x22)
    /// </summary>
    /// <remarks>
    /// The contents of the low-order portion of HL (L) are loaded to memory address (NN), and the contents of the
    /// high-order portion of HL (H) are loaded to the next highest memory address(NN + 1).
    /// 
    /// T-States: 16 (4, 3, 3, 3, 3)
    /// Contention breakdown: pc:4,pc+1:3,pc+2:3,nn:3,nn+1:3
    /// </remarks>
    private void LdNNiHL()
    {
        Store16(Regs.L, Regs.H);
    }

    /// <summary>
    /// "scf" operation (0x37)
    /// </summary>
    /// <remarks>
    /// The Carry flag in F is set.
    /// Other flags are not affected, except R5 and R3
    /// 
    /// T-States: 4
    /// Contention breakdown: pc:4
    /// </remarks>
    private void Scf()
    {
        Regs.F = (byte)((Regs.F & FlagsSetMask.SZPV) | FlagsSetMask.C);
        SetR5R3ForScfAndCcf();
    }

    /// <summary>
    /// "dec a" operation (0x3D)
    /// </summary>
    /// <remarks>
    /// Register A is decremented.
    /// S is set if result is negative; otherwise, it is reset.
    /// Z is set if result is 0; otherwise, it is reset.
    /// H is set if borrow from bit 4, otherwise, it is reset.
    /// P/V is set if m was 80h before operation; otherwise, it is reset.
    /// N is set.
    /// C is not affected.
    /// 
    /// T-States: 4
    /// </remarks>
    private void DecA()
    {
        Regs.F = (byte)(s_8BitDecFlags[Regs.A--] | (Regs.F & FlagsSetMask.C));
    }

    /// <summary>
    /// "ld a,N" operation (0x3E)
    /// </summary>
    /// <remarks>
    /// The 8-bit integer N is loaded to A.
    /// 
    /// T-States: 7 (4, 3)
    /// Contention breakdown: pc:4,pc+1:3
    /// </remarks>
    private void LdAN()
    {
        Regs.A = ReadCodeMemory();
    }
}