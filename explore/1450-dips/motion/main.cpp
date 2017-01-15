// Generate up to 4 independent servo motion control pulses in hardware.
// See http://jeelabs.org/2014/12/10/dip-into-the-lpc810/

#include "LPC8xx.h"

#define SCT_MHZ 12  // SCT clock ticks per microsecond

extern "C" void SysTick_Handler () {
    // the only effect is to generate an interrupt, no work is done here
}

static void pwmSetup () {
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<8);    // enable the SCT
    LPC_SCT->CONFIG = (1<<0);               // unify

    LPC_SCT->MATCH[0].U = LPC_SCT->MATCHREL[0].U = 20000 * SCT_MHZ;
    for (int i = 1; i < 5; ++i)
        LPC_SCT->MATCH[i].U = LPC_SCT->MATCHREL[i].U = 1500 * SCT_MHZ;

    for (int i = 0; i < 5; ++i) {
        LPC_SCT->EVENT[i].CTRL = (i << 0) | (1<<12);
        LPC_SCT->EVENT[i].STATE = (1<<0);
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

    //LPC_SWM->PINASSIGN6 = 0x02FFFFFF;       // connect CTOUT_0 to PIO0_2
    //LPC_SWM->PINASSIGN7 = 0xFF000403;       // cto1 -> 3, cto2 -> 4, cto3 -> 0
    LPC_SWM->PINASSIGN7 = 0xFFFF01FF;       // connect CTOUT_2 to PIO0_1, pin 5

    pwmSetup();

    SysTick_Config(12000000);

    // change pulse widths on pin 2 once a second
    while (true) {
        __WFI();
        LPC_SCT->MATCHREL[3].U = 1000 * SCT_MHZ;
        __WFI();
        LPC_SCT->MATCHREL[3].U = 1500 * SCT_MHZ;
        __WFI();
        LPC_SCT->MATCHREL[3].U = 2000 * SCT_MHZ;
        __WFI();
        LPC_SCT->MATCHREL[3].U = 1500 * SCT_MHZ;
    }
}
