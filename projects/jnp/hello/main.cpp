// Send some messages over the serial port.

#include "LPC8xx.h"
#include "uart.h"
#include <stdio.h>

class Tick {
public:
  static void init (int hz, bool fast =false) {
    SysTick_Config((fast ? 30000000 : 12000000) / hz);
  }

  static void delay (unsigned ms) {
    uint32_t start = millis;
    while (millis - start < ms)
      __WFI();
  }

  static volatile uint32_t millis;
};

volatile uint32_t Tick::millis;

extern "C" void SysTick_Handler () {
  ++Tick::millis;
}

class Serial {
public:
  static void init (int baud) {
    LPC_SWM->PINASSIGN0 = 0xFFFFFF04;
    uart0Init(baud);
  }
};

Tick tick;
Serial serial;

int main () {
  serial.init(115200);
  tick.init(1000);

  printf("\n[hello]\n");

  while (true) {
    tick.delay(1000);
    printf("%u\n", tick.millis);
  }
}
