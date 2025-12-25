; Test 4: All addressing modes
.org $8000

reset:
    ; Immediate
    lda #$11
    ldx #$22
    ldy #$33
    
    ; Absolute
    sta $0300
    stx $0301
    sty $0302
    
    ; Zero Page
    sta $00
    stx $01
    sty $02
    
    ; Absolute,X
    lda #$44
    sta $0300,x     ; X = $22, so store at $0322
    
    ; Absolute,Y
    lda #$55
    sta $0300,y     ; Y = $33, so store at $0333
    
    ; Zero Page,X
    lda #$66
    sta $00,x       ; Store at $22
    
    ; Zero Page,Y (LDX only)
    ldx $00,y       ; Load from $33
    
    ; Indexed Indirect (X)
    ; Set up indirect pointer
    lda #$00
    sta $40
    lda #$04
    sta $41         ; $40 points to $0400
    
    ldx #$00
    lda #$77
    sta ($40,x)     ; Store at $0400
    
    ; Indirect Indexed (Y)
    ldy #$10
    lda #$88
    sta ($40),y     ; Store at $0410
    
    ; Test reading back
    lda $0300       ; Should be $11
    ldx $0301       ; Should be $22
    ldy $0302       ; Should be $33
    
    lda $0322       ; Should be $44
    lda $0333       ; Should be $55
    lda $0022       ; Should be $66
    lda $0400       ; Should be $77
    lda $0410       ; Should be $88
    
    jmp reset

.org $fffc
.word $8000
.word $0000