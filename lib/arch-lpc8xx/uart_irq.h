#include "ringbuf.h"

RingBuf<16> txBuf;
RingBuf<64> rxBuf;

extern "C" void UART0_IRQHandler () {
  if (LPC_USART0->STAT & UART_STATUS_RXRDY && !rxBuf.isFull())
    rxBuf.put(LPC_USART0->RXDATA);

  if (LPC_USART0->STAT & UART_STATUS_TXRDY) {
    if (txBuf.isEmpty())
      LPC_USART0->INTENCLR = UART_INTEN_TXRDY;
    else
      LPC_USART0->TXDATA = txBuf.get();
  }
}

void uart0Init (uint32_t baudRate) {
  const uint32_t UARTCLKDIV = 1;

  LPC_SYSCON->UARTCLKDIV = UARTCLKDIV;
  LPC_SYSCON->SYSAHBCLKCTRL |=  (1 << 14);
  //LPC_SYSCON->PRESETCTRL    &= ~(1 << 3);
  LPC_SYSCON->PRESETCTRL    |=  (1 << 3);

  uint32_t clk = SystemCoreClock * LPC_SYSCON->SYSAHBCLKDIV / UARTCLKDIV;
  LPC_USART0->CFG = UART_DATA_LENGTH_8 | UART_PARITY_NONE | UART_STOP_BIT_1;
  LPC_USART0->BRG = clk / 16 / baudRate - 1;
  LPC_SYSCON->UARTFRGDIV = 0xFF;
  LPC_SYSCON->UARTFRGMULT = (((clk / 16) * (LPC_SYSCON->UARTFRGDIV + 1)) /
    (baudRate * (LPC_USART0->BRG + 1))) - (LPC_SYSCON->UARTFRGDIV + 1);

  LPC_USART0->STAT = UART_STATUS_CTSDEL | UART_STATUS_RXBRKDEL;
  NVIC_EnableIRQ(UART0_IRQn);
  LPC_USART0->CFG |= UART_ENABLE;
  LPC_USART0->INTENSET = UART_INTEN_RXRDY;
}

void uart0SendChar (char c) {
  while (txBuf.isFull())
    ;
  LPC_USART0->INTENCLR = UART_INTEN_TXRDY;
  txBuf.put(c);
  LPC_USART0->INTENSET = UART_INTEN_TXRDY;
}

int uart0RecvChar () {
  int c = -1;
  if (!rxBuf.isEmpty()) {
    LPC_USART0->INTENCLR = UART_INTEN_RXRDY;
    c = rxBuf.get();
    LPC_USART0->INTENSET = UART_INTEN_RXRDY;
  }
  return c;
}
