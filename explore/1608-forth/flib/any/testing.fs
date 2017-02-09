\ Simple support for unit tests.

-1 variable tests-OK
: fail-tests 0 tests-OK ! ;
: test-summary 
  tests-OK @ not if ." ** TESTS FAILED! **" else
  depth 0<> if ." ** TESTS OK but stack not empty: " .v else
  ." ** ALL OK **" then then cr ;

: =always ( n1 n2 -- ) \ assert that the two TOS values must be equal
  2dup <> if
    ." FAIL: got " swap . ." expected " . fail-tests
  else 2drop then ;

: =always-fix ( df1 df2 -- ) \ assert that the two TOS fixed-point values must be equal
  2dup 2rot 2dup 2rot ( df2 df1 df1 df2 )
  d<> if
    ." FAIL: got " f. ." expected " f. fail-tests
  else 2drop 2drop then ;

: always ( f -- )
  0= if
    ." FAIL!" fail-tests
  else ." OK!" then ;
