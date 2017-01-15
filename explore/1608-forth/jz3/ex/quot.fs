\ Quotations by MK, like in Punyforth

forgetram

: { ( -- )
  $3F04 h,        \ subs r7, #4
  $603E h,        \ str r6, [ r7 #0 ]
  $467E h,        \ mov  r6, pc
  postpone ahead  \ b.n ...
  $B500 h,        \ push {lr}
immediate ;

: } ( -- )
  postpone exit
  postpone then
immediate ;

: qdemo 3 { 1+ 5 * } execute . ;

qdemo
