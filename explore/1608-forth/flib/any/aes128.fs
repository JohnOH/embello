\ 128-bit AES
\ Author: SevenW from sevenwatt.com
\ Date  : 2017-Feb-21
\ 
\ Description:
\ AES128 block cipher encryption
\ Implementation is optimized on (low) memory usage.
\ For AES-CTR and AES-CMAC as used in LoraWAN, only AES-encryption is needed.
\ Decryption is suported in the separate file: aes128inv.fs
\ A testmodule is provided in the file aes128test.fs
\ 
\ Usage    : ( c-addr key ) +aes
\ With     : c-addr : input data in a 16-byte buffer
\            key    : the encryption key in 16-bytes 
\ Output   : Encryption is in-situ so the 16-byte input data buffer contains the encrypted output.
\ 
\ 

hex
create s.box
  63 C, 7C C, 77 C, 7B C, F2 C, 6B C, 6F C, C5 C, 30 C, 01 C, 67 C, 2B C, FE C, D7 C, AB C, 76 C,
  CA C, 82 C, C9 C, 7D C, FA C, 59 C, 47 C, F0 C, AD C, D4 C, A2 C, AF C, 9C C, A4 C, 72 C, C0 C,
  B7 C, FD C, 93 C, 26 C, 36 C, 3F C, F7 C, CC C, 34 C, A5 C, E5 C, F1 C, 71 C, D8 C, 31 C, 15 C,
  04 C, C7 C, 23 C, C3 C, 18 C, 96 C, 05 C, 9A C, 07 C, 12 C, 80 C, E2 C, EB C, 27 C, B2 C, 75 C,
  09 C, 83 C, 2C C, 1A C, 1B C, 6E C, 5A C, A0 C, 52 C, 3B C, D6 C, B3 C, 29 C, E3 C, 2F C, 84 C,
  53 C, D1 C, 00 C, ED C, 20 C, FC C, B1 C, 5B C, 6A C, CB C, BE C, 39 C, 4A C, 4C C, 58 C, CF C,
  D0 C, EF C, AA C, FB C, 43 C, 4D C, 33 C, 85 C, 45 C, F9 C, 02 C, 7F C, 50 C, 3C C, 9F C, A8 C,
  51 C, A3 C, 40 C, 8F C, 92 C, 9D C, 38 C, F5 C, BC C, B6 C, DA C, 21 C, 10 C, FF C, F3 C, D2 C,
  CD C, 0C C, 13 C, EC C, 5F C, 97 C, 44 C, 17 C, C4 C, A7 C, 7E C, 3D C, 64 C, 5D C, 19 C, 73 C,
  60 C, 81 C, 4F C, DC C, 22 C, 2A C, 90 C, 88 C, 46 C, EE C, B8 C, 14 C, DE C, 5E C, 0B C, DB C,
  E0 C, 32 C, 3A C, 0A C, 49 C, 06 C, 24 C, 5C C, C2 C, D3 C, AC C, 62 C, 91 C, 95 C, E4 C, 79 C,
  E7 C, C8 C, 37 C, 6D C, 8D C, D5 C, 4E C, A9 C, 6C C, 56 C, F4 C, EA C, 65 C, 7A C, AE C, 08 C,
  BA C, 78 C, 25 C, 2E C, 1C C, A6 C, B4 C, C6 C, E8 C, DD C, 74 C, 1F C, 4B C, BD C, 8B C, 8A C,
  70 C, 3E C, B5 C, 66 C, 48 C, 03 C, F6 C, 0E C, 61 C, 35 C, 57 C, B9 C, 86 C, C1 C, 1D C, 9E C,
  E1 C, F8 C, 98 C, 11 C, 69 C, D9 C, 8E C, 94 C, 9B C, 1E C, 87 C, E9 C, CE C, 55 C, 28 C, DF C,
  8C C, A1 C, 89 C, 0D C, BF C, E6 C, 42 C, 68 C, 41 C, 99 C, 2D C, 0F C, B0 C, 54 C, BB C, 16 C,
decimal

\ allocate working memory in RAM
16 buffer: scratch

\ lookup byte in s.box
: s.b@ ( b -- b ) dup $0F and swap 4 rshift $0F and 16 * + s.box + c@ ;

\ substitute scratch with bytes from s.box
: s.b-all@ scratch 16 0 do dup i + dup c@ s.b@ swap c! loop drop ;

\ shift bytes
: r1<
  4 scratch + dup c@ swap
  3 0 do dup i + dup 1+ c@ swap c! loop 3 + c! ;
: r2<
  8 scratch + dup dup c@ swap
  dup 2+ dup -rot c@ swap c! c!
  1+ dup c@ swap
  dup 2+ dup -rot c@ swap c! c! ;
: r3<
  12 scratch + dup 3 + c@ swap
  1 3 do dup i + dup 1- c@ swap c! -1 +loop c! ;
: sh-bytes r1< r2< r3< ;

\ mix columns
4 buffer: m1
4 buffer: m2

\ galios field mulitply modulus
: gfmod ( a -- a' )
  ( a ) dup 1 lshift ( a 2a ) swap ( 2a a ) $80 and $80 = if $1B xor ( 2a' ) then ( 2a ) ;

\ multiply column with vector [ 2, 3, 1, 1 ] for encryption
: m1m2
  ( r c )       over tuck ( r r c r ) 4 * + scratch + c@
  ( r r b1 )    dup gfmod
  ( r r b1 b2 ) rot ( r b1 b2 r ) m2 + c!
  ( r b1 )      swap m1 + c! ;

: mixing
  m2 c@ m1 1+ c@ m2 1+ c@ m1 2+ c@ m1 3 + c@ xor xor xor xor scratch i + c!
  m1 c@ m2 1+ c@ m1 2+ c@ m2 2+ c@ m1 3 + c@ xor xor xor xor scratch 4 + i + c!
  m1 c@ m1 1+ c@ m2 2+ c@ m1 3 + c@ m2 3 + c@ xor xor xor xor scratch 8 + i + c!
  m1 c@ m2 c@ m1 1+ c@ m1 2+ c@ m2 3 + c@ xor xor xor xor scratch 12 + i + c! ;

: mix-col
  4 0 do
    4 0 do i j m1m2 loop
    mixing
  loop ;

\ Round key (rk)
16 buffer: round.key
4 buffer: rk-val

: rk-init
  round.key 12 + rk-val 4 move ;

: rk-rotsub
  rk-val dup c@ s.b@ swap
  3 0 do dup i + dup 1+ c@ s.b@ swap c! loop 3 + c! ;

\ calculate rcon
: rcon ( round -- rcon )
  1 swap
  ( rcon round ) begin dup 1 <> while 
    swap dup $80 and swap 1 lshift swap $80 = if $1B xor then
    swap 1- 
  repeat drop ;

: xor-rcon ( round -- )
  \ only first byte xor
  rk-val dup c@ rot rcon xor swap c! ;

: rk-calc
  ( ii io )          4 * over + round.key + 
  ( ii rki )         swap rk-val +
  ( rki rkvali )     over c@ over c@ xor
  ( rki rkvali val ) tuck swap c! swap c! ;

: rk-update
  4 0 do
    4 0 do i j rk-calc loop
  loop ;  

\ Rijndael (incremental) key expansion
: round-key ( round -- )
  rk-init rk-rotsub xor-rcon rk-update ;

: rk+calc
  ( r c ) over over swap 4 * + scratch + -rot
  ( scratch-idx r c ) 4 * + round.key +
  ( scratch-idx rk-idx ) c@ over c@ xor swap c! ;

\ add round key
: round-key+
  4 0 do
    4 0 do i j rk+calc loop
  loop ;

\ move input block to scratch  
: >aes ( caddr -- ) \ input data block (16-bytes)
  scratch swap 
  4 0 do
    4 0 do
       ( scratch data )over over i + j 4 * + swap
       ( scratch data data-idx scratch) i 4 * + j + swap
       ( scratch data scratch-idx data-idx ) c@ swap c!
    loop
  loop drop drop ; 
 
\ move scratch to output block
: aes> ( caddr -- ) \ output encrypted data block (16-bytes)
  scratch 
  4 0 do
    4 0 do
       ( data scratch )over over i 4 * + j + swap
       ( data scratch scratch-idx data) i + j 4 * + swap
       ( data scratch data-idx scratch-idx ) c@ swap c! 
    loop
  loop drop drop ; 
 
\ store the first round key
: key-in ( caddr -- )
  round.key 16 move ;

\ perform one round of encryption
: one-round ( round )
  s.b-all@
  sh-bytes
  dup 10 <> if mix-col then
  ( round ) round-key
  round-key+ ;

\ encrypt block
: aes ( c-addr key -- ) \ aes128 encrypt block
  key-in
  dup >aes
  round-key+
  11 1 do i one-round loop
  \ input block is chiphered in-situ
  ( c-addr ) aes> ;

\ alias
: +aes ( c-addr key -- ) \ aes128 encrypt block (identical to aes)
  aes ; 
