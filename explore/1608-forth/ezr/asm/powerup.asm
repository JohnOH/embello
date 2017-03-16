; Power-up initialisation code for eZ80

SRAM: equ 0E000h ; starting address of common SRAM
SIZE: equ 00080h ; size of the power-up init code
DEST: equ 0E400h ; load and jump to this address

BANK: equ 24h	 ; SRAM and MBASE are set to this bank
SAVE: equ 25h	 ; original SRAM contents is this bank
FROM: equ 20h	 ; bank from which to copy everything

    org SRAM

; 1) disable ERAM and move SRAM to BANK
    ld hl,$8000+BANK
    db 0EDh,21h,0B4h ; out0 (RAM_CTL),h ; disable ERAM
    db 0EDh,29h,0B5h ; out0 (RAM_BANK),l ; SRAM to BANK

; 2) copy 8K SRAM {BANK,SRAM} to {SAVE,SRAM}
    db 5Bh,21h ; ld.lil hl,{BANK,SRAM}
    dw SRAM
    db BANK
    db 5Bh,11h ; ld.lil de,{SAVE,SRAM}
    dw SRAM
    db SAVE
    db 5Bh,01h ; ld.lil bc,002000h
    dw 2000h
    db 00h
    db 49h,0EDh,0B0h ; ldir.l

; 3) copy 6.5K {FROM,0000h} to {BANK,0E380h..0FD80h}
    db 5Bh,21h ; ld.lil hl,{FROM,0000h}
    dw 0000h
    db FROM
    db 5Bh,11h ; ld.lil de,{BANK,DEST-SIZE}
    dw DEST-SIZE
    db BANK
    db 5Bh,01h ; ld.lil bc,001A00h
    dw 1A00h
    db 00h
    db 49h,0EDh,0B0h ; ldir.l

; 4) room to add more setup code here...

; 5) jump to SRAM before changing MBASE (still in A)
    db 5Bh,0C3h ; jp.lil {BANK,DEST-6} (enters ADL mode)
    dw DEST-6 ; running 0380h higher now, from new copy!
    db BANK

    ds  SRAM+SIZE-$-6 ; take up slack space

; 6) change MBASE from ADL mode and fall through to dest
    db 0EDh,6Dh ; ld  mb,a
    db 40h,0C3h ; jp.sis DEST (this also exits ADL mode)
    dw DEST

; ready for use, now running in Z80 mode at {BANK,DEST}
    end
