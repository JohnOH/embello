	org $E000

pd_alt2:    equ $A5
uart0_thr:  equ $C0
uart0_rbr:  equ $C0
uart0_brgl: equ $C0
uart0_fctl: equ $C2
uart0_lctl: equ $C3
uart0_lsr:  equ $C5

init:	ld  b, 0

	ld  c, pd_alt2
	ld  a, $03
	out (c), a

	ld  c, uart0_lctl
	ld  a, $80
	out (c), a

	ld  c, uart0_brgl
	ld  a, $1A
	out (c), a

	ld  c, uart0_lctl
	ld  a, $03
	out (c), a

	ld  c, uart0_fctl
	ld  a, $06
	out (c), a

print:	ld  hl, msg
prloop:	ld  a, (hl)
	and a
	jr  z, done
prwait:	ld  c, uart0_lsr
	in  a, (c)
	and $20
	jr  z, prwait
prout:	ld  c, uart0_thr
	ld  a, (hl)
	out (c), a
	inc hl
	jr  prloop

done:	jr  done

msg:	dm  "Hello world!", 10, 13, 0

	end
