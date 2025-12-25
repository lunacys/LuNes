; Test 3: Branch and Jump instructions
.org $8000

reset:
    ; Test unconditional branches
    jmp skip1       ; Jump forward
    lda #$ff        ; Should be skipped
    sta $0200
    
skip1:
    ; Test conditional branches
    ldx #$00
    inx
    beq should_not_branch  ; Should NOT branch (Z=0)
    bne should_branch      ; SHOULD branch (Z=0)
    
should_not_branch:
    lda #$ff        ; Should never execute
    jmp end_test
    
should_branch:
    lda #$01
    sta $0200
    
    ; Test loop with branches
    ldx #$05
    ldy #$00
loop:
    iny
    dex
    bne loop        ; Loop 5 times
    
    sty $0201       ; Should be $05
    
    ; Test negative flag branches
    lda #$80        ; Negative number
    bmi is_negative ; Should branch
    lda #$ff        ; Should not execute
    jmp end_test
    
is_negative:
    lda #$02
    sta $0202
    
    ; Test overflow flag
    clv
    bvc no_overflow ; Should branch
    lda #$ff
    jmp end_test
    
no_overflow:
    lda #$03
    sta $0203
    
end_test:
    jmp end_test    ; Infinite loop

.org $fffc
.word $8000
.word $0000