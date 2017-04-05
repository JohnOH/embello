; Erase and unlock flash memory

	org $E000

	ld  bc,$00F5 ; FLASH_KEY
	ld  a,$B6 ; key 1
	out (c),a
	ld  a,$49 ; key 2
	out (c),a

	ld  c,$F9 ; FLASH_FDIV
	ld  a,41  ; ceil[mhz*5.1], i.e. 41 for 8 MHz
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

	end
