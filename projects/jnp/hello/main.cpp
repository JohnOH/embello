// Send some messages over the serial port.

#include "sys.h"

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[hello]\n");

  while (true) {
    tick.delay(500);
    printf("%u\n", tick.millis);
  }
}
