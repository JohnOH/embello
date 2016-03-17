\ tft driver for ILI9163 chip, uses own bit-bang implementation

PC6  constant TFT-RS  \ register select
PB12 constant TFT-CS  \ chip select
PB13 constant TFT-SC  \ serial clock
PB15 constant TFT-DI  \ data into LCD

create tft:init
hex
\ 011 h, 214 h, 026 h, 104 h, 0B1 h, 10e h, 110 h, 0C0 h, 108 h, 100 h,
\ 0C1 h, 105 h, 0C5 h, 138 h, 140 h, 03a h, 105 h, 036 h, 11C h, 02A h,
\ 100 h, 100 h, 100 h, 17F h, 02B h, 100 h, 120 h, 100 h, 19F h, 0B4 h,
\ 100 h, 0f2 h, 101 h, 0E0 h, 13f h, 122 h, 120 h, 130 h, 129 h, 10c h,
\ 14e h, 1b7 h, 13c h, 119 h, 122 h, 11e h, 102 h, 101 h, 100 h, 0E1 h,
\ 100 h, 11b h, 11f h, 10f h, 116 h, 113 h, 131 h, 184 h, 143 h, 106 h,
\ 11d h, 121 h, 13d h, 13e h, 13f h, 029 h, 02C h, 0 h,

\ 011 h, 205 h, 03A h, 105 h, 026 h, 104 h, 0F2 h, 101 h, 0E0 h, 13F h,
\ 125 h, 11C h, 11E h, 120 h, 112 h, 12A h, 190 h, 124 h, 111 h, 100 h,
\ 100 h, 100 h, 100 h, 100 h, 0E1 h, 120 h, 120 h, 120 h, 120 h, 105 h,
\ 100 h, 115 h, 1A7 h, 13D h, 118 h, 125 h, 12A h, 12B h, 12B h, 13A h,
\ 0B1 h, 108 h, 108 h, 0B4 h, 107 h, 0C0 h, 10A h, 102 h, 0C1 h, 102 h,
\ 0C5 h, 150 h, 15B h, 0C7 h, 140 h, 02A h, 100 h, 100 h, 100 h, 17F h,
\ 02B h, 100 h, 100 h, 100 h, 17F h, 036 h, 100 h, 029 h, 02C h, 0 h,

\ https://github.com/pyrohaz/STM32F0-ILI9163/blob/master/ILI9163.c
\ 001 h, 2FF h, 011 h, 214 h, 026 h, 104 h, 0C0 h, 11F h, 0C1 h, 100 h,
\ 0C2 h, 100 h, 107 h, 0C3 h, 100 h, 107 h, 0C5 h, 124 h, 1C8 h, 038 h,
\ 03A h, 105 h, 036 h, 108 h, 029 h, 020 h, 02C h, 0 h,

  001 h, 201 h, 011 h, 214 h, 028 h, 013 h, 020 h, 026 h, 101 h, 02A h,
  100 h, 100 h, 100 h, 17F h, 02B h, 100 h, 100 h, 100 h, 17F h, 036 h,
  10A h, 03A h, 155 h, 278 h, 029 h, 0 h,
\ 02A h, 100 h, 100 h, 100 h, 17F h,
\ 02B h, 100 h, 100 h, 100 h, 17F h, 02C h, 0 h,
decimal

: >tft ( u -- )
  dup $100 and TFT-RS io!
  TFT-CS ioc!
  8 0 do
    dup $80 and TFT-DI io!
    TFT-SC ios!
    shl
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

$0000 variable tft-bg
$FC00 variable tft-fg

: tft-init ( -- )
  OMODE-PP TFT-RS io-mode!  TFT-RS ios!
  OMODE-PP TFT-CS io-mode!  TFT-CS ios!
  OMODE-PP TFT-SC io-mode!  TFT-SC ioc!
  OMODE-PP TFT-DI io-mode!
  tft:init begin
    dup h@  ?dup while
      dup $200 and if $FF and ms else >tft then
  2 + repeat drop ;

: goxy ( x y -- )
  $2A >tft $100 >tft $100 or >tft $100 >tft $17F >tft
  $2B >tft $100 >tft $100 or >tft $100 >tft $17F >tft
  $2C >tft
;

\ clear, putpixel, and display are used by the graphics.fs code

: clear ( -- )  \ clear display memory
  0 0 goxy  tft-bg @  16384 0 do  dup h>tft  loop  drop ;

: putpixel ( x y -- )  \ set a pixel in display memory
  goxy  tft-fg @ h>tft ;

: display ( -- ) ;  \ update tft from display memory (ignored)
