\ long term RSSI measurement and auto-thresholding
0 variable rssi.n
0 variable rssi.sum
0 variable rssi.sumsqr
0 variable rssi.var
0 variable rssi.stddev
0 variable rssi.avg
0 variable rssi.max

: rssiclear ( -- )
  0 rssi.n ! 0 rssi.sum ! 0 rssi.sumsqr ! 0 rssi.max ! ;

: >rssi ( u -- ) \ every ms collect statistics for RSSI
    dup rssi.sum +!
    dup dup * rssi.sumsqr +!
    dup rssi.max @ > if rssi.max ! else drop then
    1 rssi.n +! ;

: rssivar ( -- )
  \ 32bits n<65000
  rssi.sum @ dup 8 lshift rssi.n @ / swap 8 rshift * rssi.sumsqr @ swap - rssi.n @ 1 - /
  rssi.var ! ;

: rssiavg ( -- )
  rssi.sum @ rssi.n @ / rssi.avg ! ;

: rssistddev ( -- )
  \ safe upto sqr(15)
  1 begin dup dup dup * rssi.var @ < swap 10 < and while 1+ repeat
  rssi.stddev ! ;

: rssicalc ( -- )
  rssivar rssiavg rssistddev ;

: rssithd
  rssi.var @ 64 < if \ original test 36 <
    rssi.stddev @ 2 * 6 max rssi.avg @ + \ original stddev 3 *
    dup ook.thd @ <> if ook-thd ." thd: " ook.thd @ . cr else drop then
  then ;

: rssiprint
  \ ." n: " rssi.n @ . 
  \ ." sum: " rssi.sum @ . 
  \ ." sumsqr: " rssi.sumsqr @ . 
  ." RSSI: " rssi.avg @ .
  ." (v" rssi.var @ . 
  ." s" rssi.stddev @ . 
  ." m" rssi.max @ . 
  ." )" 
  ." THD:" ook.thd @ . cr ;

: rssi ( -- )
  ook-rssi@ 255 swap - >rssi ;

: rssireport ( -- )
  \ RSSI statistics and auto threshold
  rssicalc rssithd rssiprint rssiclear ;
