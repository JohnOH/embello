\ tft driver for ILI9163 chip, uses own bit-bang implementation

PB12 constant TFT-CS  \ chip select
PB13 constant TFT-SC  \ serial clock
PB15 constant TFT-DI  \ data into LCD
PC6  constant TFT-RS  \ register select

create tft:init
hex
  011 h, 205 h, 03A h, 105 h, 026 h, 104 h, 0F2 h, 101 h, 0E0 h, 13F h,
  125 h, 11C h, 11E h, 120 h, 112 h, 12A h, 190 h, 124 h, 111 h, 100 h,
  100 h, 100 h, 100 h, 100 h, 0E1 h, 120 h, 120 h, 120 h, 120 h, 105 h,
  100 h, 115 h, 1A7 h, 13D h, 118 h, 125 h, 12A h, 12B h, 12B h, 13A h,
  0B1 h, 108 h, 108 h, 0B4 h, 107 h, 0C0 h, 10A h, 102 h, 0C1 h, 102 h,
  0C5 h, 150 h, 15B h, 0C7 h, 140 h, 02A h, 100 h, 100 h, 100 h, 17F h,
  02B h, 100 h, 100 h, 100 h, 17F h, 036 h, 100 h, 029 h, 02C h, 0 h,
decimal align

: >tft ( u -- )
  dup $100 and TFT-RS io!
  TFT-CS ioc!
  8 0 do
    dup $80 and TFT-DI io!
    TFT-SC ios!
    shr
    TFT-SC ioc!
  loop
  TFT-CS ios!  TFT-RS ios!  drop ;

: h>tft ( u -- )
\ assumes TFT-RS is already set
  TFT-CS ioc!
  16 0 do
    dup $8000 and TFT-DI io!
    TFT-SC ios!
    shr
    TFT-SC ioc!
  loop
  TFT-CS ios! drop ;

: tft-config ( -- )
  tft:init begin
    dup h@  ?dup while
      dup $200 and if $FF and ms else >tft then
  2 + repeat drop ;

$0000 variable tft-bg
$FC00 variable tft-fg

: tft-init ( -- )
  OMODE-PP TFT-CS io-mode!  TFT-CS ios!
  OMODE-PP TFT-SC io-mode!  TFT-SC ioc!
  OMODE-PP TFT-DI io-mode!
  OMODE-PP TFT-RS io-mode!
  tft-config ;

: goxy ( x y -- )
  $2A >tft $100 >tft $100 or >tft $100 >tft $17F >tft
  $2B >tft $100 >tft $100 or >tft $100 >tft $17F >tft
;

\ clear, putpixel, and display are used by the graphics.fs code

: clear ( -- )  \ clear display memory
  0 0 goxy  tft-bg @  16384 0 do  dup h>tft  loop  drop ;

: putpixel ( x y -- )  \ set a pixel in display memory
  goxy  tft-fg @ h>tft ;

: display ( -- ) ;  \ update tft from display memory (ignored)
