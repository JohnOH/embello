\ see http://jeelabs.org/article/1707a/

: rxtest ( -- )
  rf-init
  begin
    rf-recv ?dup if
      cr  rf.buf 2+  swap 2-  type
    then
  again ;
