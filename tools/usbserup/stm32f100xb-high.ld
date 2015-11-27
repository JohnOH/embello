MEMORY
{
	rom (rx) : ORIGIN = 0x0801C000, LENGTH = 16K
	ram (rwx) : ORIGIN = 0x20000000, LENGTH = 8K
}

INCLUDE libopencm3_stm32f1.ld
