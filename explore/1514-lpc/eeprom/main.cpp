// Try out eeprom emulation using upper flash memory.

#include "sys.h"
#include <string.h>

class Flash {
public:
  Flash (int size) : bytes (size) {}

  const void* base (int offset) const {
    return (const void*) (0x1000 - (2 - offset) * bytes);
  }

  void save (const void* ptr, int offset) {
    // TODO save to flash
  }

private:
  int bytes; // TODO could be passed in as template constant
};

template < typename FLASH, int NUM >
class Eeprom {
  enum { S_EMPTY = 0xFFFFFFFF, S_ACTIVE = 0x12345678, S_STABLE = 0x00000000 };
  typedef struct { uint16_t key, value; } Tuple;

  void clear () {
    memset(map, 0, sizeof map);
    memset(data, ~0, sizeof data);
  }

  void collect (int offset) {
    const Tuple* orig = (const Tuple*) flash.base(offset);
    for (int i = 1; i < NUM; ++i) {
      int m = orig[i].key;
      if (m >= NUM)
        break;
      if (map[m] == 0)
        map[m] = ++fill;
      data[map[m]] = orig[i];
    }
  }

public:
  Eeprom () : fill (0), flash (NUM * sizeof (Tuple)) {
    clear();

    const Tuple* orig = (const Tuple*) flash.base(0);
    if (orig[0].value == S_STABLE)
      collect(0);
  }

  uint16_t operator[] (int index) const {
    int m = map[index];
    return data[m].value;
  }

  void set (int index, uint16_t value) {
    if (value == data[map[index]].value)
      return;
    if (fill >= NUM) {
      flash.save(data, 1);
      clear();
      collect(1);
    }
    int m = map[index] = ++fill;
    printf("set(%d,%u) -> slot %d\n", index, value, m);
    data[m].key = index;
    data[m].value = value;
  }

private:
  Tuple data [NUM];
  uint8_t fill, map [NUM];
  FLASH flash;
};

Eeprom<Flash,16> eeprom;

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[hello]\n");

  unsigned i = 0;
  while (true) {
    tick.delay(500);
    int n = ++i % 7;
    printf("%u\n", eeprom[n]);
    eeprom.set(n, tick.millis);
  }
}

// vim: ts=2 sts=2 sw=2
