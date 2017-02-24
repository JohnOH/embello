\ 128-bit AES Encryption
\ Author: SevenW from sevenwatt.com
\ Date  : 2017-Feb-21
\ 
\ Description:
\ AES128 block cipher encryption
\ Implementation is optimized on (low) memory usage.
\ 
\ Usage    : ( c-addr key ) +aes
\ With     : c-addr : input data in a 16-byte buffer
\            key    : the encryption key in 16-bytes 
\ Output   : Encryption is in-situ so the 16-byte input data buffer contains the encrypted output.

create s.box
hex
  63 c, 7C c, 77 c, 7B c, F2 c, 6B c, 6F c, C5 c, 30 c, 01 c, 67 c, 2B c, FE c, D7 c, AB c, 76 c,
  CA c, 82 c, C9 c, 7D c, FA c, 59 c, 47 c, F0 c, AD c, D4 c, A2 c, AF c, 9C c, A4 c, 72 c, C0 c,
  B7 c, FD c, 93 c, 26 c, 36 c, 3F c, F7 c, CC c, 34 c, A5 c, E5 c, F1 c, 71 c, D8 c, 31 c, 15 c,
  04 c, C7 c, 23 c, C3 c, 18 c, 96 c, 05 c, 9A c, 07 c, 12 c, 80 c, E2 c, EB c, 27 c, B2 c, 75 c,
  09 c, 83 c, 2C c, 1A c, 1B c, 6E c, 5A c, A0 c, 52 c, 3B c, D6 c, B3 c, 29 c, E3 c, 2F c, 84 c,
  53 c, D1 c, 00 c, ED c, 20 c, FC c, B1 c, 5B c, 6A c, CB c, BE c, 39 c, 4A c, 4C c, 58 c, CF c,
  D0 c, EF c, AA c, FB c, 43 c, 4D c, 33 c, 85 c, 45 c, F9 c, 02 c, 7F c, 50 c, 3C c, 9F c, A8 c,
  51 c, A3 c, 40 c, 8F c, 92 c, 9D c, 38 c, F5 c, BC c, B6 c, DA c, 21 c, 10 c, FF c, F3 c, D2 c,
  CD c, 0C c, 13 c, EC c, 5F c, 97 c, 44 c, 17 c, C4 c, A7 c, 7E c, 3D c, 64 c, 5D c, 19 c, 73 c,
  60 c, 81 c, 4F c, DC c, 22 c, 2A c, 90 c, 88 c, 46 c, EE c, B8 c, 14 c, DE c, 5E c, 0B c, DB c,
  E0 c, 32 c, 3A c, 0A c, 49 c, 06 c, 24 c, 5C c, C2 c, D3 c, AC c, 62 c, 91 c, 95 c, E4 c, 79 c,
  E7 c, C8 c, 37 c, 6D c, 8D c, D5 c, 4E c, A9 c, 6C c, 56 c, F4 c, EA c, 65 c, 7A c, AE c, 08 c,
  BA c, 78 c, 25 c, 2E c, 1C c, A6 c, B4 c, C6 c, E8 c, DD c, 74 c, 1F c, 4B c, BD c, 8B c, 8A c,
  70 c, 3E c, B5 c, 66 c, 48 c, 03 c, F6 c, 0E c, 61 c, 35 c, 57 c, B9 c, 86 c, C1 c, 1D c, 9E c,
  E1 c, F8 c, 98 c, 11 c, 69 c, D9 c, 8E c, 94 c, 9B c, 1E c, 87 c, E9 c, CE c, 55 c, 28 c, DF c,
  8C c, A1 c, 89 c, 0D c, BF c, E6 c, 42 c, 68 c, 41 c, 99 c, 2D c, 0F c, B0 c, 54 c, BB c, 16 c,
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
: aes ( c-addr key -- )  \ aes128 encrypt block
  key-in
  dup >aes
  round-key+
  11 1 do i one-round loop
  \ input block is chiphered in-situ
  ( c-addr ) aes> ;

\ 128-bit AES CTR and AES CMAC as used in LoraWAN
\ Author: SevenW from sevenwatt.com
\ Date  : 2017-Feb-23
\ 
\ Description:
\ AES128 CTR en- and de-cryption of a stream buffer
\ AES128 CMAC hash key calculation
\ 
\ Requires : aes128.fs
\ 
\ Usage    : ( buf len key iv -- ) aes-ctr
\ With     : buf        : c-addr input data in a byte buffer
\            len        : the length of the encrypted data
\            key        : c-addr of the encryption key
\            iv         : c-addr of the initialization vector
\ Output   : Encryption is in-situ so the input data buffer contains the encrypted output.
\ Note     : Decryption is achieved by calling the encrypted data with the same IV and key.
\ 
\ Usage    : ( buf len key iv -- mic ) aes-cmac
\ With     : buf        : c-addr input data in a byte buffer
\            len        : the length of the encrypted data
\            key        : c-addr of the encryption key
\            iv         : c-addr of the initialization vector
\ Output   : mic        : c-addr of the mac. Lora uses the first four bytes as 32-bit mic

16 buffer: AESkey
16 buffer: AESaux

\ AES-CTR

16 buffer: ctr
0 variable buf-addr
0 variable buf-seg

: xor-buf-key+1 ( buf key -- buf+1 key+1 )
    ( buf key ) over c@ ( buf key val ) over c@ xor ( buf key val2 ) rot ( key val2 buf ) tuck ( key buf val2 buf ) c! 
    ( key buf ) 1+ swap 1+ \ increment pointers
    ( buf key ) ;

: aes-ctr-int ( buf-addr buf-len -- ) \ AES-CTR encrypt buffer.
  ctr swap
  ( buf ctr len ) 0 do
    i $0F and 0= if
      ( buf ctr ) drop ctr              \ reset ctr to bit 0
      ( buf ctr ) AESaux over 16 move   \ copy AESaux to ctr
      ( buf ctr ) dup AESkey aes        \ get exncrypted ctr
      ( buf ctr ) 1 AESaux 15 + c+!     \ used to be at end of loop but ctr contains the right key
    then
    ( buf ctr ) xor-buf-key+1
    ( buf+1 ctr+1 )
  loop drop drop ;

: aes-ctr ( buf len key iv )  \ AES-CTR encrypt buffer, encryption is in-situ
                              \ buf: c-addr of buffer to be encrypted
                              \ len: encryption length
                              \ key: c-addr of 128-bit encryption key
                              \ iv : c-addr of 16-byte initialization vector
  ( iv  ) AESaux 16 move
  ( key ) AESkey 16 move
  ( buf len ) aes-ctr-int ;

\ AES-CMAC

16 buffer: final.key
false variable padding
0 variable carry

: xor-key ( buf key len -- )
  ( len ) 0 do xor-buf-key+1 loop drop drop ;

: buf<<1 ( c-addr len )
  ( buf len )             0 swap \ initialise carry bit
  ( buf carry len )       0 swap 1- do \ loop from len to 0
  ( buf carry )           over i + tuck c@
  ( buf buf+i carry val ) tuck shl or ( buf  buf+i val val2 ) rot c!
  ( buf val )             $80 and if 1 else 0 then
  ( buf carry )           -1 +loop 
  drop drop ;

: cmac-calc-kn ( c-addr-fkey -- c-addr-fkey )
  ( fkey ) dup c@ $80 and 0<> \ if first bit of first byte is set
  ( fkey flag ) over 16 buf<<1
  ( fkey flag ) if dup 15 + dup c@ $87 xor swap c! then
  ( fkey ) ;

: cmac-xor-k1k2
  final.key
  ( fkey ) dup 16 0 fill
  ( fkey ) dup AESkey aes
  ( fkey ) cmac-calc-kn \ calc K1
  ( fkey ) padding @ if cmac-calc-kn then \ calc K2
  ( fkey ) AESaux swap 16 xor-key 
  ;

: cmac-calc ( buf len -- )
  padding over $0F and 0<> swap ! \ padding if len is not mulitple of 16.
  dup 1+ 0 do 
  ( buf+i len-i ) over AESaux i $0F and + tuck ( . . auxaddr buf+i auxaddr ) c@ swap c@ xor swap c! \ xor aux and buf
  ( buf+i len-i ) dup 0= if
  ( buf+i len-i )   AESaux i $0F and +  dup c@ $80 xor swap c! \ xor last byte with $80
  ( buf+i len-i )   cmac-xor-k1k2 \ perform this for last byte in buffer
  ( buf+i len-i )   AESaux AESkey aes
  ( buf+i len-i ) else 
  ( buf+i len-i )   i 1+ $0F and 0= if AESaux AESkey aes then \ mulitples of 16 bytes
  ( buf+i len-i ) then
  ( buf+i len-i ) 1- swap 1+ swap \ decrement len, increment pointer
  loop drop drop ;

: aes-cmac-noaux ( buf len )
  AESaux 16 0 fill
  cmac-calc ;

: aes-cmac-int ( buf-addr buf-len -- )  \ AES-CMAC hash key (mic) calculation
  AESaux AESkey aes
  cmac-calc ;

: aes-cmac ( buf len key iv -- mic )  \ AES-CMAC hash key calculation
                                      \ buf: c-addr of buffer to be encrypted
                                      \ len: encryption length
                                      \ key: c-addr of 128-bit encryption key
                                      \ iv : c-addr of 16-byte initialization vector
                                      \ mic: c-addr of message integrity check
  ( iv  ) AESaux 16 move
  ( key ) AESkey 16 move
  ( buf len ) aes-cmac-int
  AESaux ; \ put on stack as mic address
