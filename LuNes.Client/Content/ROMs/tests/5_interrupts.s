; Correct interrupt test for 6502
.org $8000

reset:
    sei             ; Disable interrupts initially
    cld             ; Clear decimal mode
    ldx #$ff
    txs             ; Set stack pointer to $01FF
    
    ; Main program - just increment a counter
    lda #$00
    sta $0200       ; IRQ counter
    sta $0201       ; NMI counter
    
    ; Enable interrupts
    cli
    
main_loop:
    ; Just wait for interrupts
    nop
    nop
    jmp main_loop

; IRQ Handler (triggered by Q key)
irq_handler:
    pha             ; Save A
    inc $0200       ; Increment IRQ counter
    pla             ; Restore A
    rti             ; Return from interrupt

; NMI Handler (triggered by W key)
nmi_handler:
    pha             ; Save A
    inc $0201       ; Increment NMI counter
    pla             ; Restore A
    rti

; Vectors (MUST be at these specific addresses)
.org $fffa
.word nmi_handler   ; NMI vector at $FFFA-$FFFB
.word reset         ; Reset vector at $FFFC-$FFFD
.word irq_handler   ; IRQ vector at $FFFE-$FFFF