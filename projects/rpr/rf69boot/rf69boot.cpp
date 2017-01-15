// Test JeeBoot mechanism with a RasPi RF board.

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <errno.h>

#include "util.h"
#include "fileaccess.h"
#include "bootserver.h"

#include <wiringPi.h>
#include <wiringPiSPI.h>

#define DEBUG   1             // prints all incoming packets to stdout if set

// fixed configuration settings for now
#define RF_FREQ   8686
#define RF_GROUP  42
#define RF_ID     62

#include "spi.h"
#include "rf69.h"

RF69<SpiDev0> rf;

const char* filePath;

class MyFileAccess : public FileAccess {
public:
    MyFileAccess () : FileAccess (filePath) {}
};

int main (int argc, const char** argv) {
    if (argc < 2) {
        fprintf(stderr, "Usage: rf69boot <filepathprefix>\n");
        return 1;
    }

    filePath = argv[1];
    printf("\n[rf69boot] using: %s*\n", filePath);

    wiringPiSetup();
    if (wiringPiSPISetup (0, 4000000) < 0) {
        printf("Can't open the SPI bus: %d\n", errno);
        return 1;
    }

    rf.init(RF_ID, RF_GROUP, RF_FREQ);
    //rf.encrypt("mysecret");
    rf.txPower(15); // 0 = min .. 31 = max

    BootServer<MyFileAccess> server;

    while (true) {
        uint8_t buf [64];
        int len = rf.receive(buf, sizeof buf);
        if (len >= 0) {
#if DEBUG
            printf("OK ");
            for (int i = 0; i < len; ++i)
                printf("%02x", buf[i]);
            printf(" (%d%s%d:%d)\n",
                    rf.rssi, rf.afc < 0 ? "" : "+", rf.afc, rf.lna);
#endif

            if (buf[1] == 0xC0) {
                BootReply reply;
                int len2 = server.request(buf + 2, len - 2, &reply);
                if (len2 <= 0) {
                    printf("ignoring %d bytes\n", len - 2);
                } else {
                    printf("sending %d bytes\n", len2);
                    rf.send(0xC0, (const uint8_t*) &reply + 2, len2);
                }
            }
        }

        chThdYield();
    }
}
