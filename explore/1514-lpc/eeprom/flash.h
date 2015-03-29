// Interface to the LPC8xx flash programming routines in ROM.
// This will disable interrupts for a millisecond or so while running.

class Flash {
public:
  enum { pageSize = 64, pagesPerSector = 16};

  static void erase (int pos, int num) {
    // printf("erase(%d,%d) ", pos, num);
    const int pps = pagesPerSector;
    __disable_irq();
    Chip_IAP_PreSectorForReadWrite(pos / pps, (pos + num - 1) / pps);
    Chip_IAP_ErasePage(pos, pos + num - 1);
    __enable_irq();
  }

  static void save (int pos, int num, const void* ptr) {
    // printf("save(%d,%d) ", pos, num);
    const int pps = pagesPerSector;
    while (--num >= 0) {
      __disable_irq();
      Chip_IAP_PreSectorForReadWrite(pos / pps, pos / pps);
      Chip_IAP_CopyRamToFlash(pos * pageSize, (uint32_t*) ptr, pageSize);
      __enable_irq();
      ++pos;
    }
  }
};
