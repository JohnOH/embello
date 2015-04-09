// Command line interface using the serial port.

#include "sys.h"
#include "uart_irq.h"
#include <string.h>

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

  void cmd_ram_plus () {
    top += 0x10000000;
  }

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

  const Def commands [] = {
    { "nl", cmd_nl },
    { "ram+", cmd_ram_plus },
    { "dump", cmd_dump },
    { 0, 0 }
  };
}

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[cmder]\n");
  
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

  while (true) {
    int ch = uart0RecvChar();
    if (ch >= 0)
      printf("%d\n", ch);
  }
}
