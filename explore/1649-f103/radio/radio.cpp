// Simple RF69 legacy demo application.

#include <libopencm3/cm3/nvic.h>
#include <libopencm3/stm32/gpio.h>
#include <libopencm3/stm32/usart.h>
#include <libopencm3/stm32/exti.h>
#include <libopencm3/cm3/systick.h>
#include <libopencm3/cm3/cortex.h>
#include <stdio.h>

// defined in main.cpp
extern int serial_getc ();
extern uint32_t millis();

#include "spi.h"
#include "rf69_legacy.h"

RF69<SpiDev> rf;

uint8_t rxBuf[71];	// :grp:dest:len:66 bytes:crc-l:crc-h:
uint8_t txBuf[62];
uint16_t txCnt = 0;

const int rf_freq = 8680;
const int rf_group = 212;
const int rf_nodeid = 28;

const bool verbose = true;
volatile int external;

void exti2_isr(void)           			//ISR function 
{
      if((EXTI_PR & EXTI2) != 0)   		//Check if PA2 has triggered the interrupt
      {                                 
          EXTI_PR |= EXTI2;				//Clear PA2
          external = 1;
      }

}

void setup () {
//	_disable_interrupts();

    // LED on HyTiny F103 is PA1, LED on BluePill F103 is PC13
//    gpio_set_mode(GPIOA, GPIO_MODE_OUTPUT_2_MHZ,
//            GPIO_CNF_OUTPUT_PUSHPULL, GPIO1);
    gpio_set_mode(GPIOC, GPIO_MODE_OUTPUT_2_MHZ,
            GPIO_CNF_OUTPUT_PUSHPULL, GPIO13);
            
    gpio_set_mode(GPIOA, GPIO_MODE_INPUT,	// PA2 Interrupt
            GPIO_CNF_INPUT_PULL_UPDOWN, GPIO2);

    printf("\n[radio]\n");

	rcc_periph_clock_enable(RCC_GPIOA);
	exti_select_source(EXTI2, GPIOA);				// set the AFIO_EXTICR1 register     
	exti_set_trigger(EXTI2, EXTI_TRIGGER_RISING);	// set the EXTI_RTSR register
	exti_enable_request(EXTI2);						// set the EXTI_IMR & EXTI_EMR register

	nvic_enable_irq(EXTI2);
	
    rf.init(rf_nodeid, rf_group, rf_freq);
    //rf.encrypt("mysecret");
    rf.txPower(16); // 0 = min .. 31 = max
	
	printf("RCC_APB2ENR=0x%04X\n", RCC_APB2ENR);
	printf("GPIOA_CRL=0x%08X\n", GPIOA_CRL);
//	printf("GPIOA_CRH=0x%08X\n", GPIOA_CRH);
	printf("GPIOA_IDR=0x%04X\n", GPIOA_IDR);
	printf("GPIOA_ODR=0x%04X\n", GPIOA_ODR);
	printf("GPIOA_IDR=0x%08X\n", GPIOA_IDR);
	printf("GPIOA_BSRR=0x%08X\n", GPIOA_BSRR);
	printf("GPIOA_BRR=0x%04X\n", GPIOA_BRR);
	printf("AFIO_EXTICR1=0x%04X\n", AFIO_EXTICR1);
//	printf("AFIO_EXTICR2=0x%04X\n", AFIO_EXTICR2);
//	printf("AFIO_EXTICR3=0x%04X\n", AFIO_EXTICR3);
//	printf("AFIO_EXTICR4=0x%04X\n", AFIO_EXTICR4);
	printf("EXTI_RTSR=0x%05X\n", EXTI_RTSR);
	printf("EXTI_FTSR=0x%05X\n", EXTI_FTSR);
	printf("EXTI_IMR=0x%05X\n", EXTI_IMR);
	printf("EXTI_EMR=0x%05X\n", EXTI_EMR);

    for (int i = 0; i < (int) sizeof txBuf; ++i)
        txBuf[i] = i;

    printf("  Enter 't' to broadcast a test packet as node %d.\n", rf_nodeid);
    printf("  Listening for packets on %.1f MHz, group %d ...\n\n",
            rf_freq * 0.1, rf_group);
/*
     RCC_APB2ENR = 0x00000045;   //Enable AFIO, GPIOA and GPIOE                              
     GPIOA_CRL = 0x22220000;     //Set GPIOA 4 - 7 as 2MHz Push-pull outputs
     GPIOE_CRH = 0x88888000;     //Set GPIOE 11 - 15 as digital inputs
     GPIOE_ODR = 0x0000F800;     //Enable pullups for Joystick / PE pins 
     GPIOA_ODR = 0x000000F0;     //Set LED / PA 4 - 7 pins high                           
                                                                       
     AFIO_EXTICR3 = 0x00004000;  //Select PE lines for EXTI interfaces                               
     AFIO_EXTICR4 = 0x00004444;  //Select PE lines for EXTI interfaces 
     EXTI_FTSR = 0x0000F800;     //Select falling edge interrupt
     EXTI_IMR = 0x0000F800;      //Unmask bits 11 - 15 for interrupt on those lines
*/ 
 //    NVIC_IntEnable(IVT_INT_EXTI15_10);  //Enable NVIC interface
 //	exti_enable_request(EXTI5);
 //    EnableInterrupts();                 //Enable global interrupt                      



}

void loop () {
    if (serial_getc() == 't') {
        printf("  Broadcasting %d-byte test packet\n", txCnt);
        rf.send(0, txBuf, txCnt);
        txCnt = (txCnt + 1) % sizeof txBuf;
    }

    int len = rf.receive(rxBuf, sizeof rxBuf);
    if (len >= 0 && len <= 72) {
        printf("rf69 %04X%02X%02X%02X%04X g%u i%u l=%u %u",
                rf_freq, rf_group, rf.rssi, rf.lna, rf.afc,
                rxBuf[0], (rxBuf[1] & 0x1F), len, rxBuf[1]);
        for (int i = 3; i < len + 3; ++i)
            printf(" %u", rxBuf[i]);
        const char* sep = rf.afc < 0 ? "" : "+";
        if (verbose)
            printf("  (%g%s%d:%d)", rf.rssi * 0.5, sep, rf.afc, rf.lna);
        putchar('\n');

        gpio_toggle(GPIOA, GPIO1);
        gpio_toggle(GPIOC, GPIO13);

    }
    if (external) {
    	printf("Interrupted\n");
    	external = 0;
    }
}
void SysTick_Handler(void)
{
        gpio_toggle(GPIOA, GPIO1);
        gpio_toggle(GPIOC, GPIO13);
}
