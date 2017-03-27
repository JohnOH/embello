\ try SD card access

compiletoram? [if]  forgetram  [then]

( blocks: ) sd-init sd-size .
sd.buf 16 dump

0 sd-read
0 sd-read
0 sd-read
sd.buf 512 dump
$1c6 2/ sd-read
sd.buf 128 dump
$574 2/ sd-read
sd.buf 128 dump
