// Demo of a C++ template wrapper for GPIO pins.

#include "embello.h"

#if 0 // see embello.h

template < int N >
class Pin {
public:
  operator int ()             { return LPC_GPIO_PORT->B[0][N]; }
  void operator= (int value)  { LPC_GPIO_PORT->B[0][N] = value != 0; }
  void setInput()             { LPC_GPIO_PORT->DIR[0] &= ~(1<<N); }
  void setOutput()            { LPC_GPIO_PORT->DIR[0] |= 1<<N; }
  void toggle()               { LPC_GPIO_PORT->NOT[0] = 1<<N; }
};

#endif

Pin<13> led;

int main () {
  tick.init(1000);

  led.setOutput();
  led = 1;
  tick.delay(1000);
  led = 0;
  tick.delay(2000);

  while (true) {
    led.toggle();
    tick.delay(500);
  }
}
