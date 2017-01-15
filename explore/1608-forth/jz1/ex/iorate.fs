\ explore I/O pin access rates
\ needs core.fs
cr cr reset
cr

: go
  begin
    500 ms
  key? until drop ;

omode-pp pa0 io-mode!

: t0 micros 1000 0 do          loop micros swap - . ; t0
: t1 micros 1000 0 do pa0 ios! loop micros swap - . ; t1
: t2 micros 1000 0 do pa0 ioc! loop micros swap - . ; t2
: t3 micros 1000 0 do pa0 iox! loop micros swap - . ; t3

PA0 constant IO-PIN

IO-PIN io-base constant IO.GPIO
IO-PIN io#     constant IO.PIN

: io.out0       IO.PIN 16 + bit  IO.GPIO GPIO.BSRR    + !    ;
: io.out1       IO.PIN      bit  IO.GPIO GPIO.BSRR    + !    ;

: x1 micros 1000 0 do io.out1 loop micros swap - . ; x1
: x2 micros 1000 0 do io.out0 loop micros swap - . ; x2

: go
  2.1MHz 1000 systick-hz
  t0 t1 t2 t3 x1 x2
  65KHz 100 systick-hz
  t0 t1 t2 t3 x1 x2
  2.1MHz 1000 systick-hz
;

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
