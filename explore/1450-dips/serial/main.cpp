// Demonstrate printf over the serial port.
// See http://jeelabs.org/2014/12/10/dip-into-the-lpc810/

#include <stdio.h>
#include "serial.h"

int main () {
    LPC_SWM->PINASSIGN0 = 0xFFFFFF04UL; // only connect TXD
    serial.init(LPC_USART0, 115200);

    printf("DEV_ID = %08x\n", LPC_SYSCON->DEVICE_ID);
    printf("GPREG0 = %08x\n", LPC_PMU->GPREG0);
    printf("GPREG1 = %08x\n", LPC_PMU->GPREG1);
    printf("GPREG2 = %08x\n", LPC_PMU->GPREG2);
    printf("GPREG3 = %08x\n", LPC_PMU->GPREG3);

    return 0;
}
