// Generate sine waves, with the SCT generating PWM pulses through an RC filter.

#include "embello.h"
#include "sine.h"

static void pwmSetup () {
  LPC_SYSCON->SYSAHBCLKCTRL |= (1<<8);    // enable the SCT
  LPC_SCT->CONFIG = (1<<0);               // unify

  // counter runs up to 1024 at 12 MHz
  LPC_SCT->MATCH[0].U = LPC_SCT->MATCHREL[0].U = 1024;
  for (int i = 1; i < 5; ++i)
    LPC_SCT->MATCH[i].U = LPC_SCT->MATCHREL[i].U = 0;

  for (int i = 0; i < 5; ++i) {
    LPC_SCT->EV[i].CTRL = (i << 0) | (1<<12);
    LPC_SCT->EV[i].STATE = (1<<0);
  }

  for (int i = 0; i < 4; ++i) {
    LPC_SCT->OUT[i].SET = (1<<0);
    LPC_SCT->OUT[i].CLR = 1 << (i+1);
  }

  LPC_SCT->LIMIT_L = (1<<0);              // event 0 clears the counter
  LPC_SCT->CTRL_L &= ~(1<<2);             // start the SCT
}

int main () {
  LPC_SWM->PINENABLE0 |= 3<<2;            // disable SWCLK and SWDIO
  LPC_SWM->PINASSIGN[8] = 0xFFFF03FF;     // connect CTOUT_2 to PIO0_3

  pwmSetup();

  tick.init(50*1024); // run at 51,200 Hz to generate a 50 Hz sine

  int phase = 0;
  while (true) {
    uint8_t step = ++phase;
    if (phase & (1<<8))
        step = ~step;
    int ampl = sineTable[step];
    if (phase & (1<<9))
        ampl = - ampl;

    __WFI(); // sync up with the systick timer
    LPC_SCT->MATCHREL[3].U = 512 + (ampl >> 6);
  }
}

