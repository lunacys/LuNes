namespace LuNes.Cpu;

public partial class Cpu6502
{
    public readonly List<Instruction> Lookup;

    public Cpu6502()
    {
        Lookup = new List<Instruction>(256);
        
        Lookup.AddRange([
            /* 0x00 */ I("BRK", Brk, Imm, 7), I("ORA", Ora, Izx, 6), I("???", Xxx, Imp, 2), I("???", Xxx, Imp, 8),
            /* 0x04 */ I("???", Nop, Imp, 3), I("ORA", Ora, Zp0, 3), I("ASL", Asl, Zp0, 5), I("???", Xxx, Imp, 5),
            /* 0x08 */ I("PHP", Php, Imp, 3), I("ORA", Ora, Imm, 2), I("ASL", Asl, Imp, 2), I("???", Xxx, Imp, 2),
            /* 0x0C */ I("???", Nop, Imp, 4), I("ORA", Ora, Abs, 4), I("ASL", Asl, Abs, 6), I("???", Xxx, Imp, 6),

            /* 0x10 */ I("BPL", Bpl, Rel, 2), I("ORA", Ora, Izy, 5), I("???", Xxx, Imp, 2), I("???", Xxx, Imp, 8),
            /* 0x14 */ I("???", Nop, Imp, 4), I("ORA", Ora, Zpx, 4), I("ASL", Asl, Zpx, 6), I("???", Xxx, Imp, 6),
            /* 0x18 */ I("CLC", Clc, Imp, 2), I("ORA", Ora, Aby, 4), I("???", Nop, Imp, 2), I("???", Xxx, Imp, 7),
            /* 0x1C */ I("???", Nop, Imp, 4), I("ORA", Ora, Abx, 4), I("ASL", Asl, Abx, 7), I("???", Xxx, Imp, 7),

            /* 0x20 */ I("JSR", Jsr, Abs, 6), I("AND", And, Izx, 6), I("???", Xxx, Imp, 2), I("???", Xxx, Imp, 8),
            /* 0x24 */ I("BIT", Bit, Zp0, 3), I("AND", And, Zp0, 3), I("ROL", Rol, Zp0, 5), I("???", Xxx, Imp, 5),
            /* 0x28 */ I("PLP", Plp, Imp, 4), I("AND", And, Imm, 2), I("ROL", Rol, Imp, 2), I("???", Xxx, Imp, 2),
            /* 0x2C */ I("BIT", Bit, Abs, 4), I("AND", And, Abs, 4), I("ROL", Rol, Abs, 6), I("???", Xxx, Imp, 6),

            /* 0x30 */ I("BMI", Bmi, Rel, 2), I("AND", And, Izy, 5), I("???", Xxx, Imp, 2), I("???", Xxx, Imp, 8),
            /* 0x34 */ I("???", Nop, Imp, 4), I("AND", And, Zpx, 4), I("ROL", Rol, Zpx, 6), I("???", Xxx, Imp, 6),
            /* 0x38 */ I("SEC", Sec, Imp, 2), I("AND", And, Aby, 4), I("???", Nop, Imp, 2), I("???", Xxx, Imp, 7),
            /* 0x3C */ I("???", Nop, Imp, 4), I("AND", And, Abx, 4), I("ROL", Rol, Abx, 7), I("???", Xxx, Imp, 7),

            /* 0x40 */ I("RTI", Rti, Imp, 6), I("EOR", Eor, Izx, 6), I("???", Xxx, Imp, 2), I("???", Xxx, Imp, 8),
            /* 0x44 */ I("???", Nop, Imp, 3), I("EOR", Eor, Zp0, 3), I("LSR", Lsr, Zp0, 5), I("???", Xxx, Imp, 5),
            /* 0x48 */ I("PHA", Pha, Imp, 3), I("EOR", Eor, Imm, 2), I("LSR", Lsr, Imp, 2), I("???", Xxx, Imp, 2),
            /* 0x4C */ I("JMP", Jmp, Abs, 3), I("EOR", Eor, Abs, 4), I("LSR", Lsr, Abs, 6), I("???", Xxx, Imp, 6),

            /* 0x50 */ I("BVC", Bvc, Rel, 2), I("EOR", Eor, Izy, 5), I("???", Xxx, Imp, 2), I("???", Xxx, Imp, 8),
            /* 0x54 */ I("???", Nop, Imp, 4), I("EOR", Eor, Zpx, 4), I("LSR", Lsr, Zpx, 6), I("???", Xxx, Imp, 6),
            /* 0x58 */ I("CLI", Cli, Imp, 2), I("EOR", Eor, Aby, 4), I("???", Nop, Imp, 2), I("???", Xxx, Imp, 7),
            /* 0x5C */ I("???", Nop, Imp, 4), I("EOR", Eor, Abx, 4), I("LSR", Lsr, Abx, 7), I("???", Xxx, Imp, 7),

            /* 0x60 */ I("RTS", Rts, Imp, 6), I("ADC", Adc, Izx, 6), I("???", Xxx, Imp, 2), I("???", Xxx, Imp, 8),
            /* 0x64 */ I("???", Nop, Imp, 3), I("ADC", Adc, Zp0, 3), I("ROR", Ror, Zp0, 5), I("???", Xxx, Imp, 5),
            /* 0x68 */ I("PLA", Pla, Imp, 4), I("ADC", Adc, Imm, 2), I("ROR", Ror, Imp, 2), I("???", Xxx, Imp, 2),
            /* 0x6C */ I("JMP", Jmp, Ind, 5), I("ADC", Adc, Abs, 4), I("ROR", Ror, Abs, 6), I("???", Xxx, Imp, 6),

            /* 0x70 */ I("BVS", Bvs, Rel, 2), I("ADC", Adc, Izy, 5), I("???", Xxx, Imp, 2), I("???", Xxx, Imp, 8),
            /* 0x74 */ I("???", Nop, Imp, 4), I("ADC", Adc, Zpx, 4), I("ROR", Ror, Zpx, 6), I("???", Xxx, Imp, 6),
            /* 0x78 */ I("SEI", Sei, Imp, 2), I("ADC", Adc, Aby, 4), I("???", Nop, Imp, 2), I("???", Xxx, Imp, 7),
            /* 0x7C */ I("???", Nop, Imp, 4), I("ADC", Adc, Abx, 4), I("ROR", Ror, Abx, 7), I("???", Xxx, Imp, 7),

            /* 0x80 */ I("???", Nop, Imp, 2), I("STA", Sta, Izx, 6), I("???", Nop, Imp, 2), I("???", Xxx, Imp, 6),
            /* 0x84 */ I("STY", Sty, Zp0, 3), I("STA", Sta, Zp0, 3), I("STX", Stx, Zp0, 3), I("???", Xxx, Imp, 3),
            /* 0x88 */ I("DEY", Dey, Imp, 2), I("???", Nop, Imp, 2), I("TXA", Txa, Imp, 2), I("???", Xxx, Imp, 2),
            /* 0x8C */ I("STY", Sty, Abs, 4), I("STA", Sta, Abs, 4), I("STX", Stx, Abs, 4), I("???", Xxx, Imp, 4),

            /* 0x90 */ I("BCC", Bcc, Rel, 2), I("STA", Sta, Izy, 6), I("???", Xxx, Imp, 2), I("???", Xxx, Imp, 6),
            /* 0x94 */ I("STY", Sty, Zpx, 4), I("STA", Sta, Zpx, 4), I("STX", Stx, Zpy, 4), I("???", Xxx, Imp, 4),
            /* 0x98 */ I("TYA", Tya, Imp, 2), I("STA", Sta, Aby, 5), I("TXS", Txs, Imp, 2), I("???", Xxx, Imp, 5),
            /* 0x9C */ I("???", Nop, Imp, 5), I("STA", Sta, Abx, 5), I("???", Xxx, Imp, 5), I("???", Xxx, Imp, 5),

            /* 0xA0 */ I("LDY", Ldy, Imm, 2), I("LDA", Lda, Izx, 6), I("LDX", Ldx, Imm, 2), I("???", Xxx, Imp, 6),
            /* 0xA4 */ I("LDY", Ldy, Zp0, 3), I("LDA", Lda, Zp0, 3), I("LDX", Ldx, Zp0, 3), I("???", Xxx, Imp, 3),
            /* 0xA8 */ I("TAY", Tay, Imp, 2), I("LDA", Lda, Imm, 2), I("TAX", Tax, Imp, 2), I("???", Xxx, Imp, 2),
            /* 0xAC */ I("LDY", Ldy, Abs, 4), I("LDA", Lda, Abs, 4), I("LDX", Ldx, Abs, 4), I("???", Xxx, Imp, 4),

            /* 0xB0 */ I("BCS", Bcs, Rel, 2), I("LDA", Lda, Izy, 5), I("???", Xxx, Imp, 2), I("???", Xxx, Imp, 5),
            /* 0xB4 */ I("LDY", Ldy, Zpx, 4), I("LDA", Lda, Zpx, 4), I("LDX", Ldx, Zpy, 4), I("???", Xxx, Imp, 4),
            /* 0xB8 */ I("CLV", Clv, Imp, 2), I("LDA", Lda, Aby, 4), I("TSX", Tsx, Imp, 2), I("???", Xxx, Imp, 4),
            /* 0xBC */ I("LDY", Ldy, Abx, 4), I("LDA", Lda, Abx, 4), I("LDX", Ldx, Aby, 4), I("???", Xxx, Imp, 4),

            /* 0xC0 */ I("CPY", Cpy, Imm, 2), I("CMP", Cmp, Izx, 6), I("???", Nop, Imp, 2), I("???", Xxx, Imp, 8),
            /* 0xC4 */ I("CPY", Cpy, Zp0, 3), I("CMP", Cmp, Zp0, 3), I("DEC", Dec, Zp0, 5), I("???", Xxx, Imp, 5),
            /* 0xC8 */ I("INY", Iny, Imp, 2), I("CMP", Cmp, Imm, 2), I("DEX", Dex, Imp, 2), I("???", Xxx, Imp, 2),
            /* 0xCC */ I("CPY", Cpy, Abs, 4), I("CMP", Cmp, Abs, 4), I("DEC", Dec, Abs, 6), I("???", Xxx, Imp, 6),

            /* 0xD0 */ I("BNE", Bne, Rel, 2), I("CMP", Cmp, Izy, 5), I("???", Xxx, Imp, 2), I("???", Xxx, Imp, 8),
            /* 0xD4 */ I("???", Nop, Imp, 4), I("CMP", Cmp, Zpx, 4), I("DEC", Dec, Zpx, 6), I("???", Xxx, Imp, 6),
            /* 0xD8 */ I("CLD", Cld, Imp, 2), I("CMP", Cmp, Aby, 4), I("NOP", Nop, Imp, 2), I("???", Xxx, Imp, 7),
            /* 0xDC */ I("???", Nop, Imp, 4), I("CMP", Cmp, Abx, 4), I("DEC", Dec, Abx, 7), I("???", Xxx, Imp, 7),

            /* 0xE0 */ I("CPX", Cpx, Imm, 2), I("SBC", Sbc, Izx, 6), I("???", Nop, Imp, 2), I("???", Xxx, Imp, 8),
            /* 0xE4 */ I("CPX", Cpx, Zp0, 3), I("SBC", Sbc, Zp0, 3), I("INC", Inc, Zp0, 5), I("???", Xxx, Imp, 5),
            /* 0xE8 */ I("INX", Inx, Imp, 2), I("SBC", Sbc, Imm, 2), I("NOP", Nop, Imp, 2), I("???", Sbc, Imp, 2),
            /* 0xEC */ I("CPX", Cpx, Abs, 4), I("SBC", Sbc, Abs, 4), I("INC", Inc, Abs, 6), I("???", Xxx, Imp, 6),

            /* 0xF0 */ I("BEQ", Beq, Rel, 2), I("SBC", Sbc, Izy, 5), I("???", Xxx, Imp, 2), I("???", Xxx, Imp, 8),
            /* 0xF4 */ I("???", Nop, Imp, 4), I("SBC", Sbc, Zpx, 4), I("INC", Inc, Zpx, 6), I("???", Xxx, Imp, 6),
            /* 0xF8 */ I("SED", Sed, Imp, 2), I("SBC", Sbc, Aby, 4), I("NOP", Nop, Imp, 2), I("???", Xxx, Imp, 7),
            /* 0xFC */ I("???", Nop, Imp, 4), I("SBC", Sbc, Abx, 4), I("INC", Inc, Abx, 7), I("???", Xxx, Imp, 7)
        ]);
        
        Initialize65C02Extensions();
    }
    
    private void Initialize65C02Extensions()
    {
        // BRA - Branch Always
        Lookup[0x80] = I("BRA", Bra, Rel, 2);
    
        // STZ - Store Zero
        Lookup[0x64] = I("STZ", Stz, Zp0, 3);
        Lookup[0x74] = I("STZ", Stz, Zpx, 4);
        Lookup[0x9C] = I("STZ", Stz, Abs, 4);
        Lookup[0x9E] = I("STZ", Stz, Abx, 5);
    
        // PHX/PLX/PHY/PLY
        Lookup[0xDA] = I("PHX", Phx, Imp, 3);
        Lookup[0xFA] = I("PLX", Plx, Imp, 4);
        Lookup[0x5A] = I("PHY", Phy, Imp, 3);
        Lookup[0x7A] = I("PLY", Ply, Imp, 4);
    
        // Additional NOPs
        Lookup[0x1A] = I("NOP", Nop, Imp, 2);
        Lookup[0x3A] = I("NOP", Nop, Imp, 2);
        //Lookup[0x5A] = I("NOP", Nop, Imp, 2); // PHY
        //Lookup[0x7A] = I("NOP", Nop, Imp, 2); // PLY
        //Lookup[0xDA] = I("NOP", Nop, Imp, 2); // PHX
        //Lookup[0xFA] = I("NOP", Nop, Imp, 2); // PLX
    
        // BIT immediate
        Lookup[0x89] = I("BIT", BitImm, Imm, 2);
    
        // WAI - Wait for Interrupt
        Lookup[0xCB] = I("WAI", Wai, Imp, 3);
    
        // STP - Stop
        Lookup[0xDB] = I("STP", Stp, Imp, 3);
    }

    private Instruction I(string name, Operate operation, AddressMode addressMode, byte cycles)
        => new (name, operation, addressMode, cycles, GetAddressModeEnum(addressMode));

    private AddressModeEnum GetAddressModeEnum(AddressMode mode)
    {
        if (mode == Imp) return AddressModeEnum.Imp;
        if (mode == Imm) return AddressModeEnum.Imm;
        if (mode == Zp0) return AddressModeEnum.Zp0;
        if (mode == Zpx) return AddressModeEnum.Zpx;
        if (mode == Zpy) return AddressModeEnum.Zpy;
        if (mode == Rel) return AddressModeEnum.Rel;
        if (mode == Abs) return AddressModeEnum.Abs;
        if (mode == Abx) return AddressModeEnum.Abx;
        if (mode == Aby) return AddressModeEnum.Aby;
        if (mode == Ind) return AddressModeEnum.Ind;
        if (mode == Izx) return AddressModeEnum.Izx;
        if (mode == Izy) return AddressModeEnum.Izy;
        return AddressModeEnum.Unknown;
    }
}