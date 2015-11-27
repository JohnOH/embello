// Install boot loader in high flash memory and jump to it.
// -jcw, 2015-011-27

#include <libmaple/util.h>
#include <flash_stm32.h>

#define Log Serial

const int pageBits = 10; // 1K pages on medium-density F103's
const int bootPages = 16; // first 8 KB of flash contains boot loader
const int totalPages = 128; // total flash memory size
const uint32_t flashStart = 0x08000000; // start of flash memory

// need to use a copy of the vector jump table when reflashing low-memory
// this probably needs to be 256-byte aligned, so let's manually put it in RAM
const uint32_t vectorCopy = 0x20002000; // for a copy of the vector table
const uint32_t vectorSize = 0x200;      // more than enough for all vectors

#define pageSize (1 << pageBits)

#define userStart (flashStart)
#define userLimit (flashStart + (totalPages-bootPages) * pageSize)

const uint8_t data[] = {
#include "data.h"
};

void setup () {
    Log.begin(115200);
    Log.println("[installboot]");

#if 0
    memcpy((void*) vectorCopy, (const void*) flashStart, vectorSize);
    uint32_t* SCB_VTOR = (uint32_t*) 0xE000ED08;
    *SCB_VTOR = vectorCopy;
#endif

    FLASH_Unlock();
    FLASH_Status status;

    Log.print("Erasing: ");
    for (int offset = 0; offset < 2*pageSize; offset += pageSize) {
        uint32_t addr = userLimit + offset;
        status = FLASH_ErasePage(addr);
        if (status != FLASH_COMPLETE)
            Log.print(status);
        else
            Log.print('+');
    }
    Log.println(" OK");
    delay(100);

    Log.print("Writing: ");
    for (int i = 0; i < sizeof data; i += 2) {
        uint16_t val = *(const uint16_t*) (data + i);
        status = FLASH_ProgramHalfWord(userLimit + i, val);
        if (status != FLASH_COMPLETE) {
            Log.println();
            Log.print("Status @ 0x");
            Log.print(userLimit + i, HEX);
            Log.print(" = ");
            Log.print(status);
            Log.print(' ');
            break;
        }
        if (i % 256 == 0)
            Log.print('+');
        delay(2);
    }
    Log.println(" OK");
    delay(100);

    Log.print("Verifying: ");
    bool ok = memcmp(data, (const uint8_t*) userLimit, sizeof data) == 0;
    Log.println(ok ? "OK" : "*** NOT OK! ***");
    if (!ok) {
        const uint32_t* p1 = (const uint32_t*) data;
        const uint32_t* p2 = (const uint32_t*) (data + 1024);
        const uint32_t* q1 = (const uint32_t*) userLimit;
        const uint32_t* q2 = (const uint32_t*) (userLimit + 1024);
        for (int i = 0; i < 20; ++i) {
            Log.print(i);
            Log.print(": 0x");
            Log.print(p1[i], HEX);
            Log.print(" = 0x");
            Log.print(q1[i], HEX);
            Log.print(", 0x");
            Log.print(p2[i], HEX);
            Log.print(" = 0x");
            Log.println(q2[i], HEX);
        }
        return; // abort
    }

#if 0
    Log.print("Fixing boot jump: ");
    delay(100);

    FLASH_ErasePage(flashStart);

    for (int i = 0; i < 8; i += 2) {
        uint16_t val = *(const uint16_t*) (data + i);
        status = FLASH_ProgramHalfWord(flashStart + i, val);
        Log.print(status);
        delay(2);
    }
    Log.println(" OK.");
#endif

    FLASH_Lock();
    Log.println("Done - the boot loader has been installed.");

    //*SCB_VTOR = flashStart;
    uint32_t vec1 = ((const uint32_t*) data)[1];
    ((void (*)(void)) vec1)();
}

void loop () {}
