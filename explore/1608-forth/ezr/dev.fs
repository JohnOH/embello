\ Control an eZ80 microcontroller from a Blue Pill via ZDI

include vdisk.fs

: t1  \ show output from USART2
  uart-init 19200 uart-baud
  c
  100000 0 do
    uart-key? if uart-key emit then
  loop cr b r ;

: h  \ send greeting over serial, see asm/hello.asm
  b u
include asm/hello.fs
  u t1 ;

: l  \ send greeting over serial, see asm/hellow.asm, in low mem
  b $FF0100 a
include asm/hellow.fs
  $FF0100 a t1 ;

: halt $76 ins1 ;

: go  $3A6000 a c t ;
