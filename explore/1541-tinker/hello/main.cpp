// Periodically send some text through the serial port.
// See http://jeelabs.org/book/1541c/

#include "embello.h"

int main () {
  tick.init(1000);
  serial.init(115200);
  printf("[hello]\n");

  while (true) {
    tick.delay(500);
    printf("%u\n", tick.millis);
  }
}
