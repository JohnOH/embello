// Blink an LED by toggling it at 2 Hz via the SysTick timer.

#define chThdYield() // FIXME still used in radio.h

#include "spi.h"
#include "radio.h"

#define LED 10  // GPIO0_10 also happens to be on pin 10

RF69<SpiDevice> rf;

int main () {
    LPC_GPIO_PORT->DIR0 |= 1<<LED;      // define LED as an output pin

    // NSS=13, SCK=17, MISO=14, MOSI=23
    LPC_SWM->PINASSIGN3 = 0x11FFFFFF;   // sck  -    -    -
    LPC_SWM->PINASSIGN4 = 0xFF0D0E17;   // -    nss  miso mosi

    SysTick_Config(12000000/2);         // 2 Hz interrupt rate

    rf.init(1, 42, 8683);
    //rf.encrypt("mysecret");
    rf.txPower(0); // minimal

    while (true) {
        __WFI();                        // wait for SysTick
        rf.send(0, "abc", 3);           // send out one packet
    }
}

extern "C" void SysTick_Handler () {
    LPC_GPIO_PORT->NOT0 = 1<<LED;
}
