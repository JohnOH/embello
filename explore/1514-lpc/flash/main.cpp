// Interface to an SPI-connected 8 MB dataflash memory chip.

#include "sys.h"
#include "spi.h"

template< typename SPI >
class SpiFlash {
public:
  void init () {
    spi.master(1);
    printf("%02x\n", spi.rwReg(0x05, 0));
    printf("%02x\n", spi.rwReg(0x35, 0));
    printf("%02x\n", spi.rwReg(0x9F, 0));
  }

private:
  SPI spi;
};

SpiFlash<SpiDev1> spif;

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[flash]\n");

  LPC_SWM->PINENABLE0 |= 3<<2;          // disable SWCLK/SWDIO
  LPC_SWM->PINASSIGN[4] = 0x02FFFFFF;   // sck  -    -    -
  LPC_SWM->PINASSIGN[5] = 0xFF0D0703;   // -    nss  miso mosi
  spif.init();

  while (true) {
    tick.delay(500);
    printf("%u\n", (unsigned) tick.millis);
    tick.delay(5000);
  }
}
