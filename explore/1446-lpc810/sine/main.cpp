// Generate a 50 Hz sine wave on PIO0_3.
//
// The 1-bit DAC synthesis was adapted from some clever code by Jan Ostman,
// see http://www.hackster.io/janost/micro-virtual-analog-synthesizer

#include "LPC8xx.h"
#include "sine.h"

int32_t phase;
volatile int32_t dac;

static void mrtInit (int count) {
    // enable MRT clock and reset it
    LPC_SYSCON->SYSAHBCLKCTRL |= 1<<10;
    LPC_SYSCON->PRESETCTRL |= 1<<7;

    // config for repeated countdown interrupts
    LPC_MRT->Channel[0].INTVAL = (1<<31) | count;
    LPC_MRT->Channel[0].CTRL = 1<<0;

    // set up the MRT interrupt as NMI
    NVIC_DisableIRQ(MRT_IRQn);
    LPC_SYSCON->NMISRC = (1<<31) | MRT_IRQn;                                       
}

int main () {
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<6) | (1<<18); // enable GPIO and IOCON
    LPC_SYSCON->PRESETCTRL |= 1<<10;
    LPC_SWM->PINENABLE0 |= (1<<2) | (1<<3); // disable SWCLK and SWDIO
    LPC_GPIO_PORT->DIR0 |= 1<<3;
    LPC_IOCON->PIO0_3 = 1<<7; // no pull-up

    mrtInit(12000000 / (50 << 10)); // 50 Hz with 1024 samples each

    uint16_t err = 0;
    LPC_GPIO_PORT->MASK0 = ~(1<<3);
    while (true) {
        //LPC_GPIO_PORT->MPIN0 = dac >= err ? ~0 : 0;
        LPC_GPIO_PORT->MPIN0 = (int) (err - dac) >> 17;
        err -= dac;
    }
}

extern "C" void NMI_Handler () {
    LPC_MRT->Channel[0].STAT = 1<<0; // clear interrupt

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
