\ tft driver for ILI9163 chip, uses own bit-bang implementation

create tft:init
hex
  001 h, 201 h, 011 h, 214 h, 028 h, 013 h, 020 h, 026 h, 101 h, 02A h,
  100 h, 100 h, 100 h, 17F h, 02B h, 100 h, 100 h, 100 h, 17F h, 036 h,
  14A h, 03A h, 155 h, 278 h, 029 h, 0 h,
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
  $2A >tft $100 >tft $102 + >tft $100 >tft $181 >tft
  $2B >tft $100 >tft $101 + >tft $100 >tft $180 >tft
  $2C >tft
;

\ clear, putpixel, and display are used by the graphics.fs code

: clear ( -- )  \ clear display memory
  0 0 goxy  tft-bg @  16384 0 do  dup h>tft  loop  drop ;

: putpixel ( x y -- )  \ set a pixel in display memory
  goxy  tft-fg @ h>tft ;

: display ( -- ) ;  \ update tft from display memory (ignored)
