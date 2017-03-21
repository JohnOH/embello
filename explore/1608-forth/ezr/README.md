EZ80 Retro setup

Pin connections:

    PB0 = EZ80 XIN, pin 86
    PB1 = EZ80 RESET, pin 55
    PB4 = EZ80 ZCL, pin 67 (w/ 10 kΩ pull-up)
    PB5 = EZ80 ZDA, pin 69 (w/ 10 kΩ pull-up)

Serial port:

    PA2 = EZ80 RX0, pin 74
    PA3 = EZ80 TX0, pin 73

External 512 KB RAM:

    A0..18 = EZ80 ADDR, pins 1-5/8-13/16-21/24-25
    D0..7 = EZ80 DATA, pins 39-46
    CEN = EZ80 CS0, pin 33
    OEN = EZ80 RDN, pin 51
    WEN = EZ80 WRN, pin 52

Micro SD card:
 
    PA4 = µSD SSEL (CS)
    PA5 = µSD SCLK (CK)
    PA6 = µSD MISO (DO)
    PA7 = µSD MOSI (DI)
