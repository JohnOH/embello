#include "embello.h"

Tick tick;
Serial serial;
Analog analog;

volatile unsigned Tick::millis;

extern "C" void SysTick_Handler () {
    ++Tick::millis;
}
