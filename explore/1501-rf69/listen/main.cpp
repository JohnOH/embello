// Report received data on the serial port of the LPC810.

#include <stdio.h>
#include "serial.h"

#define chThdYield() // FIXME still used in radio.h
#include "spi.h"
#include "radio.h"

RF69<SpiDevice> rf;
uint8_t rxBuf[66];

int main () {
    LPC_SWM->PINASSIGN0 = 0xFFFFFF04; // only connect TXD
    serial.init(LPC_USART0, 115200);
    printf("\n[listen]\n");

    // disable SWCLK/SWDIO and RESET
    LPC_SWM->PINENABLE0 |= (3<<2) | (1<<6);
    // lpc810 coin: sck=0, ssel=1, miso=2, mosi=5
    LPC_SWM->PINASSIGN3 = 0x00FFFFFF;   // sck  -    -    -
    LPC_SWM->PINASSIGN4 = 0xFF010205;   // -    nss  miso mosi

    rf.init(1, 42, 8683);
    while (true) {
        int len = rf.receive(rxBuf, sizeof rxBuf);
        if (len >= 0) {
            printf("OK ");
            for (int i = 0; i < len; ++i)
                printf("%02x", rxBuf[i]);
            printf(" (%d%s%d:%d)\n",
                    rf.rssi, rf.afc < 0 ? "" : "+", rf.afc, rf.lna);
        }
    }
}
