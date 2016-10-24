// Report received data on the serial port of the LPC810.
// See http://jeelabs.org/2015/01/28/lpc810-meets-rfm69-part-3/

#include <stdio.h>
#include "serial.h"

#include "spi.h"
#include "rf69.h"

RF69<SpiDevice> rf;
uint8_t rxBuf[66];

int main () {
    LPC_SWM->PINASSIGN0 = 0xFFFFFF04;   // only connect 4p2 (TXD)
    serial.init(LPC_USART0, 115200);
    printf("\n[listen]\n");

    LPC_SWM->PINENABLE0 |= 3<<2;        // disable SWCLK/SWDIO
    // lpc810 coin: sck=0p8, ssel=3p3, miso=2p4, mosi=1p5
    LPC_SWM->PINASSIGN3 = 0x00FFFFFF;   // sck  -    -    -
    LPC_SWM->PINASSIGN4 = 0xFF030201;   // -    nss  miso mosi

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
