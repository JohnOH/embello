// Initialise an STM32F103 with a boot loader, embedded in this code.
// MIT license, see https://github.com/jeelabs/embello -jcw, 2015-11-10

#include <Arduino.h>

#ifdef __AVR_ATmega328P__ // use a modified SoftwareSerial implementation
#define USE_SOFT_PARITY 1
#endif

#if USE_SOFT_PARITY

#include "ParitySerial.h"

#define RX_PIN      4   // Arduino Digital.4, ATmega portD.4, ATMega328 pin.6
#define TX_PIN      14  // Arduino Analog.0, ATmega portC.0, ATMega328 pin.23
ParitySerial Target (RX_PIN, TX_PIN); // defaults to even parity

#define TIMEOUT 200000

#else // assume an STM32F103 running at 72 MHz, with hardware serial

#define Target  Serial1
#define TIMEOUT 800000

#endif

// only uncomment one of the boot loaders mentioned below (or add your own)
//#define BOOT_LOADER "boot-maplemini-v20.h"
#define BOOT_LOADER "boot-usbSerial-v01.h"
//#define BOOT_LOADER "boot-bmp-jc66-v01.h"

const uint8_t data[] PROGMEM = {
#include BOOT_LOADER
};

uint8_t check;  // this checksum will get updated on each call to sendByte()

enum { ACK = 0x79, NAK = 0x1F };

// boot request codes sent to the target, based on STM's USART protocol
enum {
    GET_CMD = 0x00,
    GETID_CMD = 0x02,
    WRITE_CMD = 0x31,
    ERASE_CMD = 0x43,
    EXTERA_CMD = 0x44,
    RDUNP_CMD = 0x92,
};

// get the specific data byte to send to the target
static uint8_t getData (uint16_t index) {
    if (index >= sizeof data)
        return 0xFF;
    // this is equivalent to data[index] on ARM chips
    return pgm_read_byte(data + index);
}

// read a reply from the target, or return 0 after a certain amount of time
static uint8_t getReply () {
    for (long i = 0; i < TIMEOUT; ++i)
        if (Target.available())
            return Target.read();
    return 0;
}

// wait for an ACK reply, else print an error message and halt
static void wantAck () {
    uint8_t b = 0;
    // wait a bit longer than normal getReply() timeouts, if needed
    for (int i = 0; i < 5; ++i) {
        b = getReply();
        if (b == 0) {
            Serial.print('.'); // each retry prints a dot
            continue;
        }
        if (b != ACK)
            break;
        check = 0; // the global checksum is always cleared after an ACK
        return;
    }
    Serial.print(" FAILED - got 0x");
    Serial.println(b, HEX);
    while (true) ; // halt
}

// send a byte to the target and update the global checkum value
static void sendByte (uint8_t b) {
    check ^= b;
    Target.write(b);
}

// send a command and wait for the corresponding ACK
static void sendCmd (uint8_t cmd) {
    Serial.flush();
    sendByte(cmd);
    sendByte(~cmd);
    wantAck();
}

// special infinite loop to trigger target autobaud and wait for ACK or NAK
static void connectToTarget () {
#ifdef USE_SOFT_PARITY
    Target.begin(9600);
#else // use serial port hardware, can run much faster
    Target.begin(115200, SERIAL_8E1);
#endif

    uint8_t b = 0;
    do {
        Serial.print(".");
        Target.write(0x7F);
        b = getReply();
    } while (b != ACK && b != NAK); // NAK is fine, it's still a response
}

// boot loader request for the boot version, ignoring the rest
static uint8_t getBootVersion () {
    sendCmd(GET_CMD);
    uint8_t n = getReply();
    uint8_t bootRev = getReply();
    for (int i = 0; i < n; ++i)
        getReply();
    wantAck();
    return bootRev;
}

// boot loader request for the chip type, as 16-bit int
static uint16_t getChipType () {
    sendCmd(GETID_CMD);
    getReply(); // should be 1
    uint16_t chipType = getReply() << 8;
    chipType |= getReply();
    wantAck();
    return chipType;
}

// boot loader request for a mass erase of flash memory
static void massErase () {
#if 1
    sendCmd(ERASE_CMD);
    sendByte((uint8_t) 0xFF);
    sendByte((uint8_t) 0x00);
    wantAck();
#else
    sendCmd(EXTERA_CMD);
    sendByte((uint8_t) 0xFF);
    sendByte((uint8_t) 0xFF);
    sendByte((uint8_t) 0xFF);
    wantAck();
#endif
}

// main sketch logic
void setup () {
    Serial.begin(115200);
    Serial.print("[stm32f1init] ");
    Serial.println(BOOT_LOADER);
    Serial.println();
    Serial.println("(When you see a question mark: RESET your TARGET board!)");
    Serial.println();

    Serial.print("  Connecting? ");
    connectToTarget();
    Serial.println(" OK");

    uint8_t bootRev = getBootVersion();
    Serial.print("Boot version: 0x");
    Serial.println(bootRev, HEX);

    uint16_t chipType = getChipType();
    Serial.print("   Chip type: 0x");
    Serial.println(chipType, HEX);

    Serial.print("Unprotecting: ");
    sendCmd(RDUNP_CMD);
    wantAck();
    Serial.println("OK");

    Serial.print("    Resuming? ");
    connectToTarget();
    Serial.println(" OK");

    Serial.print("     Erasing: ");
    massErase();
    Serial.println("OK");
    
    Serial.print("     Writing: ");
    for (uint16_t offset = 0; offset < sizeof data; offset += 256) {
        Serial.print('+');
        sendCmd(WRITE_CMD);
        uint32_t addr = 0x08000000 + offset;
        sendByte(addr >> 24);
        sendByte(addr >> 16);
        sendByte(addr >> 8);
        sendByte(addr);
        sendByte(check);
        wantAck();
        sendByte(256-1);
        for (int i = 0; i < 256; ++i)
            sendByte(getData(i));
        sendByte(check);
        wantAck();
    }
    Serial.println(" OK");

    Serial.print("        Done: ");
    Serial.print(sizeof data);
    Serial.println(" bytes uploaded.");
}

void loop () {}
