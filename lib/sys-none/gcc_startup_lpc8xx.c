// gcc startup for LPC8xx
// Kamal Mostafa <kamal@whence.com>
// modified by jcw
//
// License: mixed
// - simple crt0 and statically allocated stack (Kamal Mostafa; Public Domain)
// - Note: empty boilerplate ISR routines from cr_startup_lpc8xx.c

#include "LPC8xx.h"

extern int main (void);

// Allocated stack space
extern void _vStackTop(void);

#define WEAK __attribute__ ((weak))
#define ALIAS(f) __attribute__ ((weak, alias (#f)))

// Forward declaration of the default handlers. These are aliased.
// When the application defines a handler (with the same name), this will
// automatically take precedence over these weak definitions

WEAK void ResetISR(void);
WEAK void IntDefaultHandler(void);

// Forward declaration of the specific IRQ handlers. These are aliased
// to the IntDefaultHandler, which is a 'forever' loop. When the application
// defines a handler (with the same name), this will automatically take
// precedence over these weak definitions

void NMI_Handler(void) ALIAS(IntDefaultHandler);
void HardFault_Handler(void) ALIAS(IntDefaultHandler);
void SVC_Handler(void) ALIAS(IntDefaultHandler);
void PendSV_Handler(void) ALIAS(IntDefaultHandler);
void SysTick_Handler(void) ALIAS(IntDefaultHandler);

void SPI0_IRQHandler(void) ALIAS(IntDefaultHandler);
void SPI1_IRQHandler(void) ALIAS(IntDefaultHandler);
void UART0_IRQHandler(void) ALIAS(IntDefaultHandler);
void UART1_IRQHandler(void) ALIAS(IntDefaultHandler);
void UART2_IRQHandler(void) ALIAS(IntDefaultHandler);
void I2C0_IRQHandler(void) ALIAS(IntDefaultHandler);
void I2C1_IRQHandler(void) ALIAS(IntDefaultHandler);
void I2C2_IRQHandler(void) ALIAS(IntDefaultHandler);
void I2C3_IRQHandler(void) ALIAS(IntDefaultHandler);
void SCT_IRQHandler(void) ALIAS(IntDefaultHandler);
void MRT_IRQHandler(void) ALIAS(IntDefaultHandler);
void CMP_IRQHandler(void) ALIAS(IntDefaultHandler);
void WDT_IRQHandler(void) ALIAS(IntDefaultHandler);
void BOD_IRQHandler(void) ALIAS(IntDefaultHandler);
void FLASH_IRQHandler(void) ALIAS(IntDefaultHandler);
void WKT_IRQHandler(void) ALIAS(IntDefaultHandler);
void ADC_SEQA_IRQHandler(void) ALIAS(IntDefaultHandler);
void ADC_SEQB_IRQHandler(void) ALIAS(IntDefaultHandler);
void ADC_THCMP_IRQHandler(void) ALIAS(IntDefaultHandler);
void ADC_OVR_IRQHandler(void) ALIAS(IntDefaultHandler);
void DMA_IRQHandler(void) ALIAS(IntDefaultHandler);
void PIN_INT0_IRQHandler(void) ALIAS(IntDefaultHandler);
void PIN_INT1_IRQHandler(void) ALIAS(IntDefaultHandler);
void PIN_INT2_IRQHandler(void) ALIAS(IntDefaultHandler);
void PIN_INT3_IRQHandler(void) ALIAS(IntDefaultHandler);
void PIN_INT4_IRQHandler(void) ALIAS(IntDefaultHandler);
void PIN_INT5_IRQHandler(void) ALIAS(IntDefaultHandler);
void PIN_INT6_IRQHandler(void) ALIAS(IntDefaultHandler);
void PIN_INT7_IRQHandler(void) ALIAS(IntDefaultHandler);

// The vector table.
// This relies on the linker script to place at correct location in memory.
extern void (* const g_pfnVectors[])(void);

__attribute__ ((section(".isr_vector")))
void (* const g_pfnVectors[])(void) = {
        // Core Level - CM0plus
    &_vStackTop,                        // The initial stack pointer
    ResetISR,                           // The reset handler
    NMI_Handler,                        // The NMI handler
    HardFault_Handler,                  // The hard fault handler
    0,                                  // Reserved
    0,                                  // Reserved
// bytes 24..27 are used as shadow pointer of ResetISR for JeeBoot:
    0,                                  // Reserved
// determined by trial and error: vector needs to have at least 8 entries
// with bytes 28..31 used to hold the ROM boot checksum:
    0,                                  // Reserved
#ifndef STARTUP_NO_IRQS
    0,                                  // Reserved
    0,                                  // Reserved
    0,                                  // Reserved
    SVC_Handler,                        // SVCall handler
    0,                                  // Reserved
    0,                                  // Reserved
    PendSV_Handler,                     // The PendSV handler
    SysTick_Handler,                    // The SysTick handler
        // Chip Level - LPC8xx
    SPI0_IRQHandler,                    // SPI0 controller
    SPI1_IRQHandler,                    // SPI1 controller
    0,                                  // Reserved
    UART0_IRQHandler,                   // UART0
    UART1_IRQHandler,                   // UART1
    UART2_IRQHandler,                   // UART2
    0,                                  // Reserved
    I2C1_IRQHandler,                    // I2C1 controller
    I2C0_IRQHandler,                    // I2C0 controller
    SCT_IRQHandler,                     // Smart Counter Timer
    MRT_IRQHandler,                     // Multi-Rate Timer
    CMP_IRQHandler,                     // Comparator
    WDT_IRQHandler,                     // PIO1 (0:11)
    BOD_IRQHandler,                     // Brown Out Detect
    FLASH_IRQHandler,                   // FLASH controller
    WKT_IRQHandler,                     // Wakeup timer
    ADC_SEQA_IRQHandler,                // ADC SEQA
    ADC_SEQB_IRQHandler,                // ADC SEQB
    ADC_THCMP_IRQHandler,               // ADC Threashold Compare
    ADC_OVR_IRQHandler,                 // ADC Overrun
    DMA_IRQHandler,                     // DMA controller
    I2C2_IRQHandler,                    // I2C2 controller
    I2C3_IRQHandler,                    // I2C3 controller
    0,                                  // Reserved
    PIN_INT0_IRQHandler,                 // PIO INT0
    PIN_INT1_IRQHandler,                 // PIO INT1
    PIN_INT2_IRQHandler,                 // PIO INT2
    PIN_INT3_IRQHandler,                 // PIO INT3
    PIN_INT4_IRQHandler,                 // PIO INT4
    PIN_INT5_IRQHandler,                 // PIO INT5
    PIN_INT6_IRQHandler,                 // PIO INT6
    PIN_INT7_IRQHandler,                 // PIO INT7
#endif
}; /* End of g_pfnVectors */

extern unsigned int _etext, _data, _edata, _bss, _ebss;

// Simple gcc- and g++-compatible C runtime init
extern unsigned int __init_array_start;
extern unsigned int __init_array_end;

static inline void preinit (void) {
  unsigned int psp, reg;

  /* Process Stack initialization, it is allocated starting from the
     symbol __process_stack_end__ and its lower limit is the symbol
     __process_stack_base__.*/
  asm volatile ("cpsid   i");
  //psp = SYMVAL(__process_stack_end__);
  psp = (unsigned int) &_data + 100;
  asm volatile ("msr     PSP, %0" : : "r" (psp));

#if CORTEX_USE_FPU
  /* Initializing the FPU context save in lazy mode.*/
  SCB_FPCCR = FPCCR_ASPEN | FPCCR_LSPEN;

  /* CP10 and CP11 set to full access.*/
  SCB_CPACR |= 0x00F00000;

  /* FPSCR and FPDSCR initially zero.*/
  reg = 0;
  asm volatile ("vmsr    FPSCR, %0" : : "r" (reg) : "memory");
  SCB_FPDSCR = reg;

  /* CPU mode initialization, enforced FPCA bit.*/
  reg = CRT0_CONTROL_INIT | 4;
#else
  /* CPU mode initialization.*/
  reg = 0x02;
#endif
  asm volatile ("msr     CONTROL, %0" : : "r" (reg));
  asm volatile ("isb");
}

static inline void crt0 (void)
{
    unsigned int *src, *dest;

    // copy the data section
    src  = &_etext;
    dest = &_data;
    while (dest < &_edata)
        *(dest++) = *(src++);

    // blank the bss section
    while (dest < &_ebss)
        *(dest++) = 0;

    // call C++ constructors
    dest = &__init_array_start;
    while (dest < &__init_array_end)
      (*(void(**)(void)) dest++)();
}

// Reset entry point. Sets up a simple C runtime environment.
__attribute__ ((section(".after_vectors"), naked))
void ResetISR (void)
{
    LPC_FLASHCTRL->FLASHCFG = 0;        // 1 flash clock instead of 2
#ifdef __USE_CMSIS
    SystemInit();
#endif
    //preinit();
    crt0();
    main();
    while (1) ; // hang if main returns
}

// Processor ends up here if an unexpected interrupt occurs or a specific
// handler is not present in the application code.
__attribute__ ((section(".after_vectors")))
void IntDefaultHandler(void)
{
    while(1) ;
}
