// Report supply voltage, estimated via ACMP and bandgap

#include "embello.h"

// setup the analog(ue) comparator, using the ladder on + and bandgap on -
void acmpVccSetup () {
  LPC_SYSCON->PDRUNCFG &= ~(1<<15);             // power up comparator
  LPC_SYSCON->SYSAHBCLKCTRL |= (1<<19);         // ACMP & IOCON clocks
  //LPC_SYSCON->PRESETCTRL &= ~(1<<12);           // reset comparator
  //LPC_SYSCON->PRESETCTRL |= (1<<12);            // release comparator

  // connect ladder to CMP+ and bandgap to CMP-
  // careful: 6 on LPC81x, 5 on LPC82x !
  if (LPC_SYSCON->DEVICEID < 0x8200)
    LPC_CMP->CTRL = (6<<11); 
  else
    LPC_CMP->CTRL = (5<<11); 
}

// estimate the bandgap voltage in terms of Vcc ladder steps, returns as mV
int acmpVccEstimate () {
  int i;
  for (i = 2; i < 32; ++i) {
    LPC_CMP->LAD = (i << 1) | 1;                // use ladder tap i
    for (int n = 0; n < 100; ++n) __ASM("");    // brief settling delay
    if (LPC_CMP->CTRL & (1<<21))                // if COMPSTAT bit is set
      break;                                    // ... we're done
  }
  // the result is the number of Vcc/31 ladder taps, i.e.
  //    tap * (Vcc / 31) = 0.9
  // therefore:
  //    Vcc = (0.9 * 31) / tap
  // or, in millivolt:
  int tap = i - 1;
  return (900 * 31) / tap;
}

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[vcc]\n");

  acmpVccSetup();

  while (true) {
    int vcc = acmpVccEstimate();
    printf("%d mV\n", vcc);
    tick.delay(1000);
  }
}
