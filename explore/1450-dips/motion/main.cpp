// Motion experiment.
// See http://jeelabs.org/2014/12/10/dips-into-the-lpc810/

#include "LPC8xx.h"

#define SCT_MHZ 12  // SCT clock ticks per microsecond

extern "C" void SysTick_Handler () {
    // the only effect is to generate an interrupt, no work is done here
}

static void delay (uint32_t ms) {
    while (ms-- > 0)
        __WFI();
}

int main () {
    LPC_SWM->PINENABLE0 |= 3<<2;            // disable SWCLK and SWDIO

    //LPC_SWM->PINASSIGN6 = 0x02FFFFFF;       // connect CTOUT_0 to PIO0_2
    //LPC_SWM->PINASSIGN7 = 0xFF050403;       // cto1 -> 3, cto2 -> 4, cto3 -> 5
    LPC_SWM->PINASSIGN7 = 0xFFFF04FF;       // connect CTOUT_2 to PIO0_4

    SysTick_Config(12000000/1);

    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<8);    // enable the SCT
    LPC_SCT->CONFIG = (1<<0);               // unify

    LPC_SCT->MATCH[0].U = LPC_SCT->MATCHREL[0].U = 20000 * SCT_MHZ; // 20 ms
    LPC_SCT->MATCH[1].U = LPC_SCT->MATCHREL[1].U = 1500 * SCT_MHZ;  // 1.5 ms
    LPC_SCT->MATCH[2].U = LPC_SCT->MATCHREL[2].U = 1500 * SCT_MHZ;  // 1.5 ms
    LPC_SCT->MATCH[3].U = LPC_SCT->MATCHREL[3].U = 1500 * SCT_MHZ;  // 1.5 ms
    LPC_SCT->MATCH[4].U = LPC_SCT->MATCHREL[4].U = 1500 * SCT_MHZ;  // 1.5 ms

    LPC_SCT->EVENT[0].CTRL = (0<<0) | (1<<12);
    LPC_SCT->EVENT[1].CTRL = (1<<0) | (1<<12);
    LPC_SCT->EVENT[2].CTRL = (2<<0) | (1<<12);
    LPC_SCT->EVENT[3].CTRL = (3<<0) | (1<<12);
    LPC_SCT->EVENT[4].CTRL = (4<<0) | (1<<12);

    LPC_SCT->EVENT[0].STATE = (1<<0);
    LPC_SCT->EVENT[1].STATE = (1<<0);
    LPC_SCT->EVENT[2].STATE = (1<<0);
    LPC_SCT->EVENT[3].STATE = (1<<0);
    LPC_SCT->EVENT[4].STATE = (1<<0);

    LPC_SCT->OUT[0].SET = (1<<0);
    LPC_SCT->OUT[1].SET = (1<<0);
    LPC_SCT->OUT[2].SET = (1<<0);
    LPC_SCT->OUT[3].SET = (1<<0);

    LPC_SCT->OUT[0].CLR = (1<<1);
    LPC_SCT->OUT[1].CLR = (1<<2);
    LPC_SCT->OUT[2].CLR = (1<<3);
    LPC_SCT->OUT[3].CLR = (1<<4);

    LPC_SCT->LIMIT_L = (1<<0);              // event 0 clears the counter
    LPC_SCT->CTRL_L &= ~(1<<2);             // start the SCT

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
