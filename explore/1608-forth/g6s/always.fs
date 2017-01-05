\ these definitions should always stay loaded

$5000 eraseflashfrom  \ need to start off with a clean Mecrisp image
compiletoflash

: cornerstone ( "name" -- )  \ define a flash memory cornerstone
  \ assume 2048-byte pages, in case this is an F103 w/ 256K flash or more
  <builds   begin here 2047 and while 0 h, repeat
  does> cr  begin dup  2047 and while 2+   repeat  cr eraseflashfrom ;

( always end: ) here hex.
cornerstone eraseflash
compiletoram
