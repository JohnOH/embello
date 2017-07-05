\ these definitions should always stay loaded

$5000 eraseflashfrom  \ need to start off with a clean Mecrisp image
compiletoflash

: cornerstone ( "name" -- )  \ define a flash memory cornerstone
  <builds   begin here 127 and while $FFFF h, repeat
  does> cr  begin dup  127 and while 2+       repeat  cr eraseflashfrom ;

( flash use: ) here hex.
cornerstone eraseflash
compiletoram
