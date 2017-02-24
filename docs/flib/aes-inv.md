# AES-128 Decryption

[code]: any/aes128inv.fs (aes)
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/any/aes128inv.fs">any/aes128inv.fs</a>
* Needs: aes

This package implements AES-128 decryption of 16-byte blocks.

## API

[defs]: <> (aes-inv)
```
: aes-inv ( c-addr key -- )  \ aes128 decrypt block
```

## Examples

Decrypt a 16-byte buffer with a 16-byte key

    16 buffer: mybuf
    16 buffer: mykey
    \ set contents of mybuf and mykey here ...
    mybuf mykey aes-inv
    mybuf 16 dump
