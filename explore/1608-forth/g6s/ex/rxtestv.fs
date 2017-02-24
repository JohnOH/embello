\ see http://jeelabs.org/article/1707b/

: rxtestv ( -- )
  rf-init
  begin
    rf-recv ?dup if
      cr  rf.buf 2+  swap 2-  var.
    then
  again ;
