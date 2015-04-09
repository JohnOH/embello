// Command line interface using the serial port.

#define WITH_TEST 1
#define WITH_RF69 1

#include "sys.h"
#include <string.h>

#include "uart_irq.h"

#if WITH_RF69
#define chThdYield() // FIXME still used in rf69.h
#include "spi.h"
#include "rf69.h"

RF69<SpiDev0> rf;
uint8_t rfBuf[66], myNodeId;
#endif

namespace Cmder {
  typedef void (*Cmd)();
  typedef struct { const char* name; Cmd func; } Def;

  extern const Def commands [];

  enum { DATA_STACK = 32 };
  int stack [DATA_STACK], top, *sp = stack;

  static Cmd lookup (const char* s) {
    for (const Def* p = commands; p->name != 0; ++p)
      if (strcmp(p->name, s) == 0)
        return p->func;
    return 0;
  }

  static void push (int v) { *++sp = top; top = v; }
  static int pop () { int v = top; top = *sp--; return v; }
};

namespace Cmder {

  void cmd_nl () {
    printf("\n");
  }

  void cmd_add () { top += pop(); }
  void cmd_sub () { top = pop() - top; }
  void cmd_mul () { top *= pop(); }
  void cmd_div () { top = pop() / top; }
  void cmd_mod () { top = pop() % top; }
  void cmd_negate () { top = -top; }

  void cmd_invert () { top = ~top; }
  void cmd_and () { top &= pop(); }
  void cmd_or () { top |= pop(); }
  void cmd_xor () { top ^= pop(); }
  void cmd_lshift () { top = pop() << top; }
  void cmd_rshift () { top = (unsigned) pop() >> top; }

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

  const Def commands [] = {
    { "nl", cmd_nl },

    { "add", cmd_add },
    { "sub", cmd_sub },
    { "mul", cmd_mul },
    { "div", cmd_div },
    { "mod", cmd_mod },
    { "negate", cmd_negate },

    { "invert", cmd_invert },
    { "and", cmd_and },
    { "or", cmd_or },
    { "xor", cmd_xor },
    { "lshift", cmd_lshift },
    { "rshift", cmd_rshift },

    { "ram+", cmd_ram_plus },
    { "dump", cmd_dump },

#if WITH_RF69
    { "rf-init", cmd_rf_init },
    { "rf-txpower", cmd_rf_txpower },
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
  //rf.init(63, 255, 999); // nonsense values, won't receive anything
#endif
  
#if WITH_TEST
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
  // 0 2 dump nl  3 2 dump nl  0 ram+ 8 dump
#endif

  while (true) {
    int ch = uart0RecvChar();
    if (ch >= 0)
      printf("%d\n", ch);
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
