// PDP-8 emulator, ported from Posix to embedded libopencm3
// -jcw, 2016-08-29

#include <stdio.h>

int rxData = -1;
extern int txReady (void);
extern void txSend (char ch);
extern void run (void);

const unsigned char program[] = {
#include "focal.h"
};

#define MEMSIZE 4096
typedef unsigned short Word;
Word pc, mem [MEMSIZE];

static Word mask(Word w) { return w & 07777; }
static Word lmask(Word w) { return w & 017777; }

static Word opAddr (int ir) {
    Word a = ir & 0177;
    if (ir & 0200)
        a |= (pc - 1) & 07600;
    if (ir & 0400) {
        if ((a & 07770) == 010)
            mem[a] = mask(mem[a] + 1);
        a = mem[a];
    }
    return a;
}

static int loader (const unsigned char* ptr) {
    Word addr = 0;
    printf("LOAD");
    Word b;
    for (;;) {
        // read next word
        b = *ptr++;
        if (b & 0200) // skip run-in
            continue;
        b = (b << 6) | *ptr++;

        // look for run-out, to ignore word before it as being a checksum
        int c = *ptr;
        if (c & 0200)
            break;

        // process one word
        if (b & 010000) {
            if (addr != 0)
                printf("-%04o", addr - 1);
            addr = mask(b);
            printf(" %04o", addr);
        } else
            mem[addr++] = b;
    }
    printf("-%04o CHECK %04o\n", mask(addr - 1), mask(b));
    return addr;
}

void run () {
    if (loader(program) == 0)
        return;

    pc = 0200;
    Word sr = 0;

    Word ac = 0, mq = 0;
    int iena = 0;
    for (;;) {
        iena >>= 1; // delayed interrupt enabling, relies on sign extension

        static short counter; // HACK: every 1024 ops, we fake an interrupt
        if ((iena & 1) && (++counter & 0x03FF) == 0) {
            mem[0] = pc;
            pc = 1;
            iena = 0;
        }

        int ir = mem[pc];
        //if (argc >= 5)
        //    printf("PC %04o IR %04o\r\n", pc, ir);
        pc = mask(pc + 1);
        switch ((ir >> 9) & 07) {

            case 0: // AND
                ac &= mem[opAddr(ir)] | 010000;
                break;

            case 1: // TAD
                ac = lmask(ac + mem[opAddr(ir)]);
                break;

            case 2: { // ISZ
                Word t = opAddr(ir);
                mem[t] = mask(mem[t] + 1);
                if (mem[t] == 0)
                    pc = mask(pc + 1);
                break;
            }

            case 3: // DCA
                mem[opAddr(ir)] = mask(ac);
                ac &= 010000;
                break;

            case 4: { // JMS
                Word t = opAddr(ir);
                mem[t] = pc;
                pc = mask(t + 1);
                break;
            }

            case 5: // JMP
                pc = opAddr(ir);
                break;

            case 6: // IOT
                switch ((ir >> 3) & 077) {
                    case 00:
                        switch (ir) {
                            case 06001: iena = ~1; break; // delays one cycle
                            case 06002: iena = 0; break;
                            default: printf("IOT %04o AC=%04o\r\n", ir, ac);
                        }
                        break;
                    case 03: // keyboard
                        if ((ir & 01) && rxData >= 0) // skip if ready
                            pc = mask(pc + 1);
                        if (ir & 04) { // read byte
                            int b = rxData & 0xFF;
                            rxData = -1;
                            ac = (ac & 010000) | b;
                        }
                        break;
                    case 04: // teleprinter
                        if ((ir & 01) && txReady()) // skip if ready
                            pc = mask(pc + 1);
                        if (ir & 04) // send byte
                            txSend(ac & 0177); // strip off parity
                        if (ir & 02) // clear flag
                            ac &= 010000;
                        break;
                    default:
                        //printf("IOT %04o AC=%04o\r\n", ir, ac);
                        break;
                }
                break;

            case 7: // OPR
                if ((ir & 0400) == 0) { // group 1
                    if (ir & 0200) // CLA
                        ac &= 010000;
                    if (ir & 0100) // CLL
                        ac &= 07777;
                    if (ir & 040) // CMA
                        ac ^= 07777;
                    if (ir & 020) // CML
                        ac ^= 010000;
                    if (ir & 01) // IAC
                        ac = lmask(ac + 1);
                    switch (ir & 016) {
                        case 012: // RTR
                            ac = lmask((ac >> 1) | (ac << 12)); // fall through
                        case 010: // RAR
                            ac = lmask((ac >> 1) | (ac << 12));
                            break;
                        case 06: // RTL
                            ac = lmask((ac >> 12) | (ac << 1)); // fall through
                        case 04: // RAL
                            ac = lmask((ac >> 12) | (ac << 1));
                            break;
                        case 02: // BSW
                            ac = (ac & 010000) | ((ac >> 6) & 077)
                                                | ((ac << 6) & 07700);
                            break;
                    }
                } else if ((ir & 01) == 0) { // group 2
                    // SMA, SPA, SZA, SNA, SNL, SZL
                    int s = ((ir & 0100) && (ac & 04000)) ||
                            ((ir & 040) && (ac & 07777) == 0) ||
                            ((ir & 020) && (ac & 010000) != 0) ? 0 : 010;
                    if (s == (ir & 010))
                        pc = mask(pc + 1);
                    if (ir & 0200) // CLA
                        ac &= 010000;
                    if (ir & 04) // OSR
                        ac |= sr;
                    if (ir & 02) { // HLT
                        printf("\r\nHALT %04o", mask(ac));
                        return;
                    }
                } else { // group 3
                    Word t = mq;
                    if (ir & 0200) // CLA
                        ac &= 010000;
                    if (ir & 020) { // MQL
                        mq = ac & 07777;
                        ac &= 010000;
                    }
                    if (ir & 0100)
                        ac |= t;
                }
                break;
        }
    }
}
