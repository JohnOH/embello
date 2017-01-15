// Test RGB led dimming using hardware SPI, for the WS2812B chip.
// See http://jeelabs.org/2014/12/10/dip-into-the-lpc810/

#include "LPC8xx.h"

extern "C" void SysTick_Handler () {
    // the only effect is to generate an interrupt, no work is done here
}

static void delay (uint32_t ms) {
    while (ms-- > 0)
        __WFI();
}

#define CFG_ENABLE          (1<<0)
#define CFG_MASTER          (1<<2)

#define TXDATCTL_EOT        (1<<20)
#define TXDATCTL_RX_IGNORE  (1<<22)
#define TXDATCTL_FSIZE(s)   ((s) << 24)

#define STAT_RXRDY          (1<<0)
#define STAT_TXRDY          (1<<1)

static void spiInit () {
    /* Enable SPI0 clock */
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<11);
    LPC_SYSCON->PRESETCTRL &= ~(1<<0);
    LPC_SYSCON->PRESETCTRL |= (1<<0);

    // 2.4 MHz, i.e. 12 MHz / 5
    LPC_SPI0->DIV = 4;
    LPC_SPI0->DLY = 0;

    LPC_SPI0->TXCTRL = TXDATCTL_FSIZE(12-1) | TXDATCTL_RX_IGNORE;

    LPC_SPI0->CFG = CFG_MASTER;
    LPC_SPI0->CFG |= CFG_ENABLE;
}

static void spiSend (uint16_t cmd) {
    while ((LPC_SPI0->STAT & STAT_TXRDY) == 0)
        ;
    LPC_SPI0->TXDAT = cmd;
}

static const uint16_t bits[] = {
    0b100100100100,
    0b100100100110,
    0b100100110100,
    0b100100110110,
    0b100110100100,
    0b100110100110,
    0b100110110100,
    0b100110110110,
    0b110100100100,
    0b110100100110,
    0b110100110100,
    0b110100110110,
    0b110110100100,
    0b110110100110,
    0b110110110100,
    0b110110110110,
};

static void sendByte (int value) {
    spiSend(bits[value >> 4]);
    spiSend(bits[value & 0xF]);
}

static void sendRGB (int r, int g, int b) {
    sendByte(g);
    sendByte(r);
    sendByte(b);
}

static void cometTail (int count, int r, int g, int b) {
    spiSend(0);
    spiSend(0);
    for (int i = 60 - count; i <= 60; ++i) {
        int phase = 4 * i;
        if (phase < 0)
            phase = 0;
        sendRGB(r * phase, g * phase, b * phase);
    }
}

static void cometRacer(int r, int g, int b) {
    for (int i = 0; i < 180; ++i) {
        cometTail(i, r, g, b);
        delay(5);
    }
}

int main () {
    LPC_FLASHCTRL->FLASHCFG = 0;            // 1 wait state instead of 1

    SysTick_Config(12000000/1000);          // 1000 Hz

    LPC_SWM->PINENABLE0 |= 1<<2;            // disable SWCLK
    LPC_SWM->PINASSIGN4 = 0xFFFFFF03UL;     // SPI0_MOSI 3

    spiInit();

    while (true) {
        // these should draw no more than 600 mA max
        cometRacer(1, 0, 0);    // red
        cometRacer(0, 1, 0);    // green
        cometRacer(0, 0, 1);    // blue
        // these draw lots of current: up to 1.5 A peak for 60x white
        //cometRacer(1, 1, 0);  // yellow
        //cometRacer(1, 0, 1);  // magenta
        //cometRacer(0, 1, 1);  // teal
        //cometRacer(1, 1, 1);  // white
    }
}
