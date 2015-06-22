// Test JeeBoot mechanism with a RasPi RF board.

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>

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

int main () {
  printf("\n[rf69boot]\n");

  wiringPiSetup();
  if (wiringPiSPISetup (0, 4000000) < 0) {
    printf("Can't open the SPI bus: %d\n", errno);
    return 1;
  }

  rf.init(RF_ID, RF_GROUP, RF_FREQ);

  //rf.encrypt("mysecret");
  rf.txPower(15); // 0 = min .. 31 = max

  struct {
    uint8_t buf [64];
  } rx;

  while (true) {
    int len = rf.receive(rx.buf, sizeof rx.buf);
    if (len >= 0) {
#if DEBUG
      printf("OK ");
      for (int i = 0; i < len; ++i)
        printf("%02x", rx.buf[i]);
      printf(" (%d%s%d:%d)\n", rf.rssi, rf.afc < 0 ? "" : "+", rf.afc, rf.lna);
#endif

      rx.afc = rf.afc;
      rx.rssi = rf.rssi;
      rx.lna = rf.lna;
    }

    chThdYield();
  }
}
