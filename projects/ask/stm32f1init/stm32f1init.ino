// Initialise an STM32F103 with a boot loader, embedded in this code.
// MIT license, see https://github.com/jeelabs/embello -jcw, 2015-11-10

#include <Arduino.h>

#if __AVR_ATmega328P__ // ATmega uses a modified SoftwareSerial implementation

#define Log         Serial  // use Serial for console output

#include "ParitySerial.h"

// the LED, if used, will blink at approx the following rate on 16 MHz ATmega:
// 0.25 Hz = trying to connect, 1 Hz = programming, 5 Hz = error, steady = done

#define LED_PIN     13      // Arduino Dig.13, AVR port B.5, ATMega328 pin.13
#define RESET_PIN   5       // Arduino Dig.5, AVR port D.5, ATMega328 pin.11
#define ISP_PIN     15      // Arduino Ana.1, AVR port C.1, ATMega328 pin.24
#define RX_PIN      4       // Arduino Dig.4, AVR port D.4, ATMega328 pin.6
#define TX_PIN      14      // Arduino Ana.0, AVR port C.0, ATMega328 pin.23

ParitySerial Target (RX_PIN, TX_PIN); // defaults to even parity

static void targetInit () { Target.begin(9600); }

#define SPEED       1       // relative CPU speed for use in busy loops

#else // probably an STM32F103 running at 64 or 72 MHz, with hardware serial

#ifdef ARDUINO_STM_NUCLEO_F103RB
#define Log         Serial1
#define Target      Serial
#else
#define Log         Serial
#define Target      Serial1
#endif

// adjust as needed to blink an LED and to adjust target ISP & RESET modes
//#define LED_PIN     ...
//#define RESET_PIN   ...
//#define ISP_PIN     ...

static void targetInit () { Target.begin(9600, SERIAL_8E1); }

#define SPEED       10      // busy loops run a lot faster than on ATmega328

#endif // __AVR_ATmega328P__

// only uncomment one of the boot loaders mentioned below (or add your own)
// see ./etc/bin2h.c for the utility code used to make these include files
// (moved to https://github.com/jeelabs/embello/tree/master/tools/bin2h)

//#define BOOT_LOADER "boot-maplemini-v20.h"
//#define BOOT_LOADER "boot-usbserup-v01.h"
//#define BOOT_LOADER "boot-bmp-jc66-v01.h"
#define BOOT_LOADER "mecrisp-v221a.h"

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

// toggle the LED to indicate what's going on
static void toggleLed () {
#ifdef LED_PIN
    pinMode(LED_PIN, OUTPUT);
    digitalWrite(LED_PIN, !digitalRead(LED_PIN));
#endif
}

// adjust the control pins to reset the target and get it into ISP mode
static void controlPins (bool isp, bool reset) {
#ifdef ISP_PIN
    pinMode(ISP_PIN, OUTPUT);
    digitalWrite(ISP_PIN, !isp);
#endif
#ifdef RESET_PIN
    pinMode(RESET_PIN, OUTPUT);
    digitalWrite(RESET_PIN, !reset);
#endif
}

// get the specific data byte to send to the target
static uint8_t getData (uint16_t index) {
    if (index >= sizeof data)
        return 0xFF;
    // this is equivalent to data[index] on ARM chips
    return pgm_read_byte(data + index);
}

// read a reply from the target, or return 0 after a certain amount of time
static uint8_t getReply () {
    for (long i = 0; i < SPEED * 100000; ++i)
        if (Target.available())
            return Target.read();
    toggleLed();
    return 0;
}

// wait for an ACK reply, else print an error message and halt
static void wantAck () {
    uint8_t b = 0;
    // wait a bit longer than normal getReply() timeouts, if needed
    for (int i = 0; i < 5; ++i) {
        b = getReply();
        if (b == 0) {
            Log.print('.'); // each retry prints a dot
            continue;
        }
        if (b != ACK)
            break;
        check = 0; // the global checksum is always cleared after an ACK
        return;
    }
    Log.print(" FAILED - got 0x");
    Log.println(b, HEX);

    controlPins(false, false);
    while (true) { // halt, blinking rapidly
        toggleLed();
        delay(100);
    }
}

// send a byte to the target and update the global checkum value
static void sendByte (uint8_t b) {
    check ^= b;
    Target.write(b);
}

// send a command and wait for the corresponding ACK
static void sendCmd (uint8_t cmd) {
    Log.flush();
    sendByte(cmd);
    sendByte(~cmd);
    wantAck();
}

// special infinite loop to trigger target autobaud and wait for ACK or NAK
static void connectToTarget () {
    targetInit();

    uint8_t b = 0;
    do {
        // pulse RESET pin low while keeping ISP pin low
        controlPins(true, false);
        delay(10);
        controlPins(true, true);
        delay(10);
        controlPins(true, false);

        Log.print(".");
        Target.write(0x7F);
        b = getReply();
        //Log.print(b, HEX);
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
    controlPins(false, false);

    Log.begin(115200);
    Log.print("[stm32f1init] ");
    Log.println(BOOT_LOADER);
    Log.println();
    Log.println("(When you see a question mark: RESET your TARGET board!)");
    Log.println();

    Log.print("  Connecting? ");
    connectToTarget();
    Log.println(" OK");

    uint8_t bootRev = getBootVersion();
    Log.print("Boot version: 0x");
    Log.println(bootRev, HEX);

    uint16_t chipType = getChipType();
    Log.print("   Chip type: 0x");
    Log.println(chipType, HEX);

    Log.print("Unprotecting: ");
    sendCmd(RDUNP_CMD);
    wantAck();
    Log.println("OK");

    Log.print("    Resuming? ");
    connectToTarget();
    Log.println(" OK");

    Log.print("     Erasing: ");
    massErase();
    Log.println("OK");
    
    Log.print("     Writing: ");
    for (uint16_t offset = 0; offset < sizeof data; offset += 256) {
        toggleLed();
        Log.print('+');
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
    Log.println(" OK");

    Log.print("        Done: ");
    Log.print(sizeof data);
    Log.println(" bytes uploaded.");

    // pulse RESET pin low while keeping ISP pin high to start target code
    controlPins(false, false);
    delay(10);
    controlPins(false, true);
    delay(10);
    controlPins(false, false);
}

void loop () {}
