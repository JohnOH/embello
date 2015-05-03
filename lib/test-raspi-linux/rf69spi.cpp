#include <stdio.h>
#include <stdint.h>
#include <errno.h>

#include <wiringPi.h>
#include <wiringPiSPI.h>

#define chThdYield() delay(1) // FIXME still used in rf69.h

#include "spi.h"
#include "rf69.h"

RF69<SpiDev0> rf;

uint8_t rxBuf[66];

int main () {
  wiringPiSetup();
  int myFd = wiringPiSPISetup (0, 4000000);

  if (myFd < 0) {
    printf("Can't open the SPI bus: %d\n", errno);
    return 1;
  }

    printf("\n[rf69try]\n");

    rf.init(1, 42, 8683);
    rf.encrypt("mysecret");
    rf.txPower(0); // 0 = min .. 31 = max

    uint16_t cnt = 0;
    uint8_t txBuf[66];
    for (int i = 0; i < sizeof txBuf; ++i)
      txBuf[i] = i;
    txBuf[0] = 45; // FIXME start slightly before the problem with > 48 bytes

    while (true) {
        if (++cnt == 0) {
            int txLen = ++txBuf[0] % 64;
            printf(" > #%d, %db\n", txBuf[0], txLen);
            rf.send(0, txBuf, txLen);
        }

        int len = rf.receive(rxBuf, sizeof rxBuf);
        if (len >= 0) {
            printf("OK ");
            for (int i = 0; i < len; ++i)
                printf("%02x", rxBuf[i]);
            printf(" (%d%s%d:%d)\n",
                    rf.rssi, rf.afc < 0 ? "" : "+", rf.afc, rf.lna);
        }

        chThdYield();
    }
}
