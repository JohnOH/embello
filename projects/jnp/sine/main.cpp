// Generate a 50 Hz sine wave on PIO0_3 / pin 7 of the LPC810.
// See http://jeelabs.org/2014/11/19/getting-started-episode-3/
//
// Needs a 1 KOhm + 10 uF RC filter to convert PWM to an analog value.
//
// The 1-bit sigma-delta DAC synthesis was adapted from code by Jan Ostman,
// see http://www.hackster.io/janost/micro-virtual-analog-synthesizer

#include "LPC8xx.h"
#include "sine.h"

int phase;  // signal phase: bits 0..7 are step, bits 8..9 are quadrant
int err;    // accumulator for 1-bit DAC error

int main () {
    LPC_SWM->PINENABLE0 |= 1<<4;        // disable SWCLK
    LPC_GPIO_PORT->DIR0 |= 1<<3;        // set pin 3 as output

    SysTick_Config(12000000/(50*1024)); // output 50 Hz of 1024 samples each

    // everything happens in the interrupt code
    while (true)
        __WFI();
}

extern "C" void SysTick_Handler () {
    // about 20 us have passed, time to generate the next step
    uint8_t step = ++phase;
    // inverted offset in 2nd and 4th quadrant
    if (phase & (1<<8))
        step = ~step; // 0..255 -> 255..0
    // look up the sine value, table only has data for first quadrant
    int ampl = sineTable[step];
    // negative amplitude in 3rd and 4th quadrant
    if (phase & (1<<9))
        ampl = - ampl;
    // calculate the error, this evens out over time
    err = (uint16_t) err - (1<<15) - ampl;
    // set pin 3 if dac > err, else clear pin 3
    LPC_GPIO_PORT->W0[3] = err >> 16;
}
