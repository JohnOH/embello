#include "chip.h"

template< int N >
class SpiDev {
  public:
    static void master (int div) {
      LPC_SYSCON->SYSAHBCLKCTRL |= (1<<(11+N)); // enable SPI clock
      LPC_SYSCON->PRESETCTRL &= ~(1<<N);        // reset
      LPC_SYSCON->PRESETCTRL |= (1<<N);         // release

      addr()->DIV = div-1;
      addr()->DLY = 0;

      addr()->CFG = SPI_CFG_MASTER_EN;
      addr()->CFG |= SPI_CFG_SPI_EN;
    }

    static void enable () {
      addr()->TXCTRL = SPI_TXDATCTL_FLEN(8-1);
    }

    static void disable () {
      addr()->STAT |= SPI_STAT_EOT;
    }

    static uint8_t transfer (uint8_t val) {
      addr()->TXDAT = val;
      while ((addr()->STAT & SPI_STAT_RXRDY) == 0)
        ;
      return addr()->RXDAT;
    }

    static uint8_t rwReg (uint8_t cmd, uint8_t val) {
      addr()->TXDATCTL =
        SPI_TXDATCTL_FLEN(16-1) | SPI_TXDATCTL_EOT | (cmd << 8) | val;
      while ((addr()->STAT & SPI_STAT_RXRDY) == 0)
        ;
      return addr()->RXDAT;
    }

    static LPC_SPI_T* addr () { return N == 0 ? LPC_SPI0 : LPC_SPI1; }
};

typedef SpiDev<0> SpiDev0;
typedef SpiDev<1> SpiDev1;
