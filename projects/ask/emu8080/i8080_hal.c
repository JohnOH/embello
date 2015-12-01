// Intel 8080 (KR580VM80A) microprocessor core model
//
// Copyright (C) 2012 Alexander Demin <alexander@demin.ws>
//
// Credits
//
// Viacheslav Slavinsky, Vector-06C FPGA Replica
// http://code.google.com/p/vector06cc/
//
// Dmitry Tselikov, Bashrikia-2M and Radio-86RK on Altera DE1
// http://bashkiria-2m.narod.ru/fpga.html
//
// Ian Bartholomew, 8080/8085 CPU Exerciser
// http://www.idb.me.uk/sunhillow/8080.html
//
// Frank Cringle, The origianal exerciser for the Z80.
//
// Thanks to zx.pk.ru and nedopc.org/forum communities.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2, or (at your option)
// any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
//
// Tweaked for Arduino-STM32, with Atair 4K Basic 3.2 -jcw, 2015-12-01

#include "i8080_hal.h"

unsigned char memory[0x4000];

int i8080_hal_memory_read_byte(int addr) {
    return memory[addr];
}

void i8080_hal_memory_write_byte(int addr, int byte) {
    if ((unsigned) addr < sizeof memory)
        memory[addr] = byte;
}

int i8080_hal_memory_read_word(int addr) {
    return 
        (i8080_hal_memory_read_byte(addr + 1) << 8) |
        i8080_hal_memory_read_byte(addr);
}

void i8080_hal_memory_write_word(int addr, int word) {
    i8080_hal_memory_write_byte(addr, word & 0xff);
    i8080_hal_memory_write_byte(addr + 1, (word >> 8) & 0xff);
}

int i8080_hal_io_input(int port) {
    extern int hal_io_input(int port);
    return hal_io_input(port);
}

void i8080_hal_io_output(int port, int value) {
    extern void hal_io_output(int port, int value);
    hal_io_output(port, value);
}

void i8080_hal_iff(int on) {
    // Nothing.
}

unsigned char* i8080_hal_memory(void) {
    return &memory[0];
}
