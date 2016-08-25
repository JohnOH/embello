// PDP-8 emulator, no EAE, only field 0, i.e. 4K words
// -jcw, 2016-08-24

#include <stdio.h>
#include <stdlib.h>
#include <termios.h>
#include <sys/ioctl.h>
#ifdef macosx
#include <sys/filio.h>
#endif

#define MEMSIZE 4096
typedef unsigned short Word;
Word pc, mem [MEMSIZE];

struct termios tiosOrig;
Word mask(Word w) { return w & 07777; }
Word lmask(Word w) { return w & 017777; }

Word opAddr (int ir) {
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

int loader (FILE* fp) {
    Word addr = 0;
    printf("LOAD");
    Word b;
    while (!feof(fp)) {
        if (fgetc(fp) == 0200) // skip until run-in found
            break;
    }
    while (!feof(fp)) {
        // read next word
        b = fgetc(fp);
        if (b & 0200) // skip run-in
            continue;
        b = (b << 6) | fgetc(fp);

        // look for run-out, to ignore word before it as being a checksum
        int c = fgetc(fp);
        if (c & 0200)
            break;
        ungetc(c, fp);

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

void cleanup () {
    tcsetattr(0, TCSANOW, &tiosOrig);
    printf(" PC %04o\n", mask(pc - 1));
    // dump page 0, then exit
    for (int i = 0; i < 0200; i += 010) {
        printf("%04o:", i);
        for (int j = 0; j < 010; ++j)
            printf(" %04o", mem[i+j]);
        printf("\n");
    }
}

int main (int argc, const char* argv[]) {
    if (argc < 2) {
        fprintf(stderr, "Usage: %s binrimfile ?pc? ?sr? ?-v?\n", argv[0]);
        return 1;
    }

    FILE* fp = fopen(argv[1], "r");
    if (fp == 0) {
        perror(argv[1]);
        return 2;
    }
    if (loader(fp) == 0)
        return 3;
    fclose(fp);

    pc = 0200;
    if (argc >= 3)
        pc = strtol(argv[2], 0, 8);

    Word sr = 0;
    if (argc >= 4)
        sr = strtol(argv[3], 0, 8);

    // prepare stdin tty for raw input polling
    struct termios tios = tiosOrig;
    tcgetattr(0, &tiosOrig);
    atexit(cleanup);
    cfmakeraw(&tios);
    tcsetattr(0, TCSANOW, &tios);

    Word ac = 0, mq = 0;
    int iena = 0;
    for (;;) {
        iena >>= 1; // implements delayed enabling, relies on sign extension

        static short counter; // HACK: every 65536 ops, we fake an interrupt
        if ((iena & 1) && ++counter == 0) {
            mem[0] = pc;
            pc = 1;
            iena = 0;
        }

        int ir = mem[pc];
        if (argc >= 5)
            printf("PC %04o IR %04o\r\n", pc, ir);
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
                        if (ir & 01) { // skip if ready
                            int pending;
                            ioctl(0, FIONREAD, &pending);
                            if (pending)
                                pc = mask(pc + 1);
                        }
                        if (ir & 04) { // read byte
                            int b = getchar();
                            if (b == 0x1C) { // exit program on ctrl-backslash
                                printf("\r\nQUIT");
                                return 4;
                            }
                            ac = (ac & 010000) | b;
                        }
                        break;
                    case 04: // teleprinter
                        if (ir & 01) // skip if ready
                            pc = mask(pc + 1);
                        if (ir & 04) { // send byte
                            putchar(ac & 0177); // strip off parity
                            fflush(stdout);
                        }
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
                        return 0;
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
