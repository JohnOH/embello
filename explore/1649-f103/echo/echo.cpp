// Main application logic.

#include <libopencm3/stm32/gpio.h>
#include <stdio.h>

// defined in main.cpp
extern int serial_getc ();
extern uint32_t millis();

void setup () {
    // LED on HyTiny F103 is PA1, LED on BluePill F103 is PC13
    gpio_set_mode(GPIOA, GPIO_MODE_OUTPUT_2_MHZ,
                    GPIO_CNF_OUTPUT_PUSHPULL, GPIO1);
    gpio_set_mode(GPIOC, GPIO_MODE_OUTPUT_2_MHZ,
                    GPIO_CNF_OUTPUT_PUSHPULL, GPIO13);
}

void loop () {
    printf("\nHit <enter> to toggle the LED (%lu) ...", millis());

    // the following loop takes roughly one second
    for (int i = 0; i < 1650000; ++i) {
        if (serial_getc() == '\r') {
            gpio_toggle(GPIOA, GPIO1);
            gpio_toggle(GPIOC, GPIO13);
        }
    }
}
