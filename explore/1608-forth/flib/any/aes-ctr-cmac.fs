
\ 128-bit AES CTR and AES CMAC as used in LoraWAN
\ Author: SevenW from sevenwatt.com
\ Date  : 2017-Feb-23
\ 
\ Description:
\ AES128 CTR en- and de-cryption of a stream buffer
\ AES128 CMAC hash key calculation
\ For AES-CTR and AES-CMAC as used in LoraWAN, only AES-encryption is needed.
\ A testmodule is provided in the file aes-ctr-cmac-test.fs
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
\ 
\ 

16 buffer: AESkey
16 buffer: AESaux

\ 
\ AES-CTR
\ 

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

: aes-ctr ( buf len key iv )              \ AES-CTR encrypt buffer. Encryption is in-situ.
                                          \ buf: c-addr of buffer to be encrypted
                                          \ len: encryption length
                                          \ key: c-addr of 128-bit encryption key
                                          \ iv : c-addr of 16-byte initialization vector
  ( iv  ) AESaux 16 move
  ( key ) AESkey 16 move
  ( buf len ) aes-ctr-int ;


\ 
\ AES-CMAC
\ 

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

: aes-cmac ( buf len key iv -- mic )      \ AES-CMAC hash key calculation
                                          \ buf: c-addr of buffer to be encrypted
                                          \ len: encryption length
                                          \ key: c-addr of 128-bit encryption key
                                          \ iv : c-addr of 16-byte initialization vector
                                          \ mic: c-addr of message integrity check
  ( iv  ) AESaux 16 move
  ( key ) AESkey 16 move
  ( buf len ) aes-cmac-int
  AESaux ; \ put on stack as mic address
