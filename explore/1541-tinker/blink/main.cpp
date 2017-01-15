// Blink an LED connected to GPIO 13 (pin 3 on the Tinker Pico).
// See http://jeelabs.org/book/1541c/

#include "embello.h"

Pin<13> led;

int main () {
  tick.init(1000);
  led.setOutput();

  while (true) {
    led.toggle();
    tick.delay(500);
  }
}
