// Test RGB led dimming using hardware SPI, for the WS2812B chip.
// See http://jeelabs.org/2014/12/10/dips-into-the-lpc810/

#include "serial.h"

static void delayMillis (uint32_t ms) {
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

void spiInit () {
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

void spiSend (uint16_t cmd) {
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

static void showRGB (int r, int g, int b) {
    spiSend(0);
    spiSend(0);
    for (int i = 0; i < 24; ++i) {
        sendByte(g>>4);
        sendByte(r>>4);
        sendByte(b>>4);
        sendByte(g>>2);
        sendByte(r>>2);
        sendByte(b>>2);
        sendByte(g);
        sendByte(r);
        sendByte(b);
    }
    delayMillis(10);
}

int main () {
    SysTick_Config(12000000/1000-1);          // 1000 Hz

    LPC_SWM->PINENABLE0 |= 1<<2;              // disable SWCLK
    LPC_SWM->PINASSIGN4 = 0xFFFFFF03UL;       // SPI0_MOSI 3

    spiInit();

    while (true) {
        int x = 0;

        // red fade up and down
        while (x < 256) showRGB(x++, 0, 0);
        while (x > 0)   showRGB(--x, 0, 0);
        // green fade up and down
        while (x < 256) showRGB(0, x++, 0);
        while (x > 0)   showRGB(0, --x, 0);
        // blue fade up and down
        while (x < 256) showRGB(0, 0, x++);
        while (x > 0)   showRGB(0, 0, --x);

        delayMillis(3000);
    }
}

extern "C" void SysTick_Handler () {}
