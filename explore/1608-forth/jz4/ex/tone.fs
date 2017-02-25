\ generate a tone using a delay loop

PA4 constant OUT
440 constant TONE  \ 440 Hz is an "A", 523.25 Hz is a "C"

: toggle
  begin
    OUT iox!
    1000000 TONE / 2/ us
  again ;

OMODE-PP OUT io-mode!

toggle
