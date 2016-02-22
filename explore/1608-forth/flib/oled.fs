\ interface to 128x64 OLED
\ uses i2c

: nak? ( nak -- ) if ." nak " then ;

: lcd!c ( v -- )  \ send a command to the lcd
  $3C i2c-tx nak? $00 >i2c nak? >i2c nak? i2c-stop ;

\ the oled's display memory buffer is set up as 8 rows of 128 bytes
\ each byte is 8 pixels down, from b0 at the top to b7 at the bottom

1024 buffer: lcdmem

\ clear, putpixel, and display are used by the graphics.fs code

: clear ( -- )  \ clear display memory
  lcdmem 1024 0 fill ;

: putpixel ( x y -- )  \ flip a pixel in display memory
  1 over 7 and lshift ( x y bit ) -rot
  3 rshift 7 lshift + lcdmem + xor! ;

: display ( -- )  \ update the oled from display memory
  $00 lcd!c  \ SETLOWCOLUMN
  $10 lcd!c  \ SETHIGHCOLUMN
  $40 lcd!c  \ SETSTARTLINE

  lcdmem  64 0 do  \ send as a number of 16-byte data messages
    $3C i2c-tx nak? $40 >i2c nak?
    16 0 do  dup c@ >i2c nak?  1 +  loop
    i2c-stop
  loop drop ;

create logo  \ 64x64 pixels, organized as 8 bands of 16x 32-bit unsigned
hex
  00000000 , 00000000 , 00000000 , E0C0C080 , F8F8F0F0 , 3E7C7C7C , 1E1E3E3E ,
  FF1F1F1E , 1E1F1F1F , 3E3E1E1E , 787C7C3C , F0F0F0F8 , 80C0C0E0 , 00000000 ,
  00000000 , 00000000 , 00000000 , F0E08000 , 3F7EFCF8 , 03070F1F , 00000103 ,
  00000000 , 00800000 , FF000000 , 00000000 , 00000000 , F0F0F000 , 0301F0F0 ,
  1F0F0703 , F8FC7E3F , 0080E0F0 , 00000000 , FCF08000 , 0F7FFFFF , 80000103 ,
  00000000 , 78000000 , F8F8FC7C , E1F1F0F8 , C7C2E3E1 , 888C84C6 , 00000008 ,
  FFFFFF00 , 0000FFFF , 00000000 , 03010000 , FFFF7F0F , 0000E0FC , FFFFFFE0 ,
  000003FF , 01000000 , 82030101 , 8C84C4C6 , 10180888 , 63213131 , 87C74743 ,
  1F0F8F8F , 3E3E1F1F , FF7F7F7E , 0000FFFF , 00000000 , 00000000 , FF030000 ,
  E0FFFFFF , FFFFFF07 , 0000C0FF , 00000000 , 07000000 , 0F0F0F07 , 3E1F1F1F ,
  7C7C7E3E , FFF8F8FC , E3E1F1F1 , C4C6C2E2 , 18088C84 , 20302118 , 40406020 ,
  00000000 , FFC00000 , 07FFFFFF , 3F0F0100 , F0FCFFFF , 000080C0 , 00000000 ,
  00000000 , 00000000 , 00000000 , FF000000 , 03030103 , 0F070707 , FEFFFF1F ,
  0000F0FC , 00000000 , C0800000 , FFFFFEF0 , 0000073F , 00000000 , 0F070100 ,
  FC7E3F1F , C0E0F0F8 , 000080C0 , 00000000 , 00000000 , FF000000 , 00000000 ,
  7C7C0800 , 1F3F7F7E , C080070F , F8F0E0C0 , 1F3F7EFC , 0001070F , 00000000 ,
  00000000 , 00000000 , 00000000 , 07030301 , 1F1F0F0F , 7C3E3E3E , 78787C7C ,
  FFF8F878 , 78F8F8F8 , 7C7C7878 , 1E3E3E3C , 0F0F1F1F , 01030307 , 00000000 ,
  00000000 , 00000000 ,
decimal

: show-logo ( -- )  \ show the JeeLabs logo
  lcdmem 1024 0 fill
  logo lcdmem  8 0 do
    32 +  2dup 64 move  64 96 d+
  loop
  2drop display ;

: lcd-init ( -- )  \ initialise the oled display
  i2c-init
  $AE lcd!c  \ DISPLAYOFF
  $D5 lcd!c  \ SETDISPLAYCLOCKDIV
  $80 lcd!c
  $A8 lcd!c  \ SETMULTIPLEX
   63 lcd!c
  $D3 lcd!c  \ SETDISPLAYOFFSET
    0 lcd!c
  $40 lcd!c  \ SETSTARTLINE
  $8D lcd!c  \ CHARGEPUMP
  $14 lcd!c  \ switched capacitor
  $20 lcd!c  \ MEMORYMODE
  $00 lcd!c
  $A1 lcd!c  \ SEGREMAP | 0x1
  $C8 lcd!c  \ COMSCANDEC
  $DA lcd!c  \ SETCOMPINS
  $12 lcd!c
  $81 lcd!c  \ SETCONTRAST
  $CF lcd!c
  $D9 lcd!c  \ SETPRECHARGE
  $F1 lcd!c
  $DB lcd!c  \ SETVCOMDETECT
  $40 lcd!c
  $A4 lcd!c  \ DISPLAYALLON_RESUME
  $A6 lcd!c  \ NORMALDISPLAY
  $AF lcd!c  \ DISPLAYON
;

\ lcd-init show-logo
