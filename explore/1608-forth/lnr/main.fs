\ application setup and main loop

: main
  begin
    led iox!
    500 ms
  key? until ;
