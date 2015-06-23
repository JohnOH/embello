// Increment a bit in flash page zero, just to verify that it works.

#include <string.h>

#include "sys.h"
#include "flash.h"

int main () {
    tick.init(1000);
    serial.init(115200);

    printf("\n[lowflash]\n");

    while (true) {
        tick.delay(500);
        printf("%u %u\n", tick.millis, *(const uint8_t*) 32);

        uint8_t buf [64];
        memcpy(buf, 0x0, sizeof buf);
        ++buf[32];

        Flash64::erase(0, 1);
        Flash64::save(0, buf);
    }
}
