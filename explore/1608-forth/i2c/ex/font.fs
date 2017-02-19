\ try 5x7 font on OLED

\ these includes won't all fit in ram
\ <<<core>>>
compiletoflash

\ include ../flib/i2c/ssd1306.fs
\ include ../flib/mecrisp/graphics.fs

\ assumes that the OLED display is connected to PB6..PB7

: lcd-bufpos ( -- addr )
  lcdmem  font-y @ $38 and 4 lshift +  font-x @ $7F and + ;

: lcd-emit ( c -- )  \ switch the output to the OLED, cr's move to next line
  dup $0A = if drop
    s"                      "  \ dumb way to clear a line
    0 dup font-x !  font-y @ 8 +  $38 and  dup font-y !
    drawstring
    0 font-x !
  else
\ TODO this is smaller and faster code than the current pixel-by-pixel loops
\   32 -  0 umax 95 umin  5 * font-5x7 +
\   lcd-bufpos 5 move
\   6 font-x +!
    ascii>bitpattern drawcharacterbitmap
  then ;

: go
  lcd-init show-logo
  ['] lcd-emit hook-emit !

  8686 rf.freq ! 6 rf.group ! 62 rf.nodeid !
  rf-init 16 rf-power rf-sleep

  0 begin
    500 ms
    clear
    cr ." > " dup hex.
              dup 95 u/mod drop 32 + emit space
              dup 123456789 * hex.
    display 1+
  key? until drop

  ['] serial-emit hook-emit ! ;

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
