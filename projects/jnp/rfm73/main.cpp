// Send some messages over the serial port.

#include "sys.h"
#include "rf73.h"

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[rfm73]\n");

  while (true) {
    tick.delay(500);
    printf("%u\n", tick.millis);
  }
}
