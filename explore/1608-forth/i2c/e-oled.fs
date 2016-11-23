\ show logo on an OLED attached via I2C

\ these includes won't all fit in ram
\ <<<core>>>
\ compiletoflash

\ include ../flib/stm32l0/i2c.fs
include ../flib/i2c/oled.fs
\ include ../mlib/graphics.fs

\ assumes that the OLED is connected to PB6..PB7

OLED.HEIGHT 64 < [if] $18 [else] $38 [then] constant OLED.MASK

: lcd-emit ( c -- )  \ switch the output to the OLED, cr's move to next line
  dup $0A = if drop
    s"                 "  \ dumb way to clear a line
    0 dup font-x !  font-y @  8 +  OLED.MASK and  dup font-y !
    drawstring
    0 font-x !
  else
    ascii>bitpattern drawcharacterbitmap
  then ;

: go
  hook-emit @
  ['] lcd-emit hook-emit !
  cr 0 font-x ! 0 font-y ! clear
  ." The quick brown fox ..." display cr
  ." ... jumps over the ..."  display cr
  ." ...... lazy dog !!!"     display cr
  ." (or so they say)"        display
  hook-emit ! ;

\ +i2c i2c? i2c.
lcd-init
\ show-logo

\ 1234 ms go
