# JeeBoot

An implementation of over-the-air software updates for JeeNodes:

* the bulk of the code lives in `.h` files in `include/`
* tests are located in the `.cpp` files in `tests/`
* there's a dummy source file in `src/` to keep the `ar` command happy

This structure is needed to support [CppUTest][1] without changes.

To run the  tests, set the `CPPUTEST_HOME` environment variable to point to an
installed copy of CppUTest (version 3.7.1 is known to work), then type:

    cd tests
    make

Sample output for a succesful test run:

    Running jeeboot_tests
    ..........
    OK (10 tests, 10 ran, 24 checks, 0 ignored, 0 filtered out, 0 ms)

*This code is work-in-progress. Nothing but tests so far.*

[1]: http://cpputest.github.io
