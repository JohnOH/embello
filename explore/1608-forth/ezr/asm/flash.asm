	org $E000

POWER: ; <<<<< POWER-UP INITIALISATION >>>>>

	ld  bc, $00B4 ; RAM_CTL
	ld  a, $80 ; enable SRAM, disable ERAM
	out (c), a

	inc c ; RAM_ADDR_U
	ld  a, $24 ; map SRAM to bank $24
	out (c), a

	; copy 8K SRAM $24E000 to $25E000 to stash SRAM
	db  $5B,$21,$00,$E0,$24 ; ld.lil hl, $24E000
	db  $5B,$11,$00,$E0,$25 ; ld.lil de, $25E000
	db  $5B,$01,$00,$20,$00 ; ld.lil bc, $002000
	db  $49,$ED,$B0		; ldir.l

	; copy 8K flash $000000 to $24E000 to init SRAM
	db  $5B,$21,$00,$00,$00 ; ld.lil hl, $000000
	db  $5B,$11,$00,$E0,$24 ; ld.lil de, $24E000
	db  $5B,$01,$00,$20,$00 ; ld.lil bc, $002000
	db  $49,$ED,$B0		; ldir.l

	; long jump to $240000+reloc
	db  $5B,$C3 ; jp.lil {$24,reloc}
	dw  reloc
	db  $24
reloc:	; set bank to $24 (still in a)
	db  $ED,$6D ; ld  mb, a
	db  $40,$C3 ; jp.sis $E080
	dw  $E080
	; ready for use, running in Z80 mode at $24E080

	ds  $E080-$
	jp  CPU2R ; flash cmd # 0
	jp  ERASE ; flash cmd # 1
	jp  RAM2F ; flash cmd # 2
	jp  F2RAM ; flash cmd # 3

CPU2R: ; <<<<< COPY POWER-UP CODE TO RAM DISK >>>>>

	db  $5B,$21,$00,$E0,$FF ; ld.lil hl, $FFE000
	db  $5B,$11,$00,$00,$20 ; ld.lil de, $200000
	db  $5B,$01,$80,$00,$00 ; ld.lil bc, $000080
	db  $49,$ED,$B0		; ldir.l

	halt

ERASE: ; <<<<< ERASE FLASH MEMORY >>>>>

	ld  bc, $00F5 ; FLASH_KEY
	ld  a, $B6 ; key 1
	out (c), a
	ld  a, $49 ; key 2
	out (c), a

	ld  c, $F9 ; FLASH_FDIV
	ld  a, 21  ; ceil[mhz*5.1], i.e. 21 for 4 MHz
	out (c), a

	ld  c, $F5 ; FLASH_KEY
	ld  a, $B6 ; key 1
	out (c), a
	ld  a, $49 ; key 2
	out (c), a

	ld  c, $FA ; FLASH_PROT
	ld  a, $00 ; unprotect all 8 blocks
	out (c), a

	ld  c, $FF ; FLASH_PGCTL
	ld  a, $01 ; start mass erase
	out (c), a

	halt

RAM2F: ; <<<<< COPY 256K RAM DISK TO EMPTY FLASH >>>>>

	db  $5B,$21,$00,$00,$20 ; ld.lil hl, $200000
	db  $5B,$11,$00,$00,$00 ; ld.lil de, $000000
	db  $5B,$01,$00,$00,$04 ; ld.lil bc, $040000
	db  $49,$ED,$B0		; ldir.l

	ld  bc, $00FA ; FLASH_PROT
	ld  a, $FF ; protect all 8 blocks
	out (c), a

	halt

F2RAM: ; <<<<< COPY 256K FLASH TO RAM DISK >>>>>

	db  $5B,$21,$00,$00,$00 ; ld.lil hl, $000000
	db  $5B,$11,$00,$00,$20 ; ld.lil de, $200000
	db  $5B,$01,$00,$00,$04 ; ld.lil bc, $040000
	db  $49,$ED,$B0		; ldir.l

	halt
