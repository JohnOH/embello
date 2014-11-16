// Generate a 50 Hz sine wave on PIO0_3 / pin 3 of the LPC810.
// See http://jeelabs.org/2014/11/19/getting-started-episode-3/
//
// Needs a 1 KOhm + 10 uF RC filter to convert PWM to an analog value.
//
// The 1-bit sigma-delta DAC synthesis was adapted from code by Jan Ostman,
// see http://www.hackster.io/janost/micro-virtual-analog-synthesizer

#include "LPC8xx.h"
#include "sine.h"

int32_t phase;
uint32_t err;

int main () {
    LPC_SWM->PINENABLE0 |= 1<<2;        // disable SWCLK
    LPC_SYSCON->SYSAHBCLKCTRL |= 1<<6;  // enable GPIO
    LPC_SYSCON->PRESETCTRL |= 1<<10;    // reset GPIO block
    LPC_GPIO_PORT->DIR0 |= 1<<3;        // set pin 3 as output

    SysTick_Config(12000000/(50*1024)); // output 50 Hz of 1024 samples each

    // everything happens in the interrupt code
    while (true)
        __WFI();
}

extern "C" void SysTick_Handler () {
    uint8_t off = ++phase;
    // inverted offset in 2nd and 4th quadrant
    if (phase & (1<<8))
        off = ~off; // 0..255 -> 255..0
    // look up the sine value, table only has data for first quadrant
    int ampl = sineTable[off];
    // negative amplitude in 3rd and 4th quadrant
    uint16_t dac = (1<<15) + (phase & (1<<9) ? -ampl : ampl);
    // calculate the error, this evens out over time
    err = (uint16_t) err - dac;
    // set pin 3 if dac > err, else clear pin 3
    LPC_GPIO_PORT->W0[3] = err >> 16;
}
