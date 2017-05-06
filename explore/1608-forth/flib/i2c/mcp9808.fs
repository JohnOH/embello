\ driver for a MCP9808 temperature sensor
\ needs i2c

\ fetch big-endian half-word
: i2c>h.be ( -- u )  i2c> 8 lshift i2c> or ;

: mcp9808-addr $18 i2c-addr ;                 \ set the device i2c address
: mcp9808-reg ( n -- ) mcp9808-addr >i2c ;    \ select register n

: mcp9808-init ( -- nak ) \ put device into continuous mode, 0.125C precision
  i2c-init 1 mcp9808-reg
  0 >i2c 0 >i2c \ reset values (continuous mode)
  0 i2c-xfer drop
  8 mcp9808-reg \ resolution reg
  2 >i2c        \ 2=0.125C, 3=0.0625C
  0 i2c-xfer
  ;

: mcp9808-data ( -- v ) \ read data, return as 1/100th centigrade
  5 mcp9808-reg
  2 i2c-xfer drop
  i2c>h.be 19 lshift \ move sign bit to top
  19 arshift 100 * 8 + 4 arshift
  ;

: mcp9808-sleep ( -- ) \ put device to sleep
  1 mcp9808-reg
  1 >i2c 0 >i2c \ control: shutdown mode
  0 i2c-xfer drop ;

: mcp9808-convert ( -- ms ) \ put device into continuous mode, returns ms before first cvt
  1 mcp9808-reg
  0 >i2c 0 >i2c \ control: cont mode
  0 i2c-xfer drop
  130 ;

\ mcp9808-init .
\ mcp9808-data .
