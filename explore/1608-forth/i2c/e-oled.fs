\ show logo on an OLED attached via I2C

\ these includes won't all fit in ram
\ <<<core>>>
\ compiletoflash

\ include ../flib/stm32l0/i2c.fs
\ include ../flib/i2c/oled.fs
\ include ../mlib/graphics.fs

\ assumes that the OLED is connected to PB6..PB7

: lcd-emit ( c -- )  \ switch the output to the OLED, cr's move to next line
  dup $0A = if drop
    2 OLED.LARGE - \ 2 for small OLED, 1 for large OLED
    OLED.LARGE 32 * $18 or  \ mask for Y positions: $18 or $38
    font-y @  7 or 1+ and  dup font-y !  \ advance to next line
    over * 16 * lcdmem + swap 128 * 0 fill  \ clear entire line
    0 font-x !  \ go to start of line
  else
    ascii>bitpattern drawcharacterbitmap
  then ;

: go
  hook-emit @
  ['] lcd-emit hook-emit !
  cr 0 font-x ! 0 font-y ! clear
  ." The quick brown <fox>" cr
  ." jumps over the fairly" cr
  ." lazy little <doggie>!" cr
  ."  (or so they say...)"  display
  hook-emit ! ;

\ +i2c i2c? i2c.
lcd-init
show-logo
1234 ms go
