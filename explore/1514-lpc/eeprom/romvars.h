// Generic implementation of rom-based variables, saved across power cycles.

template < typename FLASH, int BASE >
class RomVars {
  typedef struct { uint16_t key, value; } Tuple;

  enum { Empty = 0xFFFF, Busy = 0x1234, Stable = 0x0000 };
  enum { NumVars = FLASH::PageSize / sizeof (Tuple) };
  enum { BasePage = BASE / FLASH::PageSize };

  class Ref {
    RomVars& owner;
    int index;
  public:
    Ref (RomVars* rv, int i) : owner (*rv), index (i) {}
    operator uint16_t () const { return owner.get(index); }
    uint16_t operator= (uint16_t n) const { return owner.set(index, n); }
  };

public:
  void init () {
    flash.load(BasePage, 1, tuples);
    if (tuples[0].key == Stable)
      for (fill = 1; fill < NumVars; ++fill)
        if (tuples[fill].key > NumVars)
          break;
    else
      pruneAndSave();
  }

  Ref operator[] (int index) {
    return Ref (this, index);
  }

private:
  const void* base (int offset) const {
    return (const void*) ((BasePage + offset) * flash.PageSize);
  }

  uint16_t get (int index) {
    int m = map[index];
    return tuples[m].value;
  }

  uint16_t set (int index, uint16_t value) {
    if (value != tuples[map[index]].value) {
      if (fill >= NumVars) {
        eraseAndSave(1);
        pruneAndSave();
      }
      int m = map[index] = ++fill;
      // printf("set(%d,%u) -> slot %d\n", index, value, m);
      if (m < NumVars) {
        tuples[m].key = index;
        tuples[m].value = value;
        flash.save(BasePage, 1, tuples);
      }
    }
    return value;
  }

  void pruneAndSave () {
    fill = 0;
    memset(map, 0, sizeof map);
    memset(tuples, ~0, sizeof tuples);

    const Tuple* orig = (const Tuple*) base(1);
    if (orig[0].key == Stable)
      for (int i = 1; i < NumVars; ++i) {
        int m = orig[i].key;
        if (m >= NumVars)
          break;
        if (map[m] == 0) {
          if (orig[i].value == 0xFFFF)
            continue; // no need to use an entry for storing the default
          map[m] = ++fill;
        }
        tuples[map[m]] = orig[i];
      }

    eraseAndSave(0);
  }

  void eraseAndSave (int index) {
    flash.erase(BasePage + index, 1);
    tuples[0].key = Busy;
    flash.save(BasePage + index, 1, tuples);
    tuples[0].key = Stable;
    flash.save(BasePage + index, 1, tuples);
  }

  Tuple tuples [NumVars];
  uint8_t fill, map [NumVars];
  FLASH flash;
};
