\ access to FAT-formatted SD cards

compiletoram? [if]  forgetram  [then]

: tryfat
  sd-init ." blocks: " sd-size .
  cr sd-mount ls                 \ mount and show all root entries
  s" D       IMG" drop fat-find  \ locate the "D.IMG" file by name
  0 file fat-chain               \ build map for DISK3.IMG
  0 file 30 dump                 \ show map
  0 0 file fat-map sd-read       \ load first block
  sd.buf dup hex. 50 dump        \ show contents
;

tryfat
