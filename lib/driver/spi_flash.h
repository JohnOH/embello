#include "spi.h"

template< typename SPI >
class SpiFlash {
public:
  void init () {
    spi.master(1);
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
