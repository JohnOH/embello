// Demo for printf over the serial port, generating half a million primes.
// See http://jeelabs.org/2014/12/10/dips-into-the-lpc810/

#include <stdint.h>
#include <stdio.h>

#if __arm__
#include "serial.h"
#endif

#define MAX_PRIMES 400

uint16_t primes [MAX_PRIMES];

int main () {
#if __arm__
    LPC_SWM->PINASSIGN0 = 0xFFFF0004UL;
    serial.init(LPC_USART0, 115200);
#endif
    printf("Prime table:\n");

    uint32_t limit = 3, fill = 0, width = 0, count = 0;

    for (int value = 2; value < limit; ++value) {
        int i;
        for (i = 0; i < fill; ++i)
            if (value % primes[i] == 0) // check divisibility
                break;
        if (i < fill)
            continue; // found a factor, so it's not prime
        ++count;

        int chars = printf(" %d", value);

        width += chars; // wrap lines to under 80 chars
        if (width + chars >= 80) {
            width = 0;
            printf("\n");
        }

        if (fill >= MAX_PRIMES)
            continue; // no more room left in the primes table

        primes[fill] = value;
        if (primes[fill] != value)
            continue; // whoops, it got truncated
        ++fill;

        limit = value * value; // largest prime we can check for
    }

    printf("\nFound %d primes.\n", count);
    return 0;
}
