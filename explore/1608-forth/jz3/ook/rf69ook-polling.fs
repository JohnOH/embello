\ .
\ OOK receiver
\ .

\ JNZ: keep R.MSBEATS at 32 for both fast millis and micros
\ until micros slowness bug is fixed.
5 constant R.2^MSBEATS
1 R.2^MSBEATS lshift constant R.MSBEATS \ power of two for fast millis function
\ : millis ticks @ R.2^MSBEATS rshift ;
\ for tsample = 32us a fast, but low resolution micros (for the time being)
\ : micros ticks @ R.2^MSBEATS lshift ;


1000 / R.MSBEATS constant R.TSAMPLE
\ : beat ticks @ R.TSAMPLE * ;


: setup
  16MHz 1000 dup systick-hz ." systick-hz: " . cr
  \ due to slowness bug in micros, use non-standard systick.
  \ 16MHz 1000000 R.TSAMPLE / dup systick-hz ." systick-hz: " . cr
  IMODE-FLOAT DIO2 io-mode!
  868280 ook-init
  micros ook.ts ! ;

0 variable r.ts \ receiver loop timestamp
10000 constant R.MSSTAT
0 variable r.cnt
0 variable r.flips

5000 R.TSAMPLE / constant FLUSH.MAX
0 variable flush.cnt

0 variable rssi.ts
0 variable rssi.tss

: r.rssi ( --  ) 
  \ RSSI statistics and auto threshold
  millis rssi.ts @ <> if rssi millis rssi.ts ! then ;

: r.report ( -- flag ) \ returns true if rssi reported
  \ RSSI statistics and auto threshold
  millis rssi.tss @ - R.MSSTAT > if 
    rssireport millis rssi.tss ! 
    r.cnt @ R.MSSTAT 1000 * over / swap
    ." cnt: " . ." dur: " . ." us: - flips: " r.flips @ . cr
    0 r.cnt ! 0 r.flips !
  then ;

: r.stream ( b -- )
  fs20>stream if fs20.print cr od.reset then ;

: r.sample
  ook-rssi@ 255 swap - ook>rssi>delay
  ook-dio2 ook-filter
  ( rssi signal ) 
  dup ook.dio2 @ <> if 
    micros dup ook.ts @ - swap ook.ts ! r.stream
    over over swap ( rssi signal signal rssi ) ook>rssi
    0 flush.cnt !
    1 r.flips +!
  then
  ook.dio2 ! ( rssi ) drop ;

: r.flush
  flush.cnt @ FLUSH.MAX = if
    5000 r.stream 1 r.stream
  then
  1 flush.cnt +! ;


: receiver
  millis r.ts !
  millis rssi.ts !
  millis rssi.tss !
  0 r.cnt !

  begin
    micros
    r.sample \ poll the DIO2 signal and process
    r.rssi   \ RSSI statistics and auto threshold
    r.report
    r.flush
    \ \ micros swap - R.TSAMPLE < if sleep then
    micros swap - 8 + dup R.TSAMPLE < if R.TSAMPLE swap - us else drop then
    1 r.cnt +!
  key? until 
  ;

: main
  setup
  receiver
  ;

: t1 micros 20 us micros swap - . ;


