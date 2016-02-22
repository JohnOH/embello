\ dump non-empty flash memory as Intel hex
\ adapted from mecrisp 2.0.2 (GPL3)

: u.4 ( u -- ) 0 <# # # # # #> type ;
: u.2 ( u -- ) 0 <# # # #> type ;

: hexdump ( -- ) \ dumps entire flash
  cr hex
  \ STM32F103x8: Complete: $FFFF $0000
  \ STM32F103xB: 128 KB would need a somewhat different hex file format
  $FFFF $0000  \ Complete image with Dictionary
  do
    \ Check if it would be $FFFF only:
    0                 \ Not worthy to print
    i #16 + i do      \ Scan data
      i c@ $FF <> or  \ Set flag if there is a non-$FF byte
    loop

    if
      ." :10" i u.4 ." 00"  \ Write record-intro with 4 digits.
      $10           \ Begin checksum
      i          +  \ Sum the address bytes
      i 8 rshift +  \ separately into the checksum

      i #16 + i do
        i c@ u.2  \ Print data with 2 digits
        i c@ +    \ Sum it up for Checksum
      loop

      negate u.2  \ Write Checksum
      cr
    then

  #16 +loop
  ." :00000001FF" cr
  decimal
;

\ adapted from mecrisp-stellaris 2.2.1a (GPL3)

: dump16 ( addr -- )  \ print 16 bytes memory
  base @ >r hex
  $F bic
  dup hex. 2 spaces

  dup 16 + over do
    i c@ u.2 space \ Print data with 2 digits
    i $F and 7 = if 2 spaces then
  loop

  2 spaces

  dup 16 + swap do
        i c@ 32 u>= i c@ 127 u< and if i c@ else [char] . then emit
        i $F and 7 = if space then
      loop

  cr
  r> base !
;

: dump ( addr len -- )  \ print a memory region
  cr
  over 15 and if 16 + then \ one more line if not aligned on 16
  begin
    swap ( len addr )
    dup dump16
    16 + ( len addr+16 )
    swap 16 - ( addr+16 len-16 )
    dup 0 <=
  until
  2drop
;
