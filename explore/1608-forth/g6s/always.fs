\ these definitions should always stay loaded

$5000 eraseflashfrom  \ need to start off with a clean Mecrisp image
compiletoflash

: cornerstone ( "name" -- )  \ define a flash memory cornerstone
  <builds begin here dup flash-pagesize 1- and while 0 h, repeat
  does>   begin dup  dup flash-pagesize 1- and while 2+   repeat  cr
  eraseflashfrom ;

( always end: ) here hex.
cornerstone eraseflash
compiletoram
