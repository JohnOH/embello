\ first attempt to read out a rotary encoder

compiletoram? [if]  forgetram  [then]

PA5 constant ENC-A
PA3 constant ENC-B
PA4 constant ENC-C  \ common

IMODE-HIGH ENC-A io-mode!
IMODE-HIGH ENC-B io-mode!
OMODE-PP   ENC-C io-mode!  ENC-C ioc!

: read-enc
  begin
    cr ENC-A io@ . ENC-B io@ .
    500 ms
  again ;

read-enc
