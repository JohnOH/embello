// Send some messages over the serial port.

#include "LPC8xx.h"
#include "uart.h"
#include <stdio.h>

class LPC8 {
public:
  static volatile uint32_t millis;

  static void initSerial (int baud) {
    LPC_SWM->PINASSIGN0 = 0xFFFFFF04;
    uart0Init(baud);
  }

  static void initSysTick (int hz) {
    SysTick_Config(12000000/hz);
  }

  static void delay (unsigned ms) {
    uint32_t start = millis;
    while (millis - start < ms)
      __WFI();
  }
};

volatile uint32_t LPC8::millis;

LPC8 me;

extern "C" void SysTick_Handler () {
  ++me.millis;
}

static void setup () {
  me.initSerial(115200);
  me.initSysTick(1000);
}

int main () {
  setup();
  printf("\n[hello]\n");

  while (true) {
    me.delay(1000);
    printf("%u\n", me.millis);
  }
}
