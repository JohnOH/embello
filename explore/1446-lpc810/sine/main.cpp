// Generate a 50 Hz sine wave on PIO0_3 / pin 3 of the LPC810.
// See http://jeelabs.org/2014/11/19/getting-started-episode-3/
//
// Needs a 1 KOhm + 1 uF RC filter to weed out a lot of the switching noise.
//
// The 1-bit sigma-delta DAC synthesis was adapted from code by Jan Ostman,
// see http://www.hackster.io/janost/micro-virtual-analog-synthesizer

#include "LPC8xx.h"
#include "sine.h"

int32_t phase;
volatile int32_t dac;

#define SYSPLLCTRL_Val      0x24
#define SYSPLLCLKSEL_Val    0
#define MAINCLKSEL_Val      3
#define SYSAHBCLKDIV_Val    2

static void setMaxSpeed () {
    LPC_SYSCON->SYSPLLCLKSEL = SYSPLLCLKSEL_Val;    // select PLL input
    LPC_SYSCON->SYSPLLCLKUEN = 0x01;                // update clock source
    while (!(LPC_SYSCON->SYSPLLCLKUEN & 0x1)) ;     // wait until updated

    LPC_SYSCON->SYSPLLCTRL = SYSPLLCTRL_Val;        // main clock is PLL out
    LPC_SYSCON->PDRUNCFG &= ~(1<<7);                // power-up SYSPLL
    while (!(LPC_SYSCON->SYSPLLSTAT & 0x1)) ;       // wait until PLL locked

    LPC_SYSCON->MAINCLKSEL = MAINCLKSEL_Val;        // select PLL clock output
    LPC_SYSCON->MAINCLKUEN = 0x01;                  // update MCLK clock source
    while (!(LPC_SYSCON->MAINCLKUEN & 0x1)) ;       // wait until updated

    LPC_SYSCON->SYSAHBCLKDIV = SYSAHBCLKDIV_Val;
}

int main () {
    setMaxSpeed();                      // set maximum clock speed
    LPC_SYSCON->IRQLATENCY = 0;         // minimal interrupt delay

    LPC_SYSCON->SYSAHBCLKCTRL |= 1<<6;  // enable GPIO
    LPC_SYSCON->PRESETCTRL |= 1<<10;    // reset GPIO block
    LPC_SWM->PINENABLE0 |= 1<<2;        // disable SWCLK
    LPC_GPIO_PORT->DIR0 |= 1<<3;        // set pin 3 as output

    SysTick_Config (30000000 / (50 * 1024)); // out 50 Hz of 1024 samples each

    // use masked pin access to make the following loop as fast as possible
    LPC_GPIO_PORT->MASK0 = ~(1<<3);
    uint32_t err = 0;
    while (true) {
        err = (uint16_t) err - dac;
        // set pin 3 if dac > err, else clear pin 3
        LPC_GPIO_PORT->MPIN0 = err >> 16;
    }
}

extern "C" void SysTick_Handler () {
    uint8_t off = ++phase;
    // inverted offset in 2nd and 4th quadrant
    if (phase & (1<<8))
        off = ~off; // 0..255 -> 255..0
    int ampl = sineTable[off];
    // negative amplitude in 3rd and 4th quadrant
    if (phase & (1<<9))
        dac = (1<<15) - ampl;
    else
        dac = (1<<15) + ampl;
}
