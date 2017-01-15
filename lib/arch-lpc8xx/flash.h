// Interface to the LPC8xx flash programming routines in ROM.
// This will disable interrupts for a millisecond or so while running.
// see http://jeelabs.org/2015/04/01/emulating-eeprom/

class Flash64 {
public:
  enum { PageSize = 64 };

  static void load (int pos, void* ptr) {
    memcpy(ptr, (void*) (pos * PageSize), PageSize);
  }

  static void erase (int pos, int num) {
    const int pps = 16; // pages per sector
    __disable_irq();
    Chip_IAP_PreSectorForReadWrite(pos / pps, (pos + num - 1) / pps);
    Chip_IAP_ErasePage(pos, pos + num - 1);
    __enable_irq();
  }

  static void save (int pos, const void* ptr) {
    const int pps = 16; // pages per sector
    __disable_irq();
    Chip_IAP_PreSectorForReadWrite(pos / pps, pos / pps);
    Chip_IAP_CopyRamToFlash(pos * PageSize, (uint32_t*) ptr, PageSize);
    __enable_irq();
  }
};

class Flash1k {
public:
  enum { PageSize = 1024 };

  static void load (int pos, void* ptr) {
    memcpy(ptr, (void*) (pos * PageSize), PageSize);
  }

  static void erase (int pos, int num) {
    __disable_irq();
    Chip_IAP_PreSectorForReadWrite(pos, pos + num - 1);
    Chip_IAP_EraseSector(pos, pos + num - 1);
    __enable_irq();
  }

  static void save (int pos, const void* ptr) {
    __disable_irq();
    Chip_IAP_PreSectorForReadWrite(pos, pos);
    Chip_IAP_CopyRamToFlash(pos * PageSize, (uint32_t*) ptr, PageSize);
    __enable_irq();
  }
};

#include "../../../lib/vendor/lpcopen/src/iap.c"
