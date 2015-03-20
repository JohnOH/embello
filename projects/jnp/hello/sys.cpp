#include "sys.h"

Tick tick;
Serial serial;

volatile unsigned Tick::millis;

extern "C" void SysTick_Handler () {
  ++Tick::millis;
}
