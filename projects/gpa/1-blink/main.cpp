// Blink by periodically sending a message to the serial port.
// See http://jeelabs.org/2014/12/03/garage-parking-aid/

#include "stdio.h"
#include "serial.h"

extern "C" void SysTick_Handler () {                                             
    // the only effect is to generate an interrupt, no work is done here         
}

void delay (int millis) {
    while (--millis >= 0)
        __WFI(); // wait for the next SysTick interrupt
}

int main () {
    // send UART output to GPIO 4, running at 115200 baud
    LPC_SWM->PINASSIGN0 = 0xFFFFFF04UL;
    serial.init(LPC_USART0, 115200);

    printf("\n[gpa/1-blink]\n");

    SysTick_Config(12000000/1000); // 1000 Hz

    // send out a greeting twice a second
    while (true) {
        printf("Hello world!\n");
        delay(500);
    }
}
