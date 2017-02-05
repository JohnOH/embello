\ core definitions

<<<board>>>
compiletoflash
( core start: ) here dup hex.

include ../flib/mecrisp/multi.fs
include ../flib/mecrisp/disassembler-m3.fs

PA12 constant LED1
PA11 constant LED2
PA10 constant LED3  \ RX
PA9  constant LED4  \ TX
PA8  constant LED5
PB2  constant LED6

PB5  constant KEY1
PB4  constant KEY2
PB3  constant KEY3
PA15 constant KEY4

: io-init
  OMODE-PP LED1 io-mode!
  OMODE-PP LED2 io-mode!
\ OMODE-PP LED3 io-mode!
\ OMODE-PP LED4 io-mode!
  OMODE-PP LED5 io-mode!
  OMODE-PP LED6 io-mode!

  IMODE-PULL KEY1 io-mode!  KEY1 ios!
  IMODE-PULL KEY2 io-mode!  KEY2 ios!
  IMODE-PULL KEY3 io-mode!  KEY3 ios!
  IMODE-PULL KEY4 io-mode!  KEY4 ios!
;

: io-test
  begin
    KEY1 io@ not LED1 io!
    KEY2 io@ not LED2 io!
    KEY3 io@ not LED5 io!
    KEY4 io@ not LED6 io!
  key? until ;

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
