\ try SD card access

forgetram

512 buffer: sd.buf

: sd-slow ( -- )  SPI1-CR1 @  %111000 or  SPI1-CR1 ! ;  \ clk/256

: sd-wait ( -- )  begin $FF >spi> dup . $FF = until ;

: sd-cmd ( cmd arg crc -- u )
  -rot swap
  cr millis . ." CMD" dup . 
  -spi 2 us +spi 2 us
            $FF >spi
         $40 or >spi
  dup 24 rshift >spi
  dup 16 rshift >spi
   dup 8 rshift >spi
                >spi
         $01 or >spi
  begin $FF >spi> dup $80 and while drop repeat ;

: sd-init ( -- )
  spi-init  sd-slow  10 0 do $FF >spi loop
  cr 0 ticks !
  begin
    0 0 $95 sd-cmd  \ CMD0 go idle
    dup .
  $01 = until

\ 1 0 $00 sd-cmd . sd-wait
  begin
    10 ms
    55 0 $01 sd-cmd . sd-wait
    41 0 $01 sd-cmd
  0= until

  cr millis . ." FAST "
  spi-init

\ 59 0 $00 sd-cmd . sd-wait
\ 8 $1AA $87 sd-cmd . sd-wait
\ 16 $200 $00 sd-cmd . sd-wait
;

\ 0 1 2 3 4 5 6 7 8 9 A B C D E F CRC
\ 002E00325B5A83A9FFFFFF80168000916616  Kingston 2GB
\ 007F00325B5A83D3F6DBFF81968000E7772B  SanDisk 2GB

: sd-size
  9 0 $00 sd-cmd . cr
  begin $FF >spi> dup . $FE = until
  cr 18 0 do $FF >spi> h.2 loop space ;

: sd-read ( page -- )  \ read one 512-byte page from sdcard
  9 lshift  17 swap $00 sd-cmd
  begin dup . $FE <> while $FF >spi> repeat
  512 0 do  $FF >spi> sd.buf i + c!  loop
  $FF >spi> h.2 $FF >spi> h.2 space
;

sd.buf 512 0 fill
( buffer: ) sd.buf hex.

sd-init sd-size

0 sd-read
0 sd-read
0 sd-read
sd.buf 512 dump
$1c6 2/ sd-read
sd.buf 128 dump
$574 2/ sd-read
sd.buf 128 dump
