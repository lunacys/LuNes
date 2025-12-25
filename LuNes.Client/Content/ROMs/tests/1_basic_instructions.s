; Test 1: Basic instruction verification
.org $8000

reset:
    ; Test 1: Load/Store/Transfer
    lda #$aa        ; A = $AA
    sta $0200       ; Store A
    ldx #$bb        ; X = $BB
    stx $0201       ; Store X
    ldy #$cc        ; Y = $CC
    sty $0202       ; Store Y
    
    ; Test 2: Transfer instructions
    tax             ; A -> X (should be $AA)
    txa             ; X -> A
    tay             ; A -> Y (should be $AA)
    tya             ; Y -> A
    
    ; Test 3: Stack operations
    ldx #$ff
    txs             ; Set stack pointer
    lda #$55
    pha             ; Push $55
    lda #$00
    pla             ; Pull (should be $55)
    
    ; Test 4: Increment/Decrement
    lda #$01
    sta $0203
    inc $0203       ; Should be $02
    dec $0203       ; Should be $01
    
    ; Test 5: Flags
    clc             ; Clear carry
    sec             ; Set carry
    cli             ; Clear interrupt disable
    sei             ; Set interrupt disable
    cld             ; Clear decimal mode
    sed             ; Set decimal mode
    
    ; Loop forever so we can inspect results
    jmp reset

.org $fffc
.word $8000
.word $0000