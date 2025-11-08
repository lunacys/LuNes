namespace LuNes;

public partial class Cpu6502
{
    /// <summary>
    /// Add with Carry In.
    /// </summary>
    /// <returns></returns>
    public byte Adc()
    {
        Fetch();
        
        ushort temp = (ushort)((ushort)A + (ushort)Fetched + (ushort)GetFlag(Flags.CarryBit));
        
        SetFlag(Flags.CarryBit, temp > 255);
        SetFlag(Flags.Zero, (temp & 0x00FF) == 0);
        SetFlag(Flags.Overflow, (byte)((~((ushort) A ^ (ushort) Fetched) & ((ushort) A ^ (ushort) temp)) & 0x0080));
        SetFlag(Flags.Negative, (byte)(temp & 0x80));

        A = (byte)(temp & 0x00FF);
        
        return 1;
    }

    /// <summary>
    /// Bitwise Logic AND.
    /// </summary>
    /// <returns></returns>
    public byte And()
    {
        Fetch();
        A = (byte)(A & Fetched);
        SetFlag(Flags.Zero, A == 0x00);
        SetFlag(Flags.Negative, (byte)(A & 0x80));

        return 1;
    }

    /// <summary>
    /// Arithmetic Shift Left.
    /// </summary>
    /// <returns></returns>
    public byte Asl()
    {
        Fetch();
        ushort temp = (ushort)((ushort)Fetched << 1);
        SetFlag(Flags.CarryBit, (temp & 0xFF00) > 0);
        SetFlag(Flags.Zero, (temp & 0x00FF) == 0x00);
        SetFlag(Flags.Negative, temp & 0x80);
        if (Lookup[Opcode].AddressMode == Imp)
            A = (byte)(temp & 0x00FF);
        else
            Write(AddressAbsolute, (byte)(temp & 0x00FF));
        return 0;
    }

    /// <summary>
    /// Branch if Carry Clear.
    /// </summary>
    /// <returns></returns>
    public byte Bcc()
    {
        if (GetFlag(Flags.CarryBit) == 0)
        {
            Cycles++;
            AddressAbsolute = (ushort)(Pc + AddressRelative);

            if ((AddressAbsolute & 0xFF00) != (Pc & 0xFF00))
                Cycles++;

            Pc = AddressAbsolute;
        }

        return 0;
    }

    /// <summary>
    /// Branch if Carry Set.
    /// </summary>
    /// <returns></returns>
    public byte Bcs()
    {
        if (GetFlag(Flags.CarryBit) == 1)
        {
            Cycles++;
            AddressAbsolute = (ushort)(Pc + AddressRelative);

            if ((AddressAbsolute & 0xFF00) != (Pc & 0xFF00))
                Cycles++;

            Pc = AddressAbsolute;
        }

        return 0;
    }

    /// <summary>
    /// Branch if Equal.
    /// </summary>
    /// <returns></returns>
    public byte Beq()
    {
        if (GetFlag(Flags.Zero) == 1)
        {
            Cycles++;
            AddressAbsolute = (ushort)(Pc + AddressRelative);

            if ((AddressAbsolute & 0xFF00) != (Pc & 0xFF00))
                Cycles++;

            Pc = AddressAbsolute;
        }

        return 0;
    }

    /// <summary>
    /// Bit Test. Modifies flags, but doesn't change memory or registers.
    /// </summary>
    /// <returns></returns>
    public byte Bit()
    {
        Fetch();
        ushort temp = (ushort)(A & Fetched);
        SetFlag(Flags.Zero, (temp & 0x00FF) == 0x00);
        SetFlag(Flags.Negative, Fetched & (1 << 7));
        SetFlag(Flags.Overflow, Fetched & (1 << 6));
        return 0;
    }

    /// <summary>
    /// Branch if Negative.
    /// </summary>
    /// <returns></returns>
    public byte Bmi()
    {
        if (GetFlag(Flags.Negative) == 1)
        {
            Cycles++;
            AddressAbsolute = (ushort)(Pc + AddressRelative);

            if ((AddressAbsolute & 0xFF00) != (Pc & 0xFF00))
                Cycles++;

            Pc = AddressAbsolute;
        }

        return 0;
    }

    /// <summary>
    /// Branch if Not Equal.
    /// </summary>
    /// <returns></returns>
    public byte Bne()
    {
        if (GetFlag(Flags.Zero) == 0)
        {
            Cycles++;
            AddressAbsolute = (ushort)(Pc + AddressRelative);

            if ((AddressAbsolute & 0xFF00) != (Pc & 0xFF00))
                Cycles++;

            Pc = AddressAbsolute;
        }

        return 0;
    }

    /// <summary>
    /// Branch if Positive.
    /// </summary>
    /// <returns></returns>
    public byte Bpl()
    {
        if (GetFlag(Flags.Negative) == 0)
        {
            Cycles++;
            AddressAbsolute = (ushort)(Pc + AddressRelative);

            if ((AddressAbsolute & 0xFF00) != (Pc & 0xFF00))
                Cycles++;

            Pc = AddressAbsolute;
        }

        return 0;
    }

    /// <summary>
    /// Break. Program sourced interrupt.
    /// </summary>
    /// <returns></returns>
    public byte Brk()
    {
        Pc++;
        
        SetFlag(Flags.DisableInterrupts, 1);
        Write((ushort)(0x0100 + Stkp), (byte)((Pc >> 8) & 0x00FF));
        Stkp--;
        Write(0x0100 + Stkp, (byte)(Pc & 0x00FF));
        Stkp--;
        
        SetFlag(Flags.Break, 1);
        Write(0x0100 + Stkp, (byte)Status);
        Stkp--;
        SetFlag(Flags.Break, 0);

        Pc = (ushort)((ushort)Read(0xFFFE) | ((ushort)Read(0xFFFF) << 8));
        return 0;
    }

    /// <summary>
    /// Branch if Overflow Clear.
    /// </summary>
    /// <returns></returns>
    public byte Bvc()
    {
        if (GetFlag(Flags.Overflow) == 0)
        {
            Cycles++;
            AddressAbsolute = (ushort)(Pc + AddressRelative);

            if ((AddressAbsolute & 0xFF00) != (Pc & 0xFF00))
                Cycles++;

            Pc = AddressAbsolute;
        }

        return 0;
    }

    /// <summary>
    /// Branch if Overflow Set.
    /// </summary>
    /// <returns></returns>
    public byte Bvs()
    {
        if (GetFlag(Flags.Overflow) == 1)
        {
            Cycles++;
            AddressAbsolute = (ushort)(Pc + AddressRelative);

            if ((AddressAbsolute & 0xFF00) != (Pc & 0xFF00))
                Cycles++;

            Pc = AddressAbsolute;
        }

        return 0;
    }

    /// <summary>
    /// Clear Carry Flag.
    /// </summary>
    /// <returns></returns>
    public byte Clc()
    {
        SetFlag(Flags.CarryBit, false);
        return 0;
    }

    /// <summary>
    /// Clear Decimal Flag.
    /// </summary>
    /// <returns></returns>
    public byte Cld()
    {
        SetFlag(Flags.DecimalMode, false);
        return 0;
    }

    /// <summary>
    /// Disable Interrupts by Clearing Interrupt Flag.
    /// </summary>
    /// <returns></returns>
    public byte Cli()
    {
        SetFlag(Flags.DisableInterrupts, false);
        return 0;
    }

    /// <summary>
    /// Clear Overflow Flag.
    /// </summary>
    /// <returns></returns>
    public byte Clv()
    {
        SetFlag(Flags.Overflow, false);
        return 0;
    }

    /// <summary>
    /// Compare Accumulator.
    /// </summary>
    /// <returns></returns>
    public byte Cmp()
    {
        Fetch();
        ushort temp = (ushort)((ushort)A - (ushort)Fetched);
        SetFlag(Flags.CarryBit, A >= Fetched);
        SetFlag(Flags.Zero, (temp & 0x00FF) == 0x0000);
        SetFlag(Flags.Negative, temp & 0x0080);
        return 1;
    }

    /// <summary>
    /// Compare X Register.
    /// </summary>
    /// <returns></returns>
    public byte Cpx()
    {
        Fetch();
        ushort temp = (ushort)((ushort)X - (ushort)Fetched);
        SetFlag(Flags.CarryBit, X >= Fetched);
        SetFlag(Flags.Zero, (temp & 0x00FF) == 0x0000);
        SetFlag(Flags.Negative, temp & 0x0080);
        return 1;
    }

    /// <summary>
    /// Compare Y Register.
    /// </summary>
    /// <returns></returns>
    public byte Cpy()
    {
        Fetch();
        ushort temp = (ushort)((ushort)Y - (ushort)Fetched);
        SetFlag(Flags.CarryBit, Y >= Fetched);
        SetFlag(Flags.Zero, (temp & 0x00FF) == 0x0000);
        SetFlag(Flags.Negative, temp & 0x0080);
        return 1;
    }

    /// <summary>
    /// Decrement Value at Memory Location.
    /// </summary>
    /// <returns></returns>
    public byte Dec()
    {
        Fetch();
        ushort temp = (ushort)(Fetched - 1);
        Write(AddressAbsolute, (byte)(temp & 0x00FF));
        SetFlag(Flags.Zero, (temp & 0x00FF) == 0x0000);
        SetFlag(Flags.Negative, temp & 0x0080);
        return 0;
    }

    /// <summary>
    /// Decrement X Register.
    /// </summary>
    /// <returns></returns>
    public byte Dex()
    {
        X--;
        SetFlag(Flags.Zero, X == 0x00);
        SetFlag(Flags.Negative, X & 0x80);
        return 0;
    }

    /// <summary>
    /// Decrement Y Register.
    /// </summary>
    /// <returns></returns>
    public byte Dey()
    {
        Y--;
        SetFlag(Flags.Zero, Y == 0x00);
        SetFlag(Flags.Negative, Y & 0x80);
        return 0;
    }

    /// <summary>
    /// Bitwise Logic XOR.
    /// </summary>
    /// <returns></returns>
    public byte Eor()
    {
        Fetch();
        A = (byte)(A ^ Fetched);
        SetFlag(Flags.Zero, A == 0x00);
        SetFlag(Flags.Negative, A & 0x80);
        return 1;
    }

    /// <summary>
    /// Increment Value at Memory Location.
    /// </summary>
    /// <returns></returns>
    public byte Inc()
    {
        Fetch();
        ushort temp = (ushort)(Fetched + 1);
        Write(AddressAbsolute, temp & 0x00FF);
        SetFlag(Flags.Zero, (temp & 0x00FF) == 0x0000);
        SetFlag(Flags.Negative, temp & 0x0080);
        return 0;
    }

    /// <summary>
    /// Increment X Register.
    /// </summary>
    /// <returns></returns>
    public byte Inx()
    {
        X++;
        SetFlag(Flags.Zero, X == 0x00);
        SetFlag(Flags.Negative, X & 0x80);
        return 0;
    }

    /// <summary>
    /// Increment Y Register.
    /// </summary>
    /// <returns></returns>
    public byte Iny()
    {
        Y++;
        SetFlag(Flags.Zero, Y == 0x00);
        SetFlag(Flags.Negative, Y & 0x80);
        return 0;
    }

    /// <summary>
    /// Jump to Location.
    /// </summary>
    /// <returns></returns>
    public byte Jmp()
    {
        Pc = AddressAbsolute;
        return 0;
    }

    /// <summary>
    /// Jump to Subroutine.
    /// </summary>
    /// <returns></returns>
    public byte Jsr()
    {
        Pc--;
        
        Write(0x0100 + Stkp, (Pc >> 8) & 0x00FF);
        Stkp--;
        Write(0x0100 + Stkp, Pc & 0x00FF);
        Stkp--;

        Pc = AddressAbsolute;
        return 0;
    }

    /// <summary>
    /// Load the Accumulator.
    /// </summary>
    /// <returns></returns>
    public byte Lda()
    {
        Fetch();
        A = Fetched;
        SetFlag(Flags.Zero, A == 0x00);
        SetFlag(Flags.Negative, A & 0x80);
        return 1;
    }

    /// <summary>
    /// Load the X Register.
    /// </summary>
    /// <returns></returns>
    public byte Ldx()
    {
        Fetch();
        X = Fetched;
        SetFlag(Flags.Zero, X == 0x00);
        SetFlag(Flags.Negative, X & 0x80);
        return 1;
    }

    /// <summary>
    /// Load the Y Register.
    /// </summary>
    /// <returns></returns>
    public byte Ldy()
    {
        Fetch();
        Y = Fetched;
        SetFlag(Flags.Zero, Y == 0x00);
        SetFlag(Flags.Negative, Y & 0x80);
        return 1;
    }

    /// <summary>
    /// Bitwise Logic Shift Right.
    /// </summary>
    /// <returns></returns>
    public byte Lsr()
    {
        Fetch();
        SetFlag(Flags.CarryBit, Fetched & 0x0001);
        ushort temp = (ushort)(Fetched >> 1);
        SetFlag(Flags.Zero, (temp & 0x00FF) == 0x0000);
        SetFlag(Flags.Negative, temp & 0x0080);
        if (Lookup[Opcode].AddressMode == Imp)
            A = (byte)(temp & 0x00FF);
        else 
            Write(AddressAbsolute, temp & 0x00FF);

        return 0;
    }

    /// <summary>
    /// No Instruction.
    /// </summary>
    /// <returns></returns>
    public byte Nop()
    {
        switch (Opcode)
        {
            case 0x1C:
            case 0x3C:
            case 0x5C:
            case 0x7C:
            case 0xDC:
            case 0xFC:
                return 1;
        }

        return 0;
    }

    /// <summary>
    /// Bitwise Logic OR.
    /// </summary>
    /// <returns></returns>
    public byte Ora()
    {
        Fetch();
        A = (byte)(A | Fetched);
        SetFlag(Flags.Zero, A == 0x00);
        SetFlag(Flags.Negative, A & 0x80);
        return 1;
    }

    /// <summary>
    /// Push Accumulator to Stack.
    /// </summary>
    /// <returns></returns>
    public byte Pha()
    {
        Write(0x0100 + Stkp, A);
        Stkp--;
        return 0;
    }

    /// <summary>
    /// Push Status Register to Stack.
    /// </summary>
    /// <returns></returns>
    public byte Php()
    {
        Write(0x0100 + Stkp, (byte)(Status | Flags.Break | Flags.Unused));
        SetFlag(Flags.Break, 0);
        SetFlag(Flags.Unused, 0);
        Stkp--;
        return 0;
    }

    /// <summary>
    /// Pop Accumulator off Stack.
    /// </summary>
    /// <returns></returns>
    public byte Pla()
    {
        Stkp++;
        A = Read(0x0100 + Stkp);
        SetFlag(Flags.Zero, A == 0x00);
        SetFlag(Flags.Negative, A & 0x80);
        return 0;
    }

    /// <summary>
    /// Pop Status Register off Stack. 
    /// </summary>
    /// <returns></returns>
    public byte Plp()
    {
        Stkp++;
        Status = (Flags)Read(0x0100 + Stkp);
        SetFlag(Flags.Unused, 1);
        return 0;
    }

    /// <summary>
    /// Rotate Left.
    /// </summary>
    /// <returns></returns>
    public byte Rol()
    {
        Fetch();
        ushort temp = (ushort)((ushort)(Fetched << 1) | GetFlag(Flags.CarryBit));
        SetFlag(Flags.CarryBit, temp & 0xFF00);
        SetFlag(Flags.Zero, (temp & 0x00FF) == 0x0000);
        SetFlag(Flags.Negative, temp & 0x0080);
        if (Lookup[Opcode].AddressMode == Imp)
            A = (byte)(temp & 0x00FF);
        else 
            Write(AddressAbsolute, temp & 0x00FF);
        return 0;
    }

    /// <summary>
    /// Rotate Right.
    /// </summary>
    /// <returns></returns>
    public byte Ror()
    {
        Fetch();
        ushort temp = (ushort)((ushort)(GetFlag(Flags.CarryBit) << 7) | Fetched >> 1);
        SetFlag(Flags.CarryBit, Fetched & 0x01);
        SetFlag(Flags.Zero, (temp & 0x00FF) == 0x0000);
        SetFlag(Flags.Negative, temp & 0x0080);
        if (Lookup[Opcode].AddressMode == Imp)
            A = (byte)(temp & 0x00FF);
        else
            Write(AddressAbsolute, temp & 0x00FF);

        return 0;
    }

    /// <summary>
    /// Return from Interrupt.
    /// </summary>
    /// <returns></returns>
    public byte Rti()
    {
        Stkp++;
        Status = (Flags)Read(0x0100 + Stkp);
        Status &= ~Flags.Break;
        Status &= ~Flags.Unused;

        Stkp++;
        Pc = (ushort)Read(0x0100 + Stkp);
        Stkp++;
        Pc = (ushort)(Pc | ((ushort)Read(0x0100 + Stkp) << 8));

        return 0;
    }

    /// <summary>
    /// Return from Subroutine.
    /// </summary>
    /// <returns></returns>
    public byte Rts()
    {
        Stkp++;
        Pc = (ushort)Read(0x0100 + Stkp);
        Stkp++;
        Pc = (ushort)(Pc | ((ushort)Read(0x0100 + Stkp) << 8));

        Pc++;
        return 0;
    }

    /// <summary>
    /// Subtract with Carry.
    /// </summary>
    /// <returns></returns>
    public byte Sbc()
    {
        Fetch();

        ushort value = (ushort)(((ushort)Fetched) ^ 0x00FF);

        ushort temp = (ushort)((ushort)A + value + (ushort)GetFlag(Flags.CarryBit));
        SetFlag(Flags.CarryBit, (byte)(temp & 0xFF00));
        SetFlag(Flags.Zero, ((temp & 0x00FF) == 0));
        SetFlag(Flags.Overflow, (byte)((temp ^ (ushort)A) & (temp ^ value) & 0x0080));
        SetFlag(Flags.Negative, (byte)(temp & 0x0080));
        A = (byte)(temp & 0x00FF);
        return 1;
    }

    /// <summary>
    /// Set Carry Bit.
    /// </summary>
    /// <returns></returns>
    public byte Sec()
    {
        SetFlag(Flags.CarryBit, true);
        return 0;
    }

    /// <summary>
    /// Set Decimal Mode.
    /// </summary>
    /// <returns></returns>
    public byte Sed()
    {
        SetFlag(Flags.DecimalMode, true);
        return 0;
    }

    /// <summary>
    /// Enable Interrupts.
    /// </summary>
    /// <returns></returns>
    public byte Sei()
    {
        SetFlag(Flags.DisableInterrupts, true);
        return 0;
    }

    /// <summary>
    /// Store Accumulator at Address.
    /// </summary>
    /// <returns></returns>
    public byte Sta()
    {
        Write(AddressAbsolute, A);
        return 0;
    }

    /// <summary>
    /// Store X Register at Address.
    /// </summary>
    /// <returns></returns>
    public byte Stx()
    {
        Write(AddressAbsolute, X);
        return 0;
    }

    /// <summary>
    /// Store Y Register at Address.
    /// </summary>
    /// <returns></returns>
    public byte Sty()
    {
        Write(AddressAbsolute, Y);
        return 0;
    }

    /// <summary>
    /// Transfer Accumulator to X Register.
    /// </summary>
    /// <returns></returns>
    public byte Tax()
    {
        X = A;
        SetFlag(Flags.Zero, X == 0x00);
        SetFlag(Flags.Negative, X & 0x80);
        return 0;
    }

    /// <summary>
    /// Transfer Accumulator to Y Register.
    /// </summary>
    /// <returns></returns>
    public byte Tay()
    {
        Y = A;
        SetFlag(Flags.Zero, Y == 0x00);
        SetFlag(Flags.Negative, Y & 0x80);
        return 0;
    }

    /// <summary>
    /// Transfer Stack Pointer to X Register.
    /// </summary>
    /// <returns></returns>
    public byte Tsx()
    {
        X = Stkp;
        SetFlag(Flags.Zero, X == 0x00);
        SetFlag(Flags.Negative, X & 0x80);
        return 0;
    }

    /// <summary>
    /// Transfer X Register to Accumulator.
    /// </summary>
    /// <returns></returns>
    public byte Txa()
    {
        A = Stkp;
        SetFlag(Flags.Zero, A == 0x00);
        SetFlag(Flags.Negative, A & 0x80);
        return 0;
    }

    /// <summary>
    /// Transfer X Register to Stack Pointer.
    /// </summary>
    /// <returns></returns>
    public byte Txs()
    {
        Stkp = X;
        return 0;
    }

    /// <summary>
    /// Transfer Y Register to Accumulator.
    /// </summary>
    /// <returns></returns>
    public byte Tya()
    {
        A = Y;
        SetFlag(Flags.Zero, A == 0x00);
        SetFlag(Flags.Negative, A & 0x80);
        return 0;
    }

    /// <summary>
    /// Illegal opcode.
    /// </summary>
    public byte Xxx()
    {
        return 0;
    }
}