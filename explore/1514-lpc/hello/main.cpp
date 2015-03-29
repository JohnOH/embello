// Try out eeprom emulation using upper flash memory.

#include "sys.h"

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[hello]\n");

  while (true) {
    tick.delay(500);
    printf("%u\n", (unsigned) tick.millis);
  }
}

// vim: ts=2 sts=2 sw=2
