namespace LuNes.Cpu;

public partial class Cpu6502
{
    public readonly struct DisassembledInstruction(
        ushort address,
        byte[] bytes,
        string label,
        string instructionText,
        string addressingMode,
        bool hasLabel)
    {
        public readonly ushort Address = address;
        public readonly byte[] Bytes = bytes;
        public readonly string Label = label;
        public readonly string InstructionText = instructionText;
        public readonly string AddressingMode = addressingMode;
        public readonly bool HasLabel = hasLabel;

        public override string ToString()
        {
            return $"{InstructionText} {{{AddressingMode}}}";
        }
    }

    public Dictionary<ushort, DisassembledInstruction> DisassembleDetailed(ushort start, ushort stop)
    {
        var mapLines = new Dictionary<ushort, DisassembledInstruction>();
        int addr = start;

        // First pass: collect all branch/jump targets as labels
        var labelAddresses = new HashSet<ushort>();
        while (addr <= stop)
        {
            byte opcode = _bus?.CpuRead(addr, true) ?? 0;
            var instruction = Lookup[opcode];

            // Get instruction length
            int length = GetInstructionLength(opcode);

            // Check if this is a branch or jump instruction
            if (instruction.AddressMode == Rel || instruction.Name == "JMP" || instruction.Name == "JSR")
            {
                ushort targetAddr = 0;

                if (instruction.AddressMode == Rel)
                {
                    // Relative branch
                    byte offset = _bus?.CpuRead((ushort)(addr + 1), true) ?? 0;
                    short signedOffset = (sbyte)offset; // Sign extend
                    targetAddr = (ushort)(addr + 2 + signedOffset);
                }
                else if (instruction.Name == "JMP" || instruction.Name == "JSR")
                {
                    // Absolute or indirect jump
                    byte lo = _bus?.CpuRead((ushort)(addr + 1), true) ?? 0;
                    byte hi = _bus?.CpuRead((ushort)(addr + 2), true) ?? 0;
                    targetAddr = (ushort)((hi << 8) | lo);
                }

                if (targetAddr >= start && targetAddr <= stop)
                {
                    labelAddresses.Add(targetAddr);
                }
            }

            addr += (ushort)length;
        }

        // Second pass: disassemble with labels
        addr = start;
        while (addr <= stop)
        {
            ushort lineAddr = (ushort)addr;

            byte opcode = _bus?.CpuRead(addr, true) ?? 0;
            addr++;

            var instruction = Lookup[opcode];
            var bytes = new List<byte> { opcode };
            string addressingMode = instruction.GetAddressModeName();

            string operandText = "";

            // Handle different addressing modes
            if (instruction.AddressMode == Imp)
            {
                operandText = "";
            }
            else if (instruction.AddressMode == Imm)
            {
                byte value = _bus?.CpuRead((ushort)addr, true) ?? 0;
                addr++;
                bytes.Add(value);
                operandText = $"#${value:X2}";
            }
            else if (instruction.AddressMode == Zp0)
            {
                byte value = _bus?.CpuRead((ushort)addr, true) ?? 0;
                addr++;
                bytes.Add(value);
                operandText = $"${value:X2}";
            }
            else if (instruction.AddressMode == Zpx)
            {
                byte value = _bus?.CpuRead((ushort)addr, true) ?? 0;
                addr++;
                bytes.Add(value);
                operandText = $"${value:X2},X";
            }
            else if (instruction.AddressMode == Zpy)
            {
                byte value = _bus?.CpuRead((ushort)addr, true) ?? 0;
                addr++;
                bytes.Add(value);
                operandText = $"${value:X2},Y";
            }
            else if (instruction.AddressMode == Abs)
            {
                byte lo = _bus?.CpuRead((ushort)addr, true) ?? 0;
                addr++;
                byte hi = _bus?.CpuRead((ushort)addr, true) ?? 0;
                addr++;
                bytes.Add(lo);
                bytes.Add(hi);
                ushort address = (ushort)((hi << 8) | lo);
                operandText = $"${address:X4}";
            }
            else if (instruction.AddressMode == Abx)
            {
                byte lo = _bus?.CpuRead((ushort)addr, true) ?? 0;
                addr++;
                byte hi = _bus?.CpuRead((ushort)addr, true) ?? 0;
                addr++;
                bytes.Add(lo);
                bytes.Add(hi);
                ushort address = (ushort)((hi << 8) | lo);
                operandText = $"${address:X4},X";
            }
            else if (instruction.AddressMode == Aby)
            {
                byte lo = _bus?.CpuRead((ushort)addr, true) ?? 0;
                addr++;
                byte hi = _bus?.CpuRead((ushort)addr, true) ?? 0;
                addr++;
                bytes.Add(lo);
                bytes.Add(hi);
                ushort address = (ushort)((hi << 8) | lo);
                operandText = $"${address:X4},Y";
            }
            else if (instruction.AddressMode == Ind)
            {
                byte lo = _bus?.CpuRead((ushort)addr, true) ?? 0;
                addr++;
                byte hi = _bus?.CpuRead((ushort)addr, true) ?? 0;
                addr++;
                bytes.Add(lo);
                bytes.Add(hi);
                ushort address = (ushort)((hi << 8) | lo);
                operandText = $"(${address:X4})";
            }
            else if (instruction.AddressMode == Izx)
            {
                byte value = _bus?.CpuRead((ushort)addr, true) ?? 0;
                addr++;
                bytes.Add(value);
                operandText = $"(${value:X2},X)";
            }
            else if (instruction.AddressMode == Izy)
            {
                byte value = _bus?.CpuRead((ushort)addr, true) ?? 0;
                addr++;
                bytes.Add(value);
                operandText = $"(${value:X2}),Y";
            }
            else if (instruction.AddressMode == Rel)
            {
                byte offset = _bus?.CpuRead((ushort)addr, true) ?? 0;
                addr++;
                bytes.Add(offset);
                short signedOffset = (sbyte)offset; // Sign extend
                ushort targetAddr = (ushort)(lineAddr + 2 + signedOffset);
                operandText = $"${targetAddr:X4}";
            }

            // Build instruction text
            string instructionText = $"{instruction.Name} {operandText}".Trim();

            // Generate label if this address is a branch/jump target
            string label = labelAddresses.Contains(lineAddr) ? $"L{lineAddr:X4}" : "";

            mapLines[lineAddr] = new DisassembledInstruction
            (
                lineAddr,
                bytes.ToArray(),
                label,
                instructionText,
                addressingMode,
                !string.IsNullOrEmpty(label)
            );
        }

        return mapLines;
    }

    private int GetInstructionLength(byte opcode)
    {
        var instruction = Lookup[opcode];

        return instruction.AddressModeEnum switch
        {
            AddressModeEnum.Imp => 1,
            AddressModeEnum.Imm => 2,
            AddressModeEnum.Zp0 => 2,
            AddressModeEnum.Zpx => 2,
            AddressModeEnum.Zpy => 2,
            AddressModeEnum.Rel => 2,
            AddressModeEnum.Abs => 3,
            AddressModeEnum.Abx => 3,
            AddressModeEnum.Aby => 3,
            AddressModeEnum.Ind => 3,
            AddressModeEnum.Izx => 2,
            AddressModeEnum.Izy => 2,
            _ => 1
        };
    }

    public Dictionary<ushort, string> Disassemble(ushort start, ushort stop)
    {
        var mapLines = new Dictionary<ushort, string>();
        int addr = start;
        byte value = 0x00, lo = 0x00, hi = 0x00;
        ushort lineAddr = 0;

        string Hex(uint n, byte d)
        {
            char[] s = new string('0', d).ToCharArray();
            for (int i = d - 1; i >= 0; i--, n >>= 4)
                s[i] = "0123456789ABCDEF"[(int)(n & 0xF)];
            return new string(s);
        }

        while (addr <= stop)
        {
            lineAddr = (ushort)addr;

            var sInst = $"${Hex((uint)addr, 4)}: ";

            byte opcode = _bus?.CpuRead(addr, true) ?? 0;
            addr++;
            sInst += Lookup[opcode].Name + " ";

            var addrMode = Lookup[opcode].AddressMode;
            if (addrMode == Imp)
            {
                sInst += " {IMP}";
            }
            else if (addrMode == Imm)
            {
                value = _bus?.CpuRead(addr, true) ?? 0;
                addr++;
                sInst += "#$" + Hex(value, 2) + " {IMM}";
            }
            else if (addrMode == Zp0)
            {
                lo = _bus?.CpuRead(addr, true) ?? 0;
                addr++;
                hi = 0x00;
                sInst += "$" + Hex(lo, 2) + " {ZP0}";
            }
            else if (addrMode == Zpx)
            {
                lo = _bus?.CpuRead(addr, true) ?? 0;
                addr++;
                hi = 0x00;
                sInst += "$" + Hex(lo, 2) + ", X {ZPX}";
            }
            else if (addrMode == Zpy)
            {
                lo = _bus?.CpuRead(addr, true) ?? 0;
                addr++;
                hi = 0x00;
                sInst += "$" + Hex(lo, 2) + ", Y {ZPY}";
            }
            else if (addrMode == Izx)
            {
                lo = _bus?.CpuRead(addr, true) ?? 0;
                addr++;
                hi = 0x00;
                sInst += "($" + Hex(lo, 2) + ", X) {IZX}";
            }
            else if (addrMode == Izy)
            {
                lo = _bus?.CpuRead(addr, true) ?? 0;
                addr++;
                hi = 0x00;
                sInst += "($" + Hex(lo, 2) + "), Y {IZY}";
            }
            else if (addrMode == Abs)
            {
                lo = _bus?.CpuRead(addr, true) ?? 0;
                addr++;
                hi = _bus?.CpuRead(addr, true) ?? 0;
                addr++;
                sInst += "$" + Hex((ushort)((hi << 8) | lo), 4) + " {ABS}";
            }
            else if (addrMode == Abx)
            {
                lo = _bus?.CpuRead(addr, true) ?? 0;
                addr++;
                hi = _bus?.CpuRead(addr, true) ?? 0;
                addr++;
                sInst += "$" + Hex((ushort)((hi << 8) | lo), 4) + ", X {ABX}";
            }
            else if (addrMode == Aby)
            {
                lo = _bus?.CpuRead(addr, true) ?? 0;
                addr++;
                hi = _bus?.CpuRead(addr, true) ?? 0;
                addr++;
                sInst += "$" + Hex((ushort)((hi << 8) | lo), 4) + ", Y {ABY}";
            }
            else if (addrMode == Ind)
            {
                lo = _bus?.CpuRead(addr, true) ?? 0;
                addr++;
                hi = _bus?.CpuRead(addr, true) ?? 0;
                addr++;
                sInst += "($" + Hex((ushort)((hi << 8) | lo), 4) + ") {IND}";
            }
            else if (addrMode == Rel)
            {
                value = _bus?.CpuRead(addr, true) ?? 0;
                addr++;
                // Handle relative address calculation
                short offset = (short)(sbyte)value; // Sign-extend the byte
                ushort targetAddr = (ushort)(addr + offset);
                sInst += "$" + Hex(value, 2) + " [$" + Hex(targetAddr, 4) + "] {REL}";
            }

            mapLines[lineAddr] = sInst;
        }

        return mapLines;
    }
}