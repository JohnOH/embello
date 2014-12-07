// Motion experiment.
// See http://jeelabs.org/2014/12/10/dips-into-the-lpc810/

#include "LPC8xx.h"

extern "C" void SysTick_Handler () {
    // the only effect is to generate an interrupt, no work is done here
}

static void delay (uint32_t ms) {
    while (ms-- > 0)
        __WFI();
}

int main () {
    SysTick_Config(12000000/1000);          // 1000 Hz

    LPC_SWM->PINENABLE0 |= 3<<2;            // disable SWCLK and SWDIO
    LPC_SWM->PINASSIGN6 = 0x04ffffFFUL;     // connect CTOUT_0 to PIO0_4

    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<8);    // enable the SCT

    LPC_SCT->CONFIG = 0x1;                  // UNIFY

    // toggle the output on half the system clock for a 1 Hz blink rate
    LPC_SCT->MATCH[0].U = LPC_SCT->MATCHREL[0].U = 12000000 / 2;

    LPC_SCT->OUT[0].SET = 0x2;              // set on event 1
    LPC_SCT->OUT[0].CLR = 0x1;              // clear on event 0
    LPC_SCT->OUTPUT |= 0x1;                 // enable output 0

    LPC_SCT->EVENT[0].CTRL = 0x0000D000;
    LPC_SCT->EVENT[0].STATE = 0x1;
    LPC_SCT->EVENT[1].CTRL = 0x00005000;
    LPC_SCT->EVENT[1].STATE = 0x2;

    LPC_SCT->LIMIT_L = 0x3;                 // events 0 and 1 clear the counter

    LPC_SCT->CTRL_L &= ~(1<<2);             // start the SCT

    while (true)
        __WFI();
}
