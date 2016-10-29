\ these definitions should always stay loaded

cr $4000 eraseflashfrom  \ this must be loaded on top of a clean Mecrisp image!
compiletoflash

: cornerstone ( "name" -- )  \ define a flash memory cornerstone
  <builds begin here 127 and while $FFFF h, repeat
  does>   begin dup  127 and while 2+       repeat  cr
  eraseflashfrom ;

( flash use: ) here hex.
cornerstone eraseflash
