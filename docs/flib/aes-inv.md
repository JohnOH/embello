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

    $675105CA $E72CAE5E $C5E63682 $B2F79167 4 nvariable mybuf
    11 22 33 44 4 nvariable mykey
    mybuf mykey aes-inv
    : t 4 0 do mybuf i cells + @ . loop ; t
    \ expected output: 78 56 34 12
