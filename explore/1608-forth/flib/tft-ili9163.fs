\ tft driver for ILI9163 chip

PB12 constant TFT-CS  \ chip select
PB13 constant TFT-SC  \ serial clock
PB15 constant TFT-DI  \ data into LCD
PC6  constant TFT-RS  \ register select

create tft:init
hex
  E7 h, 0010 h, 00 h, 0001 h, 01 h, 0100 h, 02 h, 0700 h, 03 h, 1038 h,
  04 h, 0000 h, 08 h, 0207 h, 09 h, 0000 h, 0A h, 0000 h, 0C h, 0001 h,
  0D h, 0000 h, 0F h, 0000 h, 10 h, 0000 h, 11 h, 0007 h, 12 h, 0000 h,
  13 h, 0000 h, -1 h, #50  h, 10 h, 1590 h, 11 h, 0227 h, -1 h, #50  h,
  12 h, 009C h, -1 h, #50  h, 13 h, 1900 h, 29 h, 0023 h, 2B h, 000E h,
  -1 h, #50  h, 20 h, 0000 h, 21 h, 0000 h, -1 h, #50  h, 30 h, 0007 h,
  31 h, 0707 h, 32 h, 0006 h, 35 h, 0704 h, 36 h, 1F04 h, 37 h, 0004 h,
  38 h, 0000 h, 39 h, 0706 h, 3C h, 0701 h, 3D h, 000F h, -1 h, #50  h,
  50 h, 0000 h, 51 h, 00EF h, 52 h, 0000 h, 53 h, 013F h, 60 h, A700 h,
  61 h, 0001 h, 6A h, 0000 h, 80 h, 0000 h, 81 h, 0000 h, 82 h, 0000 h,
  83 h, 0000 h, 84 h, 0000 h, 85 h, 0000 h, 90 h, 0010 h, 92 h, 0000 h,
  93 h, 0003 h, 95 h, 0110 h, 97 h, 0000 h, 98 h, 0000 h, 07 h, 0133 h,
  20 h, 0000 h, 21 h, 0000 h, 0 ,

  011 h, 205 h, 03A h, 105 h, 026 h, 104 h, 0F2 h, 101 h, 0E0 h, 13F h, 125 h,
  11C h, 11E h, 120 h, 112 h, 12A h, 190 h, 124 h, 111 h, 100 h, 100 h, 100 h,
  100 h, 100 h, 0E1 h, 120 h, 120 h, 120 h, 120 h, 105 h, 100 h, 115 h, 1A7 h,
  13D h, 118 h, 125 h, 12A h, 12B h, 12B h, 13A h, 0B1 h, 108 h, 108 h, 0B4 h,
  107 h, 0C0 h, 10A h, 102 h, 0C1 h, 102 h, 0C5 h, 150 h, 15B h, 0C7 h, 140 h,
  02A h, 100 h, 100 h, 100 h, 17F h, 02B h, 100 h, 100 h, 100 h, 17F h, 036 h,
  100 h, 029 h, 02C h, 0 h,
decimal align

: +tft ( u - )  \ init tft: cmd=0/data=2 + write=0/read=1
;
: -tft ;

: tft! ( val reg -- )
  0 +tft 0 >spi >spi -tft
  2 +tft dup 8 rshift >spi >spi -tft ;

: tft-config ( -- )
  tft:init begin
    dup h@  ?dup while
      dup $200 and if drop $FF and ms else tft! then
  2 + repeat drop ;

$0000 variable tft-bg
$FC00 variable tft-fg

: tft-init ( -- )
  OMODE-PP TFT-CS io-mode!
  OMODE-PP TFT-SC io-mode!
  OMODE-PP TFT-DI io-mode!
  OMODE-PP TFT-RS io-mode!
  tft-config ;

\ clear, putpixel, and display are used by the graphics.fs code

: clear ( -- )  \ clear display memory
  0 $21 tft! 0 $20 tft!
  tft-bg @ 320 240 * 0 do dup $22 tft! loop drop ;

: putpixel ( x y -- )  \ set a pixel in display memory
  $21 tft! $20 tft! tft-fg @ $22 tft! ;

: display ( -- ) ;  \ update tft from display memory (ignored)
