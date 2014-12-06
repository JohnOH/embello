#include "LPC8xx.h"

template< typename UART >
class Serial {
public:
    void init (UART* u, uint32_t baudRate) {
        uart = u;

        const uint32_t uartclkdiv = 1;
        const uint32_t SystemCoreClock = 12000000;

        // TODO add support for uarts other than uart0
        LPC_SYSCON->UARTCLKDIV = uartclkdiv;
        LPC_SYSCON->SYSAHBCLKCTRL |= (1<<14);   // enable uart0 clock
        LPC_SYSCON->PRESETCTRL &= ~(1<<3);      // reset uart0
        LPC_SYSCON->PRESETCTRL |= (1<<3);       // release uart0

        // configure UART
        uint32_t clk = SystemCoreClock / uartclkdiv;
        LPC_USART0->CFG = 1<<2;                 // 8N1
        LPC_USART0->BRG = clk/16/baudRate - 1;
        LPC_SYSCON->UARTFRGDIV = 0xFF;
        LPC_SYSCON->UARTFRGMULT = (((clk/16) * (LPC_SYSCON->UARTFRGDIV+1)) /
                (baudRate * (uart->BRG+1))) - (LPC_SYSCON->UARTFRGDIV+1);

        uart->STAT = (1<<5) | (1<<11);          // clear status bits
        uart->CFG |= 1<<0;                      // enable uart
    }

    void deInit () {
        // TODO add support for uarts other than uart0
        LPC_SYSCON->SYSAHBCLKCTRL &= ~(1<<14);  // disable uart0 clock
        LPC_SYSCON->PRESETCTRL &= ~(1<<3);      // reset uart0
    }

    void send (uint8_t c) {
        while ((uart->STAT & (1<<2)) == 0)
            ;                               
        uart->TXDATA = c;
    }

private:
    UART* uart;
};

Serial<LPC_USART_TypeDef> serial;

#undef putchar

extern "C" int putchar (int c) {
    if (c == '\n')
        serial.send('\r');
    serial.send(c);
    return 0;
}

extern "C" int puts (const char* s) {
    while (*s)
        putchar(*s++);
    putchar('\n');
    return 0;
}
