\ test long input lines, i.e. more than one 64-byte USB packet

forgetram

: a ." abcdefghijabcdefghijabcdefghijabcdefghijabcdefghij" ;
a
: b ." abcdefghijabcdefghijabcdefghijabcdefghij"
    ." abcdefghijabcdefghijabcdefghijabcdefghij" ;
b
: c ." abcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghij" ;
c
: d ." abcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghij" ;
d
: e ." abcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghij" ;
e
