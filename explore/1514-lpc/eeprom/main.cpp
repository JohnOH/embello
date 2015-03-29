// Try out eeprom emulation using upper flash memory.

#include "sys.h"
#include <string.h>

#include "flash.h"
#include "romvars.h"

RomVars<Flash,16> rom;

int main () {
  serial.init(115200);
  printf("\n[eeprom]\n");

  tick.init(1000);
  rom.init();

  unsigned i = 0;
  while (true) {
    int varNum = i++ % 7;
    uint16_t newVal = tick.millis;

    uint16_t oldVal = rom[varNum];

    int start = tick.millis;
    rom.set(varNum, newVal);
    int elapsed = tick.millis - start;

    printf("#%d: old %-6u new %-6u %d ms\n", varNum, oldVal, newVal, elapsed);
    tick.delay(500);
  }
}

#include "../../../lib/vendor/lpcopen/src/iap.c"
