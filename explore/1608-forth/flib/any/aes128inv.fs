\ 128-bit AES Decryption
\ Author: SevenW from sevenwatt.com
\ Date  : 2017-Feb-23
\ 
\ Description:
\ AES128 block cipher decryption
\ Implementation is optimized for (low) memory usage.
\ 
\ Requires : aes128.fs
\ 
\ Usage    : ( c-addr key ) -aes
\ With     : c-addr : input data in a 16-byte buffer
\            key    : the orignal encryption key in 16-bytes 
\ Output   : Decryption is in-situ so the 16-byte input data buffer contains the decrypted output.

create s.box.inv
hex
  52 c, 09 c, 6A c, D5 c, 30 c, 36 c, A5 c, 38 c, BF c, 40 c, A3 c, 9E c, 81 c, F3 c, D7 c, FB c,
  7C c, E3 c, 39 c, 82 c, 9B c, 2F c, FF c, 87 c, 34 c, 8E c, 43 c, 44 c, C4 c, DE c, E9 c, CB c,
  54 c, 7B c, 94 c, 32 c, A6 c, C2 c, 23 c, 3D c, EE c, 4C c, 95 c, 0B c, 42 c, FA c, C3 c, 4E c,
  08 c, 2E c, A1 c, 66 c, 28 c, D9 c, 24 c, B2 c, 76 c, 5B c, A2 c, 49 c, 6D c, 8B c, D1 c, 25 c,
  72 c, F8 c, F6 c, 64 c, 86 c, 68 c, 98 c, 16 c, D4 c, A4 c, 5C c, CC c, 5D c, 65 c, B6 c, 92 c,
  6C c, 70 c, 48 c, 50 c, FD c, ED c, B9 c, DA c, 5E c, 15 c, 46 c, 57 c, A7 c, 8D c, 9D c, 84 c,
  90 c, D8 c, AB c, 00 c, 8C c, BC c, D3 c, 0A c, F7 c, E4 c, 58 c, 05 c, B8 c, B3 c, 45 c, 06 c,
  D0 c, 2C c, 1E c, 8F c, CA c, 3F c, 0F c, 02 c, C1 c, AF c, BD c, 03 c, 01 c, 13 c, 8A c, 6B c,
  3A c, 91 c, 11 c, 41 c, 4F c, 67 c, DC c, EA c, 97 c, F2 c, CF c, CE c, F0 c, B4 c, E6 c, 73 c,
  96 c, AC c, 74 c, 22 c, E7 c, AD c, 35 c, 85 c, E2 c, F9 c, 37 c, E8 c, 1C c, 75 c, DF c, 6E c,
  47 c, F1 c, 1A c, 71 c, 1D c, 29 c, C5 c, 89 c, 6F c, B7 c, 62 c, 0E c, AA c, 18 c, BE c, 1B c,
  FC c, 56 c, 3E c, 4B c, C6 c, D2 c, 79 c, 20 c, 9A c, DB c, C0 c, FE c, 78 c, CD c, 5A c, F4 c,
  1F c, DD c, A8 c, 33 c, 88 c, 07 c, C7 c, 31 c, B1 c, 12 c, 10 c, 59 c, 27 c, 80 c, EC c, 5F c,
  60 c, 51 c, 7F c, A9 c, 19 c, B5 c, 4A c, 0D c, 2D c, E5 c, 7A c, 9F c, 93 c, C9 c, 9C c, EF c,
  A0 c, E0 c, 3B c, 4D c, AE c, 2A c, F5 c, B0 c, C8 c, EB c, BB c, 3C c, 83 c, 53 c, 99 c, 61 c,
  17 c, 2B c, 04 c, 7E c, BA c, 77 c, D6 c, 26 c, E1 c, 69 c, 14 c, 63 c, 55 c, 21 c, 0C c, 7D c,
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
  round-key+ ;

\ decrypt block
: aes-inv ( c-addr key -- )  \ aes128 decrypt block
  \ key-in
  expand-key
  dup >aes
  11 round-key-lut
  round-key+
  1 10 do i ~one-round -1 +loop
  \ input block is dechiphered in-situ
  ( data ) aes> ;
