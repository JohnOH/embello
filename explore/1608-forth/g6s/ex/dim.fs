\ Example code for http://jeelabs.org/article/1650e/

forgetram

pc13 constant led

: led-init             omode-pp led io-mode! ;
: led-on               led ioc! ;
: led-off              led ios! ;
: on-cycle   ( n -- )  led-on ms led-off ;
: off-cycle  ( n -- )  20 swap - ms ;
: cycle      ( n -- )  dup on-cycle off-cycle ;
: dim        ( n -- )  led-init begin dup cycle key? until drop ;

1 dim  \ dim the LED to 5% (the full scale is 0..20)
