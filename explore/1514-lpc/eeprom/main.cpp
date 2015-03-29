// Try out eeprom emulation using upper flash memory.

#include "sys.h"
#include <string.h>

class Flash {
public:
  Flash () {}

  void erase (int pos, int num) {
    printf("erase(%d,%d)\n", pos, num);
    // TODO erase flash
  }

  void save (int pos, int num, const void* ptr) {
    printf("save(%d,%d)\n", pos, num);
    // TODO save to flash
  }
};

template < typename FLASH, int NUM >
class RomVars {
  enum { S_EMPTY = 0xFFFF, S_ACTIVE = 0x1234, S_STABLE = 0x0000 };
  typedef struct { uint16_t key, value; } Tuple;

  const void* base (int offset) const {
    return (const void*) (0x1000 - (2 - offset) * NUM * sizeof (Tuple));
  }

  void pruneAndSave () {
    fill = 0;
    memset(map, 0, sizeof map);
    memset(data, ~0, sizeof data);

    const Tuple* orig = (const Tuple*) base(1);
    if (orig[0].key == S_STABLE)
      for (int i = 1; i < NUM; ++i) {
        int m = orig[i].key;
        if (m >= NUM)
          break;
        if (map[m] == 0) {
          if (orig[i].value == 0xFFFF)
            continue; // no need to use an entry for storing the default
          map[m] = ++fill;
        }
        data[map[m]] = orig[i];
      }

    eraseAndSave(0);
  }

  void eraseAndSave (int index) {
    flash.erase(48 + index, 1);
    data[0].key = S_ACTIVE;
    flash.save(48 + index, 1, data);
    data[0].key = S_STABLE;
    flash.save(48 + index, 1, data);
  }

public:
  void init () {
    const Tuple* orig = (const Tuple*) base(0);
    if (orig[0].value == S_STABLE) {
      memcpy(data, orig, sizeof data);
      for (fill = 1; fill < NUM; ++fill)
        if (data[fill].key > NUM)
          break;
    } else
      pruneAndSave();
  }

  uint16_t operator[] (int index) const {
    int m = map[index];
    return data[m].value;
  }

  void set (int index, uint16_t value) {
    if (value == data[map[index]].value)
      return;
    if (fill >= NUM) {
      eraseAndSave(1);
      pruneAndSave();
    }
    int m = map[index] = ++fill;
    printf("set(%d,%u) -> slot %d\n", index, value, m);
    if (m < NUM) {
      data[m].key = index;
      data[m].value = value;
      flash.save(48, 1, data);
    }
  }

private:
  Tuple data [NUM];
  uint8_t fill, map [NUM];
  FLASH flash;
};

RomVars<Flash,16> rom;

int main () {
  tick.init(1000);
  serial.init(115200);
  rom.init();

  printf("\n[eeprom]\n");

  unsigned i = 0;
  while (true) {
    tick.delay(500);
    int n = ++i % 7;
    printf("%u\n", rom[n]);
    rom.set(n, tick.millis);
  }
}

// vim: ts=2 sts=2 sw=2
