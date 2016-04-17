This is the "Energy Monitor" node for central house monitoring at JeeLabs.

Based on an [Olimexino-STM32][O] with STM32F103RB, LiPo backup, and µSD card.

+5V tapped off D5 with extra pin to shield (for powering from FTDI on shield)

BOOT0 = button = PC9
USB power detect = PC11

Arduino headers:

    A0      PC0     analog, reserved
    A1      PC1     analog, reserved
    A2      PC2     analog, reserved
    A3      PC3     analog, reserved
    A4      PC4
    A5      PC5

    D0      PA3     RX2
    D1      PA2     TX2
    D2      PA0
    D3      PA1     yellow LED
    D4      PB5     (UEXT CS)
    D5      PB6     SCL1
    D6      PA8
    D7      PA9     FTDI RX1

    D8      PA10    FTDI TX1
    D9      PB7     SDA1
    D10     PA4     RF69 SSEL
    D11     PA7     RF69 MOSI
    D12     PA6     RF69 MISO
    D13     PA5     RF69 SCLK / green LED
    GND     -       -
    D14     PB8     CAN RX

µSD/MMC card connections
    CS      PD2     MMC CS
    SCLK    PB13    SPI2 SCLK
    DO      PB14    SPI2 MISO2
    DI      PB15    SPI2 MOSI2

UEXT connector
    1   3.3V    -       -
    2   GND     -       -
    3   PA9     TXD1    FTDI
    4   PA10    RXD1    FTDI
    5   PB10    SCL2
    6   PB11    SDA2
    7   PA6     MISO1   RF69
    8   PA7     MOSI1   RF69
    9   PA5     SCLK1   RF69
    10  PB5     UEXT CS

Extension connector
    1   PC15    32 KHz xtal
    2   PB9     CAN tx
    3   PD2     MMC CS
    4   PC10    TX3
    5   PB0     VBAT/4 ?
    6   PB1     ?
    7   PB10    SCL2 / TX3
    8   PB11    SDA2 / RX3
    9   PB12    SPI2 SSEL
    10  PB13    SPI2 SCLK
    11  PB14    SPI2 MISO
    12  PB15    SPI2 MOSI
    13  PC6
    14  PC7
    15  PC8
    16  GND     -
