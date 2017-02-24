# AES-128 Encryption / Decryption / AES-CTR / AES-CMAC

[code]: any/aes128.fs
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/any/aes128.fs">any/aes128.fs</a>
* Needs: -
[code]: any/aes128.fs (aes128)
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/any/aes128inv.fs">any/aes128inv.fs</a>
* Needs: aes128
[code]: any/aes-ctr-cmac.fs (aes128)
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/any/aes-ctr-cmac.fs">any/aes-ctr-cmac.fs</a>
* Needs: aes128

This package allows AES-128 encryption and decryption of 16-byte blocks.
It allows encryption and decryption of arbitrary length (payload) buffers 
by AES-CTR and it allows for calculation of a 'message authentication code'.
The latter two functions are used in for example LoraWAN transmissions
using the LMIC protocol.


## API

[defs]: <> (+aes)

[defs]: <> (-aes)

[defs]: <> (aes-ctr aes-cmac)

## Examples

Encrypt
	buf16 key +aes
Decrypt
	buf16 key -aes
CTR
	buf-n len-n key initvector aes-ctr
CMAC
	buf-n len-n key initvector aes-cmac
