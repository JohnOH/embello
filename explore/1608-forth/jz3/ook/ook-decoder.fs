\ Decoder
  32 constant OD.SMAX
  0 constant OD.IDLE
  1 constant OD.DONE
  2 constant OD.OK
  3 constant OD.T0
  4 constant OD.T1
  5 constant OD.T2
  6 constant OD.T3

  create od.stream od.SMAX allot
  align

  0 variable od.nbits
  0 variable od.flip
  0 variable od.state
  0 variable od.signal
  0 variable od.width
  
: od.reset ( -- )
   0 od.nbits ! 0 od.flip ! OD.IDLE od.state ! 1 od.width ! ;

: od>bit ( b -- ) \ add one bit to the stream buffer
  \ lsbit received first - original jeelib convention
  \ od.nbits @ dup 7 and 1 swap lshift swap 3 rshift od.stream +
  \ msbit received first - frequently used bit order
  od.nbits @ dup 7 and $80 swap rshift swap 3 rshift od.stream +
  ( b mask c-addr ) rot 0= if cbic! else cbis! then
  1 od.nbits +!
  od.nbits @ OD.SMAX 3 lshift >= if od.reset else OD.OK od.state ! then ;

: od.pad ( -- ) \ pad the remaining bits in last byte
  8 od.nbits @ 7 and do 0 od>bit loop ;

: od.w ." w" od.width @ . ; \ print width for debug purposes

: fs20.decode ( width -- flag ) \ decode stream to bits
  dup dup 200 > swap 875 < and if
    \ ook.rssiprint
    ( width ) 500 >= \ islong @
    od.state @ case
      OD.IDLE of
        ( islong ) if
          od.flip @ 18 > if OD.T1 od.state ! else od.reset then
        else
          1 od.flip +!
        then
      endof
      OD.OK of ( islong ) if OD.T1 else OD.T0 then od.state ! endof
      OD.T0 of 0 od>bit ( islong ) if od.reset then endof
      OD.T1 of 1 od>bit ( islong ) not if od.reset then endof
    endcase
    false
  else
    ( width ) 1500 >= od.nbits @ 40 >= and dup if od.pad OD.DONE od.state ! else od.reset then
  then ;

: fs20>stream ( width -- flag ) \ stream one interval into decoder
  dup od.width !
  od.state @ OD.DONE <> if fs20.decode then ;

: fs20>rstream ( width signal rssi -- flag ) \ stream one interval into decoder

  dup od.width !
  od.state @ OD.DONE <> if fs20.decode then ;

: fs20.print
  ." FS20 " 
  od.nbits @ 7 + 3 rshift 20 umin  0 do od.stream i + c@ h.2 loop ."  "
  ook.rssi.print
  ;
