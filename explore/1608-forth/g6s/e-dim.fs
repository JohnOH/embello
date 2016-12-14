forgetram

pc13 constant led

: led-init           omode-pp led io-mode! ;
: led-on             led ioc! ;
: led-off            led ios! ;

: on-cycle ( n -- )  led-on ms led-off ;
: off-cycle ( n -- ) 20 over - ms ;
: cycle ( n -- )     dup on-cycle off-cycle ;

: dim ( n -- )       led-init begin dup on-cycle dup off-cycle key? until drop ;
