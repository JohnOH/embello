\ ring buffers, for serial ports, etc - size must be 4..256 and power of 2
\ TODO setup is a bit messy right now, should put buffer: word inside init

\ each ring needs 4 extra bytes for internal housekeeping:
\   addr+0 = ring mask, i.e. N-1
\   addr+1 = get index: 0..255 (needs to be masked before use)
\   addr+2 = put index: 0..255 (needs to be masked before use)
\   addr+3 = spare
\   addr+4..addr+4+N-1 = actual ring buffer, N bytes
\ example:
\   16 4 + buffer: buf  buf 16 init-ring

: init-ring ( addr size -- )  \ initialise a ring buffer
  1- swap !  \ assumes little-endian so mask ends up in ring+0
;

: ++*c ( addr -- )  \ increment a byte value
  dup c@ 1+ swap c! ;

: ring-mask ( ring -- mask )  c@ ;

: ring-step ( ring 1/2 -- addr )  \ common code for saving and fetching
  over + ( ring ring-g/p ) dup c@ >r ( ring ring-g/p R: g/p ) ++*c
  dup ring-mask r> and swap 4 + + ;

: ring# ( ring -- u )  \ return current number of bytes in the ring buffer
\ TODO could be turned into a single @ word access and made interrupt-safe
  dup 1+ c@ over 2+ c@ - swap ring-mask and ;
: ring? ( ring -- f )  \ true if the ring can accept more data
  dup ring# swap ring-mask < ;
: >ring ( b ring -- )  \ save byte to end of ring buffer
  2 ring-step c! ;
: ring> ( ring -- b )  \ fetch byte from start of ring buffer
  1 ring-step c@ ;
