// Try out eeprom emulation using upper flash memory.
// see http://jeelabs.org/2015/04/01/emulating-eeprom/

#include "embello.h"
#include <string.h>

#include "flash.h"
#include "romvars.h"

RomVars<Flash64,0x0F80> rom;

int main () {
  serial.init(115200);
  printf("\n[eeprom]\n");

  tick.init(1000);
  rom.init();

  unsigned i = 0;
  while (true) {
    int varNum = i++ % 5 + 1;

    if (varNum == 1) {
      uint16_t v10 = rom[10];
      printf("bump #10 to %u\n", ++v10);
      rom[10] = v10;
    }

    uint16_t oldVal = rom[varNum];
    uint16_t newVal = tick.millis;

    int start = tick.millis;
    rom[varNum] = newVal;
    int elapsed = tick.millis - start;

    printf("#%d: old %-6u new %-6u %d ms\n", varNum, oldVal, newVal, elapsed);

    tick.delay(500);
  }
}
