EZ80 Retro setup

Pin connections:

    PB0 = eZ80 XIN, pin 86
    PB1 = eZ80 RESET, pin 55
    PB4 = eZ80 ZCL, pin 67 (w/ 10 kΩ pull-up)
    PB5 = eZ80 ZDA, pin 69 (w/ 10 kΩ pull-up)

Serial port:

    PA2 = eZ80 RX0, pin 74
    PA3 = eZ80 TX0, pin 73

External 512 KB RAM:

    A0..18 = eZ80 ADDR, pins 1-5/8-13/16-21/24-25
    D0..7 = eZ80 DATA, pins 39-46
    CEN = eZ80 CS0, pin 33
    OEN = eZ80 RDN, pin 51
    WEN = eZ80 WRN, pin 52

Micro SD card:
 
    PA4 = µSD SSEL (CS)
    PA5 = µSD SCLK (CK)
    PA6 = µSD MISO (DO)
    PA7 = µSD MOSI (DI)

SPI interconnect, eZ80 is master:

    PA8  = eZ80 PB1,  pin 101 (BUSY: STM => eZ80)
    PB12 = eZ80 PB0,  pin 100 (NSS: eZ80 => STM))
           eZ80 SS,   pin 102 (10k pullup)
    PB13 = eZ80 SCK,  pin 103
    PB14 = eZ80 MISO, pin 106
    PB15 = eZ80 MOSI, pin 107
