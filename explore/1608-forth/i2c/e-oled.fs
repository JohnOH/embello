\ show logo on an OLED attached via I2C

\ these includes won't all fit in ram
\ <<<core>>>
\ compiletoflash

\ include ../flib/stm32l0/i2c.fs
\ include ../flib/i2c/oled.fs
\ include ../mlib/graphics.fs

\ assumes that the OLED is connected to PB6..PB7

OLED.HEIGHT 64 < [if] $18 [else] $38 [then] constant OLED.MASK

: lcd-emit ( c -- )  \ switch the output to the OLED, cr's move to next line
  dup $0A = if drop
    font-y @  7 or 1+  OLED.MASK and  dup font-y !  \ advance to next line
\   16 * lcdmem + 128 0 fill  \ clear entire line
    0 font-x !  \ go to start of line
  else
    ascii>bitpattern drawcharacterbitmap
  then ;

: go
  hook-emit @
  ['] lcd-emit hook-emit !
  cr 0 font-x ! 0 font-y ! clear
  ." The quick brown <fox>" display cr
  ." jumps over the fairly" display cr
  ." lazy little <doggie>!" display cr
  ."  (or so they say...)"  display
\ 2000 ms
\ cr ."  (say...)"  display
\ 2000 ms
\ cr ."  (...)"  display
  hook-emit ! ;

\ +i2c i2c? i2c.
lcd-init
show-logo

go
