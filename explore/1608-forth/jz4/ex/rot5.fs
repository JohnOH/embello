\ basic quadrature pulse decoder sending data to an OLED over RF

compiletoram? [if]  forgetram  [then]

PA3 constant ENC-A
PA5 constant ENC-B
PA4 constant ENC-C  \ common

1000 variable counter

: ab-pins ( -- n )  \ read current A & B pin state as bits 1 and 0
  ENC-A io@ %10 and  ENC-B io@ %01 and  or ;

: step ( n -- )  counter +!  7 <pkt counter @ +pkt pkt>rf ;

: read-enc
  IMODE-HIGH ENC-A io-mode!
  IMODE-HIGH ENC-B io-mode!
  OMODE-PP   ENC-C io-mode!  ENC-C ioc!
  rf-init

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

: rxtestv ( -- )
  rf-init lcd-init clear display
  begin
    rf-recv ?dup if
      rf.buf 2+  swap 2-  var-init
      var> if drop then     \ ignore the format type
      var> if shownum then  \ show the payload on OLED
    then
  again ;

\ on the receiver end, start up with: rxtestv
\ on the transmitter end, start up with: read-enc
