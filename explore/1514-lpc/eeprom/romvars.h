// Generic implementation of rom-based variables, saved across power cycles.

template < typename FLASH, int NUM >
class RomVars {
  typedef struct { uint16_t key, value; } Tuple;

  enum { S_EMPTY = 0xFFFF, S_ACTIVE = 0x1234, S_STABLE = 0x0000 };
  enum { tuplesPerPage = FLASH::pageSize / sizeof (Tuple)};

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
    // printf("set(%d,%u) -> slot %d\n", index, value, m);
    if (m < NUM) {
      data[m].key = index;
      data[m].value = value;
      flash.save(48, 1, data);
    }
  }

private:
  const void* base (int offset) const {
    return (const void*) ((48 + offset) * flash.pageSize);
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

  Tuple data [NUM];
  uint8_t fill, map [NUM];
  FLASH flash;
};
