\ frozen application, this will run an RF packet echo loop on reset

\ eraseflash
include board.fs
include core.fs

compiletoflash
include dev.fs

: init init unattended echo ;
