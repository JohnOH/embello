\ basic quadrature pulse decoder with OLED readout

compiletoram? [if]  forgetram  [then]

PA3 constant ENC-A
PA5 constant ENC-B
PA4 constant ENC-C  \ common

1000 variable counter

: ab-pins ( -- n )  \ read current A & B pin state as bits 1 and 0
  ENC-A io@ %10 and  ENC-B io@ %01 and  or ;

: step ( n -- )  counter +!  counter @ shownum ;

: read-enc
  %11  \ previous state, stays on the stack
  begin
    2 lshift  \ prev pins in bits 3 and 2
    ab-pins tuck  \ new pins, also save as previous for next cycle
    or  \ combines prev-a/prev-b/curr-a/curr-b into a 4-bit value

    \ process this 4-bit value and leave only prev state on stack
    case
      %0001 of -1 step endof
      %0010 of  1 step endof
      %0100 of  1 step endof
      %0111 of -1 step endof
      %1000 of -1 step endof
      %1011 of  1 step endof
      %1101 of  1 step endof
      %1110 of -1 step endof
    endcase
  again ;

IMODE-HIGH ENC-A io-mode!
IMODE-HIGH ENC-B io-mode!
OMODE-PP   ENC-C io-mode!  ENC-C ioc!

lcd-init clear display
read-enc
