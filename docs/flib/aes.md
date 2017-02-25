# AES-128 Encryption

[code]: any/aes128.fs ()
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/any/aes128.fs">any/aes128.fs</a>

This package implements AES-128 encryption and decryption of 16-byte blocks. It
supports encryption and decryption of arbitrary length (payload) buffers by
AES-CTR and calculation of a 'message authentication code'.

The decryption code is located in a separate `aes128inv.fs` file.

## API

[defs]: <> (aes)
```
: aes ( c-addr key -- )  \ aes128 encrypt block
```

[defs]: <> (aes-ctr aes-cmac)
```
: aes-ctr ( buf len key iv )  \ AES-CTR encrypt buffer, encryption is in-situ
: aes-cmac ( buf len key iv -- mic )  \ AES-CMAC hash key calculation
```

## Examples

Encrypt a 16-byte buffer with a 16-byte key

    12 34 56 78 4 nvariable mybuf
    11 22 33 44 4 nvariable mykey
    mybuf mykey aes
    : t 4 0 do mybuf i cells + @ hex. loop ; t
    \ expected output: B2F79167 C5E63682 E72CAE5E 675105CA

Block cipher counter mode

    buf-n len-n key initvector aes-ctr

Cipher-based Message Authentication Code

    buf-n len-n key initvector aes-cmac
