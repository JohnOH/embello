# Variable-sized integer encoding

[code]: any/varint.fs (rf69)
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/any/varint.fs">any/varint.fs</a>
* Needs: rf69

This package allows encoding and decoding 32-bit integers in a variable number of
bytes. A pre-cursor of this design was described on the [JeeLabs
Weblog](http://jeelabs.org/article/1620c/). This encoding has now been extended
to support signed values.

The idea is to send as many bytes as needed,
each with 7 bits of the value and the high bit set only in the last byte.
Also, the sign bit is rotated to bit 0, i.e. the least signficant bit, with
the absolute value in the remaining 31 bits.

| Range | Bytes |
| :---: | --- |
| -64 .. +63 | 1 |
| -8192 .. +8191 | 2 |
| -1,048,576 .. +1,048,575 | 3 |
| -134,217.728 .. +134,217,727 | 4 |
| -2,147,483,648 .. +2,147,483,647 | 5 |

A varint will never start with a zero byte, but zeros can still occur _inside_
a varint. The zero byte is reserved for future encoding of other data types.

## API

[defs]: <> (<pkt +pkt pkt>rf)
```
: <pkt ( format -- )  \ start collecting values
: +pkt ( n -- )  \ append 32-bit signed value to packet
: pkt>rf ( -- )  \ broadcast the collected values as RF packet
```

[defs]: <> (var-init var> var.)
```
: var-init ( addr cnt -- )  \ initialise the varint decoder
: var> ( -- 0 | n 1 ) \ extract a signed number from the var buffer
: var. ( addr cnt -- )  \ decode and display all the varints
```

## Examples

Receive and decode a packet with varints (skips unrelated header bytes):

    rf-recv ?dup if
      rf.buf 2+  swap 2-  var.
    then
