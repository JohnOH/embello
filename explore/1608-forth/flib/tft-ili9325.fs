\ tft driver for ili9325 chip

: ms ( u -- )  \ millisecond delay
  0 do 10000 0 do loop loop ;

create tft:init
hex
  e7 h, 0010 h,
  00 h, 0001 h,
  01 h, 0100 h,
  02 h, 0700 h,
  03 h, 1038 h,
  04 h, 0000 h,
  08 h, 0207 h,
  09 h, 0000 h,
  0a h, 0000 h,
  0c h, 0001 h,
  0d h, 0000 h,
  0f h, 0000 h,
  10 h, 0000 h,
  11 h, 0007 h,
  12 h, 0000 h,
  13 h, 0000 h,
  -1 h, #50 h,
  10 h, 1590 h,
  11 h, 0227 h,
  -1 h, #50 h,
  12 h, 009c h,
  -1 h, #50 h,
  13 h, 1900 h,
  29 h, 0023 h,
  2b h, 000e h,
  -1 h, #50 h,
  20 h, 0000 h,
  21 h, 0000 h,
  -1 h, #50 h,
  30 h, 0007 h,
  31 h, 0707 h,
  32 h, 0006 h,
  35 h, 0704 h,
  36 h, 1f04 h,
  37 h, 0004 h,
  38 h, 0000 h,
  39 h, 0706 h,
  3c h, 0701 h,
  3d h, 000f h,
  -1 h, #50 h,
  50 h, 0000 h,
  51 h, 00ef h,
  52 h, 0000 h,
  53 h, 013f h,
  60 h, a700 h,
  61 h, 0001 h,
  6a h, 0000 h,
  80 h, 0000 h,
  81 h, 0000 h,
  82 h, 0000 h,
  83 h, 0000 h,
  84 h, 0000 h,
  85 h, 0000 h,
  90 h, 0010 h,
  92 h, 0000 h,
  93 h, 0003 h,
  95 h, 0110 h,
  97 h, 0000 h,
  98 h, 0000 h,
  07 h, 0133 h,
  20 h, 0000 h,
  21 h, 0000 h,
\ -1 h, #100 h,
  0 ,
decimal align

: +tft ( u - )  \ init tft: cmd=0/data=2 + write=0/read=1
  +spi $70 or >spi ;
: -tft ( -- ) -spi ;

: tft@ ( reg -- val )
  0 +tft 0 >spi >spi -tft
  3 +tft spi> 8 lshift spi> or -tft ;

: tft! ( val reg -- )
  0 +tft 0 >spi >spi -tft
  2 +tft dup 8 rshift >spi $FF and >spi -tft ;

: tft. ( -- )  \ dump ILI9325 register contents
  cr space 16 0 do 2 spaces i h.2 loop
  $A0 0 do
    cr  i h.2 space
    16 0 do  i j + tft@ h.4  loop
  $10 +loop ;

: tft-config ( -- )
  tft:init begin
    dup @  ?dup while
      dup 16 rshift swap ( addr val reg )
      dup $100 and if drop ms else tft! then
    4 +
  repeat drop ;

$00FF variable bg
$FF00 variable fg

: tft-init ( -- )
  PB0 ssel !  \ use PB0 to select the TFT display
  spi-init
\ switch to alternate SPI pins, PB3..5 iso PA5..7
  $03000001 AFIO-MAPR !  \ also disable JTAG & SWD to free PB3 PB4 PA15
  IMODE-FLOAT PA5 io-mode!
  IMODE-FLOAT PA6 io-mode!
  IMODE-FLOAT PA7 io-mode!
  OMODE-AF-PP OMODE-FAST + PB3 io-mode!
  OMODE-AF-PP OMODE-FAST + PB4 io-mode!
  OMODE-AF-PP OMODE-FAST + PB5 io-mode!
  OMODE-PP PB2 io-mode!  PB2 io-1!
  %0000000001010110 SPI1-CR1 !  \ clk/16, i.e. 4.5 MHz, master, CPOL=1 (!)
  tft-config ;

: clear ( -- )  \ clear display memory
\ TODO
;

: putpixel ( x y -- )  \ set a pixel in display memory
  $21 tft! $20 tft! fg @ $22 tft! ;

: display ( -- ) ;  \ update tft from display memory (ignored)

