\ SD Card interface using SPI w/ FAT access
\ uses spi

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
  $01 = until

  begin
    10 ms
    55 0 sd-cmd drop sd-wait
    41 0 sd-cmd
  0= until

  spi-init  \ revert to normal speed

\ 59 0 sd-cmd . sd-wait
\ 8 $1AA sd-cmd . sd-wait
\ 16 $200 sd-cmd . sd-wait
;

512 buffer: sd.buf
\ ( sd.buf: ) sd.buf hex.

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

\ FAT access

0 variable sd.fat   \ block # of first FAT copy
0 variable sd.spc   \ sectors per cluster (64)
0 variable sd.root  \ block # of first root sector
0 variable sd.#ent  \ number of root entries
0 variable sd.data  \ block offset of cluster #2

: sd-mount ( -- )  \ mount a FAT16 volume, extract the key disk info
                sd-init    \ initialise interface and card
              0 sd-read    \ read block #0
  sd.buf $1C6 + @          \ get location of boot sector
         dup 1+ sd.fat !   \ start sector of FAT area
            dup sd-read    \ read boot record
   sd.buf $0D + c@         \ sectors per cluster
                sd.spc !   \ depends on formatted disk size
   sd.buf $0E + h@         \ reserved sectors
   sd.buf $10 + c@         \ number of FAT copies
   sd.buf $16 + h@         \ sectors per fat
      * + + dup sd.root !  \ start sector of root directory
   sd.buf $11 + h@         \ max root entries
            dup sd.#ent !  \ save for later
     4 rshift + sd.data !  \ start sector of data area
;

: sd-mount. ( -- )  \ mount and show some basic card info
  sd-mount
  cr ." label: " sd.buf $2B + 11 type space
     ." format: " sd.buf $36 + 8 type space
     ." capacity: " sd.buf $20 + @ .
;

: dirent ( a -- a )  \ display one directory entry
  dup c@ $80 and 0= over 2+ c@ and if
    cr dup 11 type space
    dup 11 + c@ h.2 space
    dup 26 + h@ .
    dup 28 + @ .
  then ;

: ls  ( -- ) \ display files in root dir (skipping all LFNs and deleted files)
  sd.buf 512 +
  sd.#ent @ 0 do
    i $F and 0= if
      sd.root @ i 4 rshift + sd-read
      512 -
    then
    dirent
    32 +
  loop drop ;

: fat-find ( addr -- u )  \ find entry by name, return data cluster, else $FFFF
  sd.buf 512 +
  sd.#ent @ 0 do
    i $F and 0= if
      sd.root @ i 4 rshift + sd-read
      512 -
    then
    2dup 11 tuck compare
    if nip 26 + h@ unloop exit then
    32 +
  loop 2drop $FFFF ;

: fat-next ( u -- u )  \ return next FAT cluster, or $FFFx at end
  \ TODO hard-coded for 64 sec / 32 KB per cluster
  dup 8 rshift sd.fat @ + sd-read
  $FF and 2* sd.buf + h@ ;

: chain. ( u -- )  \ display the chain of clusters
  begin
    dup .
  dup $F or $FFFF <> while
    fat-next
  repeat drop ;

\ 128 clusters is 8 MB when the cluster size is 64
4 constant NFILES
129 2* NFILES * buffer: fat.maps  \ room for file maps of max 128 clusters

: file ( n -- a )  \ convert file 0..3 to a map address inside fat.maps
  129 2* * fat.maps + ;

: fat-chain ( u n -- )  \ store clusters for use as file map n
  file
  begin
    2dup ! 2+
  over $F or $FFFF <> while
    swap fat-next swap
  repeat 2drop ;

: fat-map ( n1 n2 -- n )  \ map block n1 to raw block number, using file n2 map
  file over sd.spc @ / 2* + h@
  2- sd.spc @ * swap sd.spc @ 1- and +
  sd.data @ + ;
