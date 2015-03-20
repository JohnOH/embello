#include "chip.h"
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

  static volatile unsigned millis;
};

class Serial {
public:
  static void init (int baud) {
    LPC_SWM->PINASSIGN[0] = 0xFFFFFF04;
    uart0Init(baud);
  }
};

extern Tick tick;
extern Serial serial;
