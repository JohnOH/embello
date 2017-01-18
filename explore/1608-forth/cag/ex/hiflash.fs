\ verify that all of flash from 64K to 128K can be erased and written

forgetram

: hi-wipe
  128 64 do i dup . 1024 * flashpageerase loop cr ;

: hi-fill ( u -- )
  1024 *  
  512 0 do
    i 2* over + i swap hflash!
  loop
  drop ;

: hi-fillall
  128 64 do i dup . hi-fill loop cr ;

: hi-test ( u -- )
  1024 *
  512 0 do
    i 2* over + h@
    i <> if
      i dup . 2* over + dup hex. h@ hex. ." FAIL!" quit
    then
  loop
  drop ;

: hi-testall
  128 64 do i dup . hi-test loop cr ;

( wipe: ) hi-wipe
( fill: ) hi-fillall
( test: ) hi-testall
( wipe: ) hi-wipe
