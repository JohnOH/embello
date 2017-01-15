// Command line interface using the serial port.

#define WITH_TEST     1   // include some test code
#define WITH_RF69     0   // include the RF69 driver
#define WITH_ROMVARS  0   // include the RomVars eeprom code

#include "embello.h"
#include <stdlib.h>
#include <string.h>

#include "uart_irq.h"

#if WITH_RF69
#include "spi.h"
#include "rf69.h"

RF69<SpiDev0> rf;
uint8_t rfBuf[66], myNodeId;
#endif

#if WITH_ROMVARS
#include "flash.h"
#include "romvars.h"

RomVars<Flash64,0x3F80> rom;
#endif

namespace Cmder {
  typedef void (*Cmd)();
  typedef struct { const char* name; Cmd func; } Def;

  extern const Def commands [];

  enum { DATA_STACK = 32 };
  int stack [DATA_STACK], top = 0, *sp = stack;

  enum { PAD_SIZE = 84 };
  char pad [PAD_SIZE+1], padDelim;
  uint8_t padFill = 0;
  const char* inPtr = 0;

  static Cmd lookup (const char* s) {
    for (const Def* p = commands; p->name != 0; ++p)
      if (strcmp(p->name, s) == 0)
        return p->func;
    return 0;
  }

  static int nextCh () {
    if (inPtr == 0)
      return uart0RecvChar();
    int ch = *inPtr++;
    if (ch == 0)
      inPtr = 0;
    return ch;
  }

  static int parse (char delim, bool skipPrefix =false) {
    int ch = nextCh();
    if (ch >= 0) {
      bool stop = ch == 0 || padFill >= PAD_SIZE;
      if (stop || ch == delim || (delim == ' ' && ch <= delim)) {
        if (skipPrefix && padFill == 0 && !stop)
          return -1;
        padDelim = ch;
        pad[padFill] = 0;
        int len = padFill;
        padFill = 0;
        return len;
      }
      pad[padFill++] = ch;
    }
    return -1;
  }

  static void push (int v) { *++sp = top; top = v; }
  static int pop () { int v = top; top = *sp--; return v; }
};

namespace Cmder {

  void cmd_nl () { printf("\n"); }

  void cmd_byte_at () {
    top = *(const uint8_t*) top;
  }
  void cmd_byte_bang () {
    int addr = pop();
    *(uint8_t*) addr = pop();
  }
  void cmd_word_at () {
    top = *(const uint16_t*) top;
  }
  void cmd_word_bang () {
    int addr = pop();
    *(uint16_t*) addr = pop();
  }
  void cmd_at () {
    top = *(const int*) top;
  }
  void cmd_bang () {
    int addr = pop();
    *(int*) addr = pop();
  }
  void cmd_set_bang () {
    int addr = pop();
    *(int*) addr |= pop();
  }
  void cmd_clr_bang () {
    int addr = pop();
    *(int*) addr &= ~ pop();
  }
  void cmd_not_bang () {
    int addr = pop();
    *(int*) addr ^= pop();
  }

  void cmd_add () { top += pop(); }
  void cmd_sub () { top = pop() - top; }
  void cmd_mul () { top *= pop(); }
  void cmd_div () { top = pop() / top; }
  void cmd_mod () { top = pop() % top; }
  void cmd_negate () { top = -top; }

  void cmd_dup () { push(top); }
  void cmd_drop () { pop(); }
  void cmd_swap () { int v = top; top = *sp; *sp = v; }
  void cmd_over () { push(*sp); }

  void cmd_invert () { top = ~top; }
  void cmd_and () { top &= pop(); }
  void cmd_or () { top |= pop(); }
  void cmd_xor () { top ^= pop(); }
  void cmd_lshift () { top = pop() << top; }
  void cmd_rshift () { top = (unsigned) pop() >> top; }
  void cmd_ashift () { top = pop() >> top; }

  void cmd_dot () { printf("%d ", pop()); }
  void cmd_ram_plus () { top += 0x10000000; }

  void cmd_dump () {
    int count = pop();
    for (int addr = pop(); --count >= 0; addr += 16) {
      printf("%08x: ", addr);
      for (int i = 0; i < 16; ++i) {
        if (i % 8 == 0)
          printf(" ");
        if (i < addr % 16)
          printf("   ");
        else
          printf("%02x ", *(const uint8_t*)((addr & ~0xF) + i));
      }
      for (int i = 0; i < 16; ++i) {
        if (i % 8 == 0)
          printf(" ");
        if (i < addr % 16)
          printf(" ");
        else {
          uint8_t v = *(const uint8_t*)((addr & ~0xF) + i);
          if (v < ' ' || v > '~')
            v = '.';
          printf("%c", v);
        }
      }
      printf("\n");
      addr &= ~0xF;
    }
  }

  void cmd_words () {
    for (const Def* p = commands; p->name != 0; ++p)
      printf(" %s", p->name);
    printf("\n");
  }

#if WITH_RF69
  void cmd_rf_init () {
    int freq = pop();
    int group = pop();
    myNodeId = pop();
    rf.init(myNodeId, group, freq);
  }

  void cmd_rf_txpower () {
    rf.txPower(pop()); // 0 = min .. 31 = max
  }
#endif

#if WITH_ROMVARS
  void cmd_rom_at () {
    top = rom[top];
  }

  void cmd_rom_bang () {
    int idx = pop();
    rom[idx] = pop();
  }
#endif

  const Def commands [] = {
    { "nl", cmd_nl },

    { "b@", cmd_byte_at },
    { "b@", cmd_byte_bang },
    { "w@", cmd_word_at },
    { "w!", cmd_word_bang },
    { "@", cmd_at },
    { "!", cmd_bang },
    { "set!", cmd_set_bang },
    { "clr!", cmd_clr_bang },
    { "not!", cmd_not_bang },

    { "+", cmd_add },
    { "-", cmd_sub },
    { "*", cmd_mul },
    { "/", cmd_div },
    { "mod", cmd_mod },
    { "negate", cmd_negate },

    { "dup", cmd_dup },
    { "drop", cmd_drop },
    { "swap", cmd_swap },
    { "over", cmd_over },

    { "invert", cmd_invert },
    { "and", cmd_and },
    { "or", cmd_or },
    { "xor", cmd_xor },
    { "<<", cmd_lshift },
    { ">>", cmd_rshift },
    { "a>>", cmd_ashift },

    { ".", cmd_dot },
    { "ram+", cmd_ram_plus },
    { "dump", cmd_dump },
    { "words", cmd_words },

#if WITH_RF69
    { "rf-init", cmd_rf_init },
    { "rf-txpower", cmd_rf_txpower },
#endif

#if WITH_ROMVARS
    { "rom@", cmd_rom_at },
    { "rom!", cmd_rom_bang },
#endif

    { 0, 0 }
  };
}

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[cmder]\n");

#if WITH_RF69
  // jnp v0.2
  LPC_SWM->PINASSIGN[3] = 0x06FFFFFF;
  LPC_SWM->PINASSIGN[4] = 0xFF080B09;
  LPC_IOCON->PIO0[IOCON_PIO11] |= (1<<8); // std GPIO, not I2C pin
  // LPC_IOCON->PIO0[IOCON_PIO10] |= (1<<8); // std GPIO, not I2C pin
#endif

#if 0
  Cmder::push(0x0000);
  Cmder::push(2);
  Cmder::lookup("dump")();
  Cmder::lookup("nl")();
  Cmder::push(0x0003);
  Cmder::push(2);
  Cmder::lookup("dump")();
  Cmder::lookup("nl")();
  Cmder::push(0x0000);
  Cmder::lookup("ram+")();
  Cmder::push(8);
  Cmder::lookup("dump")();
  Cmder::lookup("nl")();
  Cmder::lookup("words")();
#endif
#if WITH_TEST
  Cmder::inPtr =
    "0 2 dump nl  3 2 dump nl  0 ram+ 8 dump\rwords blah 12345 -12345 * . nl";
#endif

  while (true) {
    int len = Cmder::parse(' ', true);
    if (len >= 0) {
      Cmder::Cmd f = Cmder::lookup(Cmder::pad);
      //printf("d%d '%s' %08x\n", Cmder::padDelim, Cmder::pad, (unsigned) f);
      if (f == 0) {
        // avoid strtol(), it pulls in way too much data (ctype.h, no doubt)
        int v = 0, sign = 1;
        const char* end = Cmder::pad;
        if (*end == '-') {
          ++end;
          sign = -1;
        }
        while ('0' <= *end && *end <= '9')
          v = 10 * v + (*end++ - '0');
        v *= sign;
        // end of strtol re-implementation
        if (end > Cmder::pad && *end == 0)
          Cmder::push(v);
        else
          printf("%s?\n", Cmder::pad);
      } else
        f();
      if (Cmder::padDelim == '\r')
        printf(" ok\n");
    }
#if WITH_RF69
    if (myNodeId != 0) {
      int len = rf.receive(rfBuf, sizeof rfBuf);
      if (len >= 0) {
        printf("RF ");
        for (int i = 0; i < len; ++i)
          printf("%02x", rfBuf[i]);
        printf(" (%d%s%d:%d)\n", rf.rssi, rf.afc < 0 ? "" : "+", rf.afc, rf.lna);
      }
    }
#endif
  }
}
