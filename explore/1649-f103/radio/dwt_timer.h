/*
 * dwt_timer.h
 *
 *  Created on: Jun 15, 2015
 *      Author: kimballr
 */

#ifndef DWT_TIMER_H_
#define DWT_TIMER_H_

#ifndef DWT_BASE

/* this stuff grabbed from cm3.h */

#ifdef __cplusplus
  #define   __I     volatile             /*!< Defines 'read only' permissions                 */
#else
  #define   __I     volatile const       /*!< Defines 'read only' permissions                 */
#endif
#define     __O     volatile             /*!< Defines 'write only' permissions                */
#define     __IO    volatile             /*!< Defines 'read / write' permissions              */

typedef struct
{
  __IO uint32_t CTRL;                    /*!< Offset: 0x000 (R/W)  Control Register                          */
  __IO uint32_t CYCCNT;                  /*!< Offset: 0x004 (R/W)  Cycle Count Register                      */
  __IO uint32_t CPICNT;                  /*!< Offset: 0x008 (R/W)  CPI Count Register                        */
  __IO uint32_t EXCCNT;                  /*!< Offset: 0x00C (R/W)  Exception Overhead Count Register         */
  __IO uint32_t SLEEPCNT;                /*!< Offset: 0x010 (R/W)  Sleep Count Register                      */
  __IO uint32_t LSUCNT;                  /*!< Offset: 0x014 (R/W)  LSU Count Register                        */
  __IO uint32_t FOLDCNT;                 /*!< Offset: 0x018 (R/W)  Folded-instruction Count Register         */
  __I  uint32_t PCSR;                    /*!< Offset: 0x01C (R/ )  Program Counter Sample Register           */
  __IO uint32_t COMP0;                   /*!< Offset: 0x020 (R/W)  Comparator Register 0                     */
  __IO uint32_t MASK0;                   /*!< Offset: 0x024 (R/W)  Mask Register 0                           */
  __IO uint32_t FUNCTION0;               /*!< Offset: 0x028 (R/W)  Function Register 0                       */
       uint32_t RESERVED0[1];
  __IO uint32_t COMP1;                   /*!< Offset: 0x030 (R/W)  Comparator Register 1                     */
  __IO uint32_t MASK1;                   /*!< Offset: 0x034 (R/W)  Mask Register 1                           */
  __IO uint32_t FUNCTION1;               /*!< Offset: 0x038 (R/W)  Function Register 1                       */
       uint32_t RESERVED1[1];
  __IO uint32_t COMP2;                   /*!< Offset: 0x040 (R/W)  Comparator Register 2                     */
  __IO uint32_t MASK2;                   /*!< Offset: 0x044 (R/W)  Mask Register 2                           */
  __IO uint32_t FUNCTION2;               /*!< Offset: 0x048 (R/W)  Function Register 2                       */
       uint32_t RESERVED2[1];
  __IO uint32_t COMP3;                   /*!< Offset: 0x050 (R/W)  Comparator Register 3                     */
  __IO uint32_t MASK3;                   /*!< Offset: 0x054 (R/W)  Mask Register 3                           */
  __IO uint32_t FUNCTION3;               /*!< Offset: 0x058 (R/W)  Function Register 3                       */
} DWT_Type;

typedef struct
{
  __IO uint32_t DHCSR;                   /*!< Offset: 0x000 (R/W)  Debug Halting Control and Status Register    */
  __O  uint32_t DCRSR;                   /*!< Offset: 0x004 ( /W)  Debug Core Register Selector Register        */
  __IO uint32_t DCRDR;                   /*!< Offset: 0x008 (R/W)  Debug Core Register Data Register            */
  __IO uint32_t DEMCR;                   /*!< Offset: 0x00C (R/W)  Debug Exception and Monitor Control Register */
} CoreDebug_Type;

#define CoreDebug_DEMCR_TRCENA_Pos         24                                             /*!< CoreDebug DEMCR: TRCENA Position */
#define CoreDebug_DEMCR_TRCENA_Msk         (1UL << CoreDebug_DEMCR_TRCENA_Pos)            /*!< CoreDebug DEMCR: TRCENA Mask */

#define DWT_BASE            (0xE0001000UL)                            /*!< DWT Base Address                   */
#define CoreDebug_BASE      (0xE000EDF0UL)                            /*!< Core Debug Base Address            */

#define DWT                 ((DWT_Type * const )      DWT_BASE      )   /*!< DWT configuration struct           */
#define CoreDebug           ((CoreDebug_Type * const) CoreDebug_BASE)   /*!< Core Debug configuration struct    */

#define DWT_CTRL_CYCCNTENA_Pos              0                                          /*!< DWT CTRL: CYCCNTENA Position */
#define DWT_CTRL_CYCCNTENA_Msk             (0x1UL << DWT_CTRL_CYCCNTENA_Pos)           /*!< DWT CTRL: CYCCNTENA Mask */
#endif /* DWT_BASE */

typedef __IO uint32_t * const cuint32_ptr;
#define CYCCNT_ *(cuint32_ptr)(&DWT->CYCCNT)

class dwt_timer {
public:

    static void init() {
        if (!(CoreDebug->DEMCR & CoreDebug_DEMCR_TRCENA_Msk)) {
            CoreDebug->DEMCR |= CoreDebug_DEMCR_TRCENA_Msk;
            DWT->CTRL |= DWT_CTRL_CYCCNTENA_Msk;
        }
        CYCCNT_ = 0;
    }

    static inline __attribute__((always_inline))
    const uint32_t get(void) {
        return CYCCNT_;
    }

    static inline __attribute__((always_inline)) __attribute__((align(8)))
    void delay_cycles(const uint32_t cycles) {
        const uint32_t start = get();
        do {
        } while (get() - start < cycles);
    }
};

#define usec2cycles(n) ((n*(F_CPU/1000000))-12) /* cycle overhead for dwt polling loop overhead */

#endif /* DWT_TIMER_H_ */
