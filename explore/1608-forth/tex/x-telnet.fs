\ process known telnet escapes and ignore the rest
\ assumes usb I/O is active

0 variable tn.state

: telnet-next ( c -- c | 0 )
  tn.state @ case
    0 of  \ default state
      dup 255 = if 1 tn.state ! drop 0 then
    endof
    1 of  \ IAC seen
      dup 255 <> if  \ not doubled, it's a command
        dup 251 >= if drop 2 else 250 = if 3 else 0 then then  tn.state !  0
      then
    endof
    2 of  \ IAC WILL/WONT/DO/DONT seen
      0 tn.state !  drop 0
    endof
    3 of  \ IAC SB seen
      240 = if 0 tn.state ! then  0  \ skip everything until SE seen
    endof
  endcase ;

: telnet-key ( -- c )  \ key input with telnet processing
  begin usb-key telnet-next ?dup until ;

: telnet-emit ( c -- )  \ char output with telnet escapes
  dup 255 = if dup usb-emit then usb-emit ;

: telnet-io ( -- )  \ change hooks to use the telnet protocol
  ['] telnet-key hook-key !
  ['] telnet-emit hook-emit ! ;
