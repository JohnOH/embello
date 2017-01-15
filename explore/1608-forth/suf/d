\ load development code
\ needs l

\ reset
\ led ios!

\ needed for "h.4"
include ../flib/mecrisp/hexdump.fs

: -usb-io ( -- )
  ['] serial-key?  hook-key?  !
  ['] serial-key   hook-key   !
  ['] serial-emit? hook-emit? !
  ['] serial-emit  hook-emit  ! ;

\ in-memory output buffering

5000 buffer: outbuf
0 variable outpos

: buf-emit? true ;
: buf-emit outpos @ dup 5000 < if outbuf + c!  1 outpos +!  else drop then ;

: save-to-buf
  ['] buf-emit? hook-emit? !
  ['] buf-emit hook-emit !
  0 outpos ! ;
: restore-buf
  ['] serial-emit? hook-emit? !
  ['] serial-emit hook-emit !
  outbuf outpos @ type  0 outpos ! ;

: test-buf save-to-buf ." abc" [char] : serial-emit restore-buf ;

\ status and info dumps

: usb. ( -- )  \ dump USB info
  cr ." EP0R " 0 ep-addr h@ h.4
    ."  EP1R " 1 ep-addr h@ h.4
    ."  EP2R " 2 ep-addr h@ h.4
    ."  EP3R " 3 ep-addr h@ h.4
  cr ." CNTR " USB-CNTR h@ h.4
    ."  ISTR " USB-ISTR h@ h.4
     ."  FNR " USB-FNR h@ h.4
   ."  DADDR " USB-DADDR h@ h.4
  ."  BTABLE " USB-BTABLE h@ h.4 ;

: usb.mem ( -- )  \ dump packet buffer memory
  $150 0 do
    i $0F and 0= if cr i h.4 space then
    i $7 and 0= if space then
    i shl USBMEM + h@ h.4 space
  2 +loop ;

\ : try ( -- ) +usb-io begin usb-poll serial-key? until -usb-io ;
\ : t cr +usb try ( usb. usb.mem ) -usb cr cr ;
\ : t2 save-to-buf +usb try usb. usb.mem -usb restore-buf ;

\ : u1 ( n -- )  \ fill out-bound ring buffer with some test bytes
\   0 do [char] x usb-emit loop [char] / usb-emit usb-out-ring ring# . ;

\ vim: set ft=forth :
