// DCF77 decoder, sends received time codes to the serial port.
// See http://jeelabs.org/2014/12/10/dip-into-the-lpc810/

#include "stdio.h"
#include "serial.h"

//     DCF77:  GPIO:  8-DIP:
#define VCC     3   // pin 3
#define GND     2   // pin 4
#define DATA    0   // pin 8
#define PON     1   // pin 5

static const char* dayOfWeek [] = {
    "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"
};

extern "C" void SysTick_Handler () {
    // the only effect is to generate an interrupt, no work is done here
}

static void delay (int millis) {
    while (--millis >= 0)
        __WFI(); // wait for the next SysTick interrupt
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

static bool isEvenParity (int v) {
    v ^= v >> 16;
    v ^= v >> 8;
    v ^= v >> 4;
    v ^= v >> 2;
    v ^= v >> 1;
    return (v & 1) == 0;
}

int main () {
    LPC_SWM->PINASSIGN0 = 0xFFFFFF04UL;     // only connect TXD
    serial.init(LPC_USART0, 115200);
    printf("\n[dcf77]\n");

    SysTick_Config(12000000/1000);          // 1000 Hz

    LPC_SWM->PINENABLE0 |= (1<<2)|(1<<3);   // disable SWCLK and SWDIO
    LPC_GPIO_PORT->DIR0 |= (1<<PON)|(1<<VCC)|(1<<GND); // outputs
    LPC_GPIO_PORT->B0[GND] = 0;             // set GND low
    LPC_GPIO_PORT->B0[VCC] = 1;             // set VCC high
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
                    if (dcfGetBits(2) < 0 && isEvenParity(minute) &&
                            isEvenParity(hour) && isEvenParity(day)) {
                        uint8_t m = dcfExtractBcd(&minute, 7);
                        uint8_t h = dcfExtractBcd(&hour, 6);
                        uint8_t dd = dcfExtractBcd(&day, 6);
                        uint8_t ww = dcfExtractBcd(&day, 3);
                        uint8_t mm = dcfExtractBcd(&day, 5);
                        uint8_t yy = dcfExtractBcd(&day, 8);
                        const char* tz = preamble & (1<<17) ? "CEST" : "CET";
                        printf(" %s 20%02d-%02d-%02d %02d:%02d %s\n",
                                    dayOfWeek[ww], yy, mm, dd, h, m, tz);
                        continue;
                    }
                }
            }
        }
        printf("?\n");
    }
}
