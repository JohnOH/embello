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

#include <stdlib.h>
#include <string.h>

extern "C" {

int hal_io_input(int port) {
    if (port == 1)
        return Serial.available() ? Serial.read() : 0;
    return 0;
}

void hal_io_output(int port, int value) {
    if (port == 1)
        Serial.print((char) (value & 0x7F));
}

#include "i8080.h"
#include "i8080_hal.h"
}

const uint8_t rom[] = {
#include "rom.h"
};

void setup () {
    Serial.begin(115200);
    Serial.println("[emu8080]");

    unsigned char* mem = i8080_hal_memory();
    memset(mem, 0, 0x4000);
    memcpy(mem, rom, sizeof rom);

    i8080_jump(0x0000);
    while (true)
        i8080_instruction();
}

void loop () {}
