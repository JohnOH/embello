/* Write big data file with pseudo-random data and read it back to verify */
/* This can be compiled with either gcc on MacOS or BDS C 1.60 on CP/M */
/* See http://jeelabs.org/article/1717e/ -jcw, 2017-04-26 */

#include <stdio.h>

#define DATAFILE "data.tmp"
#define BUFSIZE 8192
#define BUFCOUNT 160

#ifndef SECSIZ
#define SECSIZ 1
#endif

unsigned r;
char wbuf [BUFSIZE];
char rbuf [BUFSIZE];

main() {
    int fd, i, j, n;

    r = 1;
    fd = creat(DATAFILE, 0666);
    for (i = 0; i < BUFCOUNT; ++i) {
        wbuf[0] = i;
        fill();
        write(fd, wbuf, BUFSIZE/SECSIZ);
        putchar('+');
    }
    close(fd);
    putchar('\n');

    r = 1;
    fd = open(DATAFILE, 0);
    for (i = 0; i < BUFCOUNT; ++i) {
        wbuf[0] = i;
        fill();
        read(fd, rbuf, BUFSIZE/SECSIZ);
        n = 0;
        for (j = 0; j < BUFSIZE; ++j)
            if (wbuf[j] == rbuf[j])
                ++n;
        if (n == BUFSIZE)
            putchar('.');
        else
            putchar('?');
    }
    close(fd);
    putchar('\n');
}

fill() {
    int i, b;

    for (i = 1; i < BUFSIZE; ++i) {
        wbuf[i] = r;
        /* see https://en.wikipedia.org/wiki/Linear-feedback_shift_register */
        b = ((r >> 0) ^ (r >> 2) ^ (r >> 3) ^ (r >> 5) ) & 1;
        r = (r >> 1) | (b << 15);
    }
}
