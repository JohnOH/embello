// Main application logic.

#include <libopencm3/stm32/gpio.h>
#include <libopencm3/stm32/usart.h>
#include <stdio.h>

// defined in main.cpp
extern int serial_getc ();
extern uint32_t millis();

#include "spi.h"
#include "rf69.h"

RF69<SpiDev1> rf;

uint8_t rxBuf[64];
uint8_t txBuf[62];
uint16_t cnt = 0;

void setup () {
    // LED on HyTiny F103 is PA1, LED on BluePill F103 is PC13
    gpio_set_mode(GPIOA, GPIO_MODE_OUTPUT_2_MHZ,
            GPIO_CNF_OUTPUT_PUSHPULL, GPIO1);
    gpio_set_mode(GPIOC, GPIO_MODE_OUTPUT_2_MHZ,
            GPIO_CNF_OUTPUT_PUSHPULL, GPIO13);

    puts("\n[radio]");

    rf.init(28, 6, 8686);
    //rf.encrypt("mysecret");
    rf.txPower(15); // 0 = min .. 31 = max

    for (int i = 0; i < (int) sizeof txBuf; ++i)
        txBuf[i] = i;
}

void loop () {
    int len = rf.receive(rxBuf, sizeof rxBuf);
    if (len >= 0) {
        printf("RF69 %02x ", len);
        for (int i = 0; i < len; ++i)
            printf("%02x", rxBuf[i]);
        const char* sep = rf.afc < 0 ? "" : "+";
        if (true)
            printf(" (%d%s%d:%d)", rf.rssi, sep, rf.afc, rf.lna);
        putchar('\n');
    }
}
