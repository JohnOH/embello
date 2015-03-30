// Generic implementation of rom-based variables, saved across power cycles.

template < typename FLASH, int BASE >
class RomVars {
  typedef struct { uint16_t key, val; } Tuple;

  enum { Empty = 0xFFFF };
  enum { NumVars = FLASH::PageSize / sizeof (Tuple) };
  enum { BasePage = BASE / FLASH::PageSize };

  class Ref {
    RomVars& owner;
    int index;
  public:
    Ref (RomVars* rv, int i) : owner (*rv), index (i) {}
    operator uint16_t () const { return owner.at(index); }
    uint16_t operator= (uint16_t n) const { return owner.set(index, n); }
  };

public:
  void init () {
    if (!reusePage(BasePage) && !reusePage(BasePage + 1)) {
      fill = NumVars / 2;
      memset(map, 0, sizeof map);
      memset(tuples, 0xFF, sizeof tuples);
    }
  }

  Ref operator[] (int index) {
    return Ref (this, index);
  }

private:
  const void* base (int offset) const {
    return (const void*) ((BasePage + offset) * flash.PageSize);
  }

  uint16_t& at (int index) {
    int m = map[index];
    return m == 0 ? values[index] : tuples[m].val;
  }

  uint16_t set (int index, uint16_t value) {
    uint16_t oldVal = at(index);
    if (value != oldVal) {
      if (fill >= NumVars) {
        eraseAndSave(values[0]);
        compact();
      }
      int m = map[index] = ++fill;
      // printf("set(%d,%u) -> slot %d\n", index, value, m);
      if (m < NumVars) {
        tuples[m].key = index;
        tuples[m].val = value;
        flash.save(values[0], tuples);
      }
    }
    return value;
  }

  bool reusePage (int page) {
    flash.load(page, tuples);
    if (values[0] != page)
      return false;

    memset(map, 0, sizeof map);
    for (fill = NumVars / 2; fill < NumVars; ++fill) {
      uint16_t key = tuples[fill].key;
      if (key >= NumVars)
        break;
      map[key] = fill;
    }
    return true;
  }

  void compact () {
    for (int i = 0; i < NumVars; ++i)
      if (map[i] != 0)
        values[i] = tuples[map[i]].val;

    fill = NumVars / 2;
    memset(map, 0, sizeof map);
    memset(tuples, 0xFF, sizeof tuples);
  }

  void eraseAndSave (int page) {
    if (values[0] != page)
      flash.erase(page, 1);
    values[0] = Empty;
    flash.save(page, tuples);
    values[0] = page;
    flash.save(page, tuples);
    flash.erase(page ^ 1, 1);
  }

  union {
    uint16_t values [NumVars];
    Tuple tuples [NumVars];
  };
  uint8_t map [NumVars];
  uint16_t fill;
  FLASH flash;
};
