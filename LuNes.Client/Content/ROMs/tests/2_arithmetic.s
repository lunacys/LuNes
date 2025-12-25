; Test 2: Arithmetic operations
.org $8000

reset:
    ; Test ADC (addition with carry)
    clc
    lda #$10
    adc #$20        ; A = $30, C=0
    sta $0200
    
    sec
    lda #$10
    adc #$20        ; A = $31, C=0 (with carry)
    sta $0201
    
    ; Test ADC with overflow
    clc
    lda #$50
    adc #$50        ; A = $A0, V=1 (signed overflow)
    sta $0202
    
    ; Test SBC (subtraction with carry)
    sec
    lda #$50
    sbc #$20        ; A = $30, C=1
    sta $0203
    
    clc
    lda #$20
    sbc #$30        ; A = $EF, C=0 (borrow)
    sta $0204
    
    ; Test comparisons
    lda #$40
    cmp #$40        ; Z=1, C=1, N=0
    cmp #$41        ; Z=0, C=0, N=1
    cmp #$3F        ; Z=0, C=1, N=0
    
    ; Test decimal mode (if W65C02 supports)
    cld             ; Clear decimal
    sed             ; Set decimal (for BCD)
    clc
    lda #$19
    adc #$01        ; In decimal mode: $19 + $01 = $20
    sta $0205
    cld             ; Back to binary
    
    ; Loop forever
    jmp reset

.org $fffc
.word $8000
.word $0000