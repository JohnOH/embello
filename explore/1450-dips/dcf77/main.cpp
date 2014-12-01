// DCF77 decoder, sends received time codes to the serial port.
// See http://jeelabs.org/2014/12/10/dips-into-the-lpc810/

#include "stdio.h"
#include "serial.h"

#define PON     2
#define DATA    3

extern "C" void SysTick_Handler () {}

static void delay (int millis) {
    while (--millis >= 0)
        __WFI();
}

static bool waitPulse () {
    for (int i = 0; i < 600; ++i) {
        if (LPC_GPIO_PORT->B0[DATA])
            return true;
        delay(1);
    }
    return false;
}

// collect n bits from the DCF77 receiver, using proper 100/200 ms timing
// return -1 when the long minute has been detected, this is used to resync
// note that this will suspend the calling thread as needed
static int dcfGetBits (int n) {
    int bits = 0;
    for (int i = 0; i < n; ++i) {
        // wait up to 600 ms for a pulse, expected at either 100 or 1100 ms
        if (!waitPulse())
            return -1;
        // here whenever the rising edge just came in
        delay(150);
        // acquire the state of the pin 150 ms later
        bits |= LPC_GPIO_PORT->B0[DATA] << i;
        // ignore any further pin changes for another 750 ms
        delay(750);
    }
    return bits;
}

// extract some bits from given int, and convert BDC to decimal
static int dcfExtractBcd (int* bits, int num) {
    int bcd = *bits & ((1 << num) - 1);
    *bits >>= num;
    return bcd - 6 * (bcd >> 4);
}

static int parity (int v) {
    v ^= v >> 16;
    v ^= v >> 8;
    v ^= v >> 4;
    v ^= v >> 2;
    v ^= v >> 1;
    return v & 1;
}

int main () {
    LPC_SWM->PINASSIGN0 = 0xFFFF0004UL;
    serial.init(LPC_USART0, 115200);
    printf("\n[dcf77]\n");

    SysTick_Config(12000000/1000);          // 1000 Hz

    LPC_SWM->PINENABLE0 |= (1<<2) | (1<<3); // disable SWCLK and SWDIO
    LPC_GPIO_PORT->DIR0 |= 1<<PON;          // set PON as output
    LPC_GPIO_PORT->B0[PON] = 0;             // set PON low

    while (true) {
        int preamble = dcfGetBits(21);
        if (preamble < 0)
            continue;
        printf("DCF %06x ", preamble);
        int minute = dcfGetBits(8);
        if (minute >= 0) {
            printf("M");
            int hour = dcfGetBits(7);
            if (hour >= 0) {
                printf("H");
                int day = dcfGetBits(23);
                if (day >= 0) {
                    printf("D");
                    if (dcfGetBits(2) < 0 && parity(minute) == 0 &&
                                    parity(hour) == 0 && parity(day) == 0) {
                        uint8_t m = dcfExtractBcd(&minute, 7);
                        uint8_t h = dcfExtractBcd(&hour, 6);
                        uint8_t dd = dcfExtractBcd(&day, 6);
                        uint8_t ww = dcfExtractBcd(&day, 3);
                        uint8_t mm = dcfExtractBcd(&day, 5);
                        uint8_t yy = dcfExtractBcd(&day, 8);
                        const char* tz = preamble & (1 << 17) ? "CEST" : "CET";
                        printf(" #%d 20%02d-%02d-%02d %02d:%02d %s\n",
                                    ww, yy, mm, dd, h, m, tz);
                        continue;
                    }
                }
            }
        }
        printf("?\n");
    }
}
