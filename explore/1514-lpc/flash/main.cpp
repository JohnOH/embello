// Interface to an SPI-connected 8 MB dataflash memory chip.

#include "sys.h"
#include "spi.h"

template< typename SPI >
class SpiFlash {
public:
  void init () {
    spi.master(3);
    spi.addr()->DLY = 0x1111;
  }

  int identify () {
    spi.enable();
    spi.transfer(0x9F);
    int v = spi.transfer(0) << 16;
    v |= spi.transfer(0) << 8;
    v |= spi.transfer(0);
    spi.disable();
    return v;
  }

  void eraseSector (int addr) {
    writeEnable();
    cmdWithAddr(0x20, addr);
    startAndWait();
  }

  void program (int addr, const void* data, int count) {
    writeEnable();
    cmdWithAddr(0x02, addr);
    for (int i = 0; i < count; ++i)
      spi.transfer(((const uint8_t*) data)[i]);
    startAndWait();
  }

  void read (int addr, void* data, int count) {
    cmdWithAddr(0x03, addr);
    for (int i = 0; i < count; ++i)
      ((uint8_t*) data)[i] = spi.transfer(0);
    spi.disable();
  }

private:
  SPI spi;

  void writeEnable () {
    spi.enable();
    spi.transfer(0x06); // write enable
    spi.disable();
  }

  void cmdWithAddr (uint8_t cmd, int addr) {
    spi.enable();
    spi.transfer(cmd); // command byte
    spi.transfer(addr >> 16);
    spi.transfer(addr >> 8);
    spi.transfer(addr);
    // not disabled yet!
  }

  void startAndWait () {
    spi.disable();
    while (spi.rwReg(0x05, 0) & 0x01)
      ;
  }
};

SpiFlash<SpiDev1> spif;

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[flash]\n");

  LPC_SWM->PINENABLE0 |= 3<<2;          // disable SWCLK/SWDIO
  LPC_SWM->PINASSIGN[4] = 0x02FFFFFF;   // sck  -    -    -
  LPC_SWM->PINASSIGN[5] = 0xFF0D0703;   // -    nss  miso mosi
  // LPC_IOCON->PIO0[IOCON_PIO7] = 0x80;   // disable pull-up
  // LPC_IOCON->PIO0[IOCON_PIO7] = 0x98;   // repeater mode
  spif.init();

  printf("0x%x\n", spif.identify());

  printf("e");
  for (int n = 0; n < 10; ++n) {
    spif.eraseSector(n << 12);
    printf(" %u", (unsigned) tick.millis);
  }
  printf("\n");

  static uint8_t buf [256];

  printf("p");
  for (int n = 0; n < 10; ++n) {
    buf[0] = 11 * n;
    buf[1] = 22 * n;
    buf[2] = 33 * n;
    spif.program(n << 8, buf, 3);
    printf(" %u", (unsigned) tick.millis);
  }
  printf("\n");

  for (int n = 0; n < 11; ++n) {
    spif.read(n << 8, buf, 3);
    printf("r %d %d,%d,%d %u\n", n, buf[0], buf[1], buf[2],
                                  (unsigned) tick.millis);
  }
}
