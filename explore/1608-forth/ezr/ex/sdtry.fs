\ try SD card access

forgetram

: sd-slow ( -- )  SPI1-CR1 @  %111000 or  SPI1-CR1 ! ;  \ clk/256

: sd-wait ( -- )  begin $FF >spi> ( dup . ) $FF = until ;

: sd-cmd ( cmd arg -- u )
  swap
\ cr millis . ." CMD" dup . 
  -spi 2 us +spi
            $FF >spi
         $40 or >spi
  dup 24 rshift >spi
  dup 16 rshift >spi
   dup 8 rshift >spi
                >spi
            $95 >spi
  begin $FF >spi> dup $80 and while drop repeat ;

: sd-init ( -- )
  spi-init  sd-slow  10 0 do $FF >spi loop
  0 ticks !
  begin
    0 0 sd-cmd  \ CMD0 go idle
\   dup .
  $01 = until

\ 1 0 sd-cmd . sd-wait
  begin
    10 ms
    55 0 sd-cmd drop sd-wait
    41 0 sd-cmd
  0= until

\ cr millis . ." FAST "
  spi-init

\ 59 0 sd-cmd . sd-wait
\ 8 $1AA sd-cmd . sd-wait
\ 16 $200 sd-cmd . sd-wait
;

512 buffer: sd.buf
( sd.buf: ) sd.buf hex.

: sd-copy ( f n -- )
  swap begin ( dup . ) $FE <> while $FF >spi> repeat
  0 do  $FF >spi> sd.buf i + c!  loop
  $FF dup >spi >spi ;

\ 0 1 2 3 4 5 6 7 8 9 A B C D E F CRC
\ 002E00325B5A83A9FFFFFF80168000916616  Kingston 2GB
\ 007F00325B5A83D3F6DBFF81968000E7772B  SanDisk 2GB

: sd-size ( -- n )  \ return card size in 512-byte blocks
  9 0 sd-cmd  16 sd-copy
\ http://www.avrfreaks.net/forum/how-determine-mmc-card-size
\ https://members.sdcard.org/downloads/pls/simplified_specs/archive/part1_301.pdf
\ TODO bytes 6 and 8 may be reversed...
\ sd.buf 7 + c@ $FF and dup . hex.
\ sd.buf 8 + c@ 6 rshift dup . hex.
  sd.buf 6 + c@ $03 and 10 lshift
  sd.buf 7 + c@ 2 lshift or
  sd.buf 8 + c@ 6 rshift or ;

: sd-read ( page -- )  \ read one 512-byte page from sdcard
  9 lshift  17 swap sd-cmd  512 sd-copy ;

: sd-write ( page -- )  \ write one 512-byte page to sdcard
  9 lshift  24 swap sd-cmd drop
  $FF >spi $FE >spi
  512 0 do  sd.buf i + c@ >spi  loop
  $FF dup >spi >spi  sd-wait ;

( blocks: ) sd-init sd-size .
sd.buf 16 dump

0 sd-read
0 sd-read
0 sd-read
sd.buf 512 dump
$1c6 2/ sd-read
sd.buf 128 dump
$574 2/ sd-read
sd.buf 128 dump
