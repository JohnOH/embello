\ these definitions should always stay loaded

$5000 eraseflashfrom  \ need to start off with a clean Mecrisp image
compiletoflash

: chipid ( -- u1 u2 u3 3 )  \ unique chip ID as N values on the stack
  $1FFFF7E8 @ $1FFFF7EC @ $1FFFF7F0 @ 3 ;
: hwid ( -- u )  \ a "fairly unique" hardware ID as single 32-bit int
  chipid 1 do xor loop ;
: flash-kb ( -- u )  \ return size of flash memory in KB
  $1FFFF7E0 h@ ;
: flash-pagesize ( addr - u )  \ return size of flash page at given address
  drop flash-kb 128 <= if 1024 else 2048 then ;

: cornerstone ( "name" -- )  \ define a flash memory cornerstone
  <builds begin here dup flash-pagesize 1- and while 0 h, repeat
  does>   begin dup  dup flash-pagesize 1- and while 2+   repeat  cr
  eraseflashfrom ;

( always end: ) here hex.
cornerstone eraseflash
compiletoram
