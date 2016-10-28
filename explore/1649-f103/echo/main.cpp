/*
 * This file is part of the libopencm3 project.
 *
 * Copyright (C) 2009 Uwe Hermann <uwe@hermann-uwe.de>,
 * Copyright (C) 2011 Piotr Esden-Tempski <piotr@esden.net>
 *
 * This library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this library.  If not, see <http://www.gnu.org/licenses/>.
 */

#include <libopencm3/stm32/rcc.h>
#include <libopencm3/stm32/gpio.h>
#include <libopencm3/stm32/usart.h>
#include <libopencm3/cm3/systick.h>
#include <stdio.h>
#include <errno.h>

static void clock_setup () {
    rcc_clock_setup_in_hse_8mhz_out_72mhz();

    /* Enable clocks for GPIO port A/C for LED and USART1. */
    rcc_periph_clock_enable(RCC_GPIOA);
    rcc_periph_clock_enable(RCC_GPIOC);
    rcc_periph_clock_enable(RCC_AFIO);
    rcc_periph_clock_enable(RCC_USART1);
}

static void usart_setup () {
    /* Setup GPIO pin GPIO_USART1_RE_TX on GPIO port B for transmit. */
    gpio_set_mode(GPIOA, GPIO_MODE_OUTPUT_50_MHZ,
              GPIO_CNF_OUTPUT_ALTFN_PUSHPULL, GPIO_USART1_TX);

    /* Setup GPIO pin GPIO_USART1_RE_RX on GPIO port B for receive. */
    gpio_set_mode(GPIOA, GPIO_MODE_INPUT,
              GPIO_CNF_INPUT_FLOAT, GPIO_USART1_RX);

    /* Setup UART parameters. */
    usart_set_baudrate(USART1, 115200);
    usart_set_databits(USART1, 8);
    usart_set_stopbits(USART1, USART_STOPBITS_1);
    usart_set_parity(USART1, USART_PARITY_NONE);
    usart_set_flow_control(USART1, USART_FLOWCONTROL_NONE);
    usart_set_mode(USART1, USART_MODE_TX_RX);

    /* Finally enable the USART. */
    usart_enable(USART1);
}

extern "C" int _write (int file, const char *ptr, int len) {
    if (file == 1) {
        for (int i = 0; i < len; ++i)
            usart_send_blocking(USART1, ptr[i]);
        return len;
    }
    errno = EIO;
    return -1;
}

int serial_getc () {
    return USART_SR(USART1) & USART_SR_RXNE ? usart_recv(USART1) : -1;
}

static void systick_setup () {
    /* 72MHz / 8 => 9000000 counts per second. */
    systick_set_clocksource(STK_CSR_CLKSOURCE_AHB_DIV8);

    /* 9000000/9000 = 1000 overflows per second - every 1ms one interrupt */
    /* SysTick interrupt every N clock pulses: set reload to N-1 */
    systick_set_reload(8999);

    systick_interrupt_enable();

    /* Start counting. */
    systick_counter_enable();
}

static uint32_t ticks;

extern "C" void sys_tick_handler () {
    ++ticks;
}

uint32_t millis () {
    return ticks;
}

// the following creates an Arduino-like environment with setup() and loop()

extern void setup ();
extern void loop ();

int main () {
    clock_setup();
    usart_setup();
    systick_setup();

    setup();

    while (true)
        loop();

    // never returns
}
