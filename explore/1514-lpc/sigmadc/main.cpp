// Sigma-delta ADC based on NXP's AppNote 11329.

#include "sys.h"

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[sigmadc]\n");

  while (true) {
    tick.delay(500);
    printf("%u\n", (unsigned) tick.millis);
  }
}
