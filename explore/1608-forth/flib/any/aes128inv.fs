\ 128-bit AES Decryption
\ Author: SevenW from sevenwatt.com
\ Date  : 2017-Feb-23
\ 
\ Description:
\ AES128 block cipher decryption
\ Implementation is optimized on (low) memory usage.
\ For AES-CTR and AES-CMAC as used in LoraWAN, this module is NOT needed.
\ A testmodule is provided in the file aes128invtest.fs
\ 
\ Requires : aes128.fs
\ 
\ Usage    : ( c-addr key ) -aes
\ With     : c-addr : input data in a 16-byte buffer
\            key    : the orignal encryption key in 16-bytes 
\ Output   : Decryption is in-situ so the 16-byte input data buffer contains the decrypted output.
\ 
\ 

hex
create s.box.inv
  52 C, 09 C, 6A C, D5 C, 30 C, 36 C, A5 C, 38 C, BF C, 40 C, A3 C, 9E C, 81 C, F3 C, D7 C, FB C,
  7C C, E3 C, 39 C, 82 C, 9B C, 2F C, FF C, 87 C, 34 C, 8E C, 43 C, 44 C, C4 C, DE C, E9 C, CB C,
  54 C, 7B C, 94 C, 32 C, A6 C, C2 C, 23 C, 3D C, EE C, 4C C, 95 C, 0B C, 42 C, FA C, C3 C, 4E C,
  08 C, 2E C, A1 C, 66 C, 28 C, D9 C, 24 C, B2 C, 76 C, 5B C, A2 C, 49 C, 6D C, 8B C, D1 C, 25 C,
  72 C, F8 C, F6 C, 64 C, 86 C, 68 C, 98 C, 16 C, D4 C, A4 C, 5C C, CC C, 5D C, 65 C, B6 C, 92 C,
  6C C, 70 C, 48 C, 50 C, FD C, ED C, B9 C, DA C, 5E C, 15 C, 46 C, 57 C, A7 C, 8D C, 9D C, 84 C,
  90 C, D8 C, AB C, 00 C, 8C C, BC C, D3 C, 0A C, F7 C, E4 C, 58 C, 05 C, B8 C, B3 C, 45 C, 06 C,
  D0 C, 2C C, 1E C, 8F C, CA C, 3F C, 0F C, 02 C, C1 C, AF C, BD C, 03 C, 01 C, 13 C, 8A C, 6B C,
  3A C, 91 C, 11 C, 41 C, 4F C, 67 C, DC C, EA C, 97 C, F2 C, CF C, CE C, F0 C, B4 C, E6 C, 73 C,
  96 C, AC C, 74 C, 22 C, E7 C, AD C, 35 C, 85 C, E2 C, F9 C, 37 C, E8 C, 1C C, 75 C, DF C, 6E C,
  47 C, F1 C, 1A C, 71 C, 1D C, 29 C, C5 C, 89 C, 6F C, B7 C, 62 C, 0E C, AA C, 18 C, BE C, 1B C,
  FC C, 56 C, 3E C, 4B C, C6 C, D2 C, 79 C, 20 C, 9A C, DB C, C0 C, FE C, 78 C, CD C, 5A C, F4 C,
  1F C, DD C, A8 C, 33 C, 88 C, 07 C, C7 C, 31 C, B1 C, 12 C, 10 C, 59 C, 27 C, 80 C, EC C, 5F C,
  60 C, 51 C, 7F C, A9 C, 19 C, B5 C, 4A C, 0D C, 2D C, E5 C, 7A C, 9F C, 93 C, C9 C, 9C C, EF C,
  A0 C, E0 C, 3B C, 4D C, AE C, 2A C, F5 C, B0 C, C8 C, EB C, BB C, 3C C, 83 C, 53 C, 99 C, 61 C,
  17 C, 2B C, 04 C, 7E C, BA C, 77 C, D6 C, 26 C, E1 C, 69 C, 14 C, 63 C, 55 C, 21 C, 0C C, 7D C,
decimal

\ lookup byte in s.box
: ~s.b@ ( b -- b ) dup $0F and swap 4 rshift $0F and 16 * + s.box.inv + c@ ;

\ substitute scratch with bytes from s.box
: ~s.b-all@ scratch 16 0 do dup i + dup c@ ~s.b@ swap c! loop drop ;

\ shift bytes
: r1>
  4 scratch + dup 3 + c@ swap
  1 3 do dup i + dup 1- c@ swap c! -1 +loop c! ;
: r2>
  8 scratch + dup 2+ dup c@ swap
  dup 2- dup -rot c@ swap c! c!
  ( i0 ) 3 + dup c@ swap
  dup 2- dup -rot c@ swap c! c! ;
: r3>
  12 scratch + dup c@ swap
  3 0 do dup i + dup 1+ c@ swap c! loop 3 + c! ;
: ~sh-bytes r1> r2> r3> ;

\ galios field multiplication
: gmul ( a b -- p )
  0 8 0 do
    ( a b p ) over ( b ) 1 and if                        \ if bit 1 of b set
      ( a b p) rot ( b p a ) swap over xor               \ p = p xor a (multiply)
      ( b a p' ) swap  ( b p' a ) -rot ( a b p' ) then
      ( a b p ) rot ( b p a ) gfmod ( b p a' )           \ a*2 GF modulo
      rot ( p a' b ) 1 rshift rot ( a' b' p )            \ b/2
  loop -rot drop drop ( p );

\ multiply column with vector [ 14, 9, 13, 11 ] for decryption
: ~mix1col ( c )
  4 0 do ( c ) dup i 4 * + scratch + c@ m1 i + c! loop \ copy column to m1
  ( c ) m1 
   dup  c@  14 gmul  over 3 + c@ 9 gmul xor over 2+ c@ 13 gmul xor  over 1+ c@ 11 gmul xor m2 c!
  dup 1+ c@ 14 gmul   over c@ 9 gmul xor     over 3 + c@ 13 gmul xor over 2+ c@ 11 gmul xor m2 1+ c!
  dup 2+ c@ 14 gmul   over 1+ c@ 9 gmul xor  over c@ 13 gmul xor     over 3 + c@ 11 gmul xor m2 2+ c!
  dup 3 + c@ 14 gmul  over 2+ c@ 9 gmul xor  over 1+ c@ 13 gmul xor    swap   c@ 11 gmul xor m2 3 + c!
  4 0 do m2 i + c@ over i 4 * + scratch + c! loop drop  ; \ copy m2 to column

: ~mix-col (  ) 
  4 0 do i ~mix1col loop ; 

16 11 * buffer: round.keys

: expand-key ( key )
  key-in
  round.key round.keys 16 move
  \ round.key h.16
  11 1 do 
    i round-key
    round.key round.keys i 16 * + 16 move
    \ round.key h.16
  loop ;

: round-key-lut ( round -- )
  1- 16 * round.keys + round.key 16 move ;
  
: ~one-round ( round )
  dup 10 <> if ~mix-col then
  ~sh-bytes
  ~s.b-all@
  ( round ) round-key-lut
  round-key+
  ;

\ decrypt block
: ~aes ( c-addr key -- ) \ aes128 decrypt block
  \ key-in
  expand-key
  dup >aes
  11 round-key-lut
  round-key+
  1 10 do i ~one-round -1 +loop
  \ input block is dechiphered in-situ
  ( data ) aes> ;

: -aes ( c-addr key -- ) \ aes128 decrypt block (identical to ~aes)
  ~aes ;
