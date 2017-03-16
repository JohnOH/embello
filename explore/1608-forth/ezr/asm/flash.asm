	org $E000

	jp  ERASE ; flash cmd # 0
	jp  RAM2F ; flash cmd # 1
	jp  F2RAM ; flash cmd # 2

ERASE: ; <<<<< ERASE FLASH MEMORY >>>>>

	ld  bc,$00F5 ; FLASH_KEY
	ld  a,$B6 ; key 1
	out (c),a
	ld  a,$49 ; key 2
	out (c),a

	ld  c,$F9 ; FLASH_FDIV
	ld  a,21  ; ceil[mhz*5.1], i.e. 21 for 4 MHz
	out (c),a

	ld  c,$F5 ; FLASH_KEY
	ld  a,$B6 ; key 1
	out (c),a
	ld  a,$49 ; key 2
	out (c),a

	ld  c,$FA ; FLASH_PROT
	ld  a,$00 ; unprotect all 8 blocks
	out (c),a

	ld  c,$FF ; FLASH_PGCTL
	ld  a,$01 ; start mass erase
	out (c),a

	jr $

RAM2F: ; <<<<< COPY 256K RAM DISK TO EMPTY FLASH >>>>>

	db $5B,$21,$00,$00,$20 ; ld.lil hl,$200000
	db $5B,$11,$00,$00,$00 ; ld.lil de,$000000
	db $5B,$01,$00,$00,$04 ; ld.lil bc,$040000
	db $49,$ED,$B0	       ; ldir.l

	ld  bc,$00FA ; FLASH_PROT
	ld  a,$FF ; protect all 8 blocks
	out (c),a

	jr $

F2RAM: ; <<<<< COPY 256K FLASH TO RAM DISK >>>>>

	db $5B,$21,$00,$00,$00 ; ld.lil hl,$000000
	db $5B,$11,$00,$00,$20 ; ld.lil de,$200000
	db $5B,$01,$00,$00,$04 ; ld.lil bc,$040000
	db $49,$ED,$B0	       ; ldir.l

	jr $

	end
