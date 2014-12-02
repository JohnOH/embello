// Blink by periodically sending a message to the serial port.
// See http://jeelabs.org/2014/12/03/garage-parking-aid/

#include "stdio.h"
#include "serial.h"

// waste some time by doing nothing for a while
void delay (int count) {
    while (--count >= 0)
        __ASM(""); // twiddle thumbs
}

int main () {
    // send UART output to GPIO 4, running at 115200 baud
    LPC_SWM->PINASSIGN0 = 0xFFFFFF04UL;
    serial.init(LPC_USART0, 115200);

    printf("\n[gpa/1-blink]\n");

    // send out a greeting about twice a second
    while (true) {
        printf("Hello world!\n");
        delay(1000000);
    }
}
