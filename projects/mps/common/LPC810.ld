MEMORY
{
  flash : org = 0x00000000, len = 4k
  ram : org = 0x10000000, len = 1k
}

ENTRY(ResetISR)

SECTIONS
{
    .text :
    {
        KEEP(*(.isr_vector))
        *(.after_vectors*)
        *(.text*)
        *(.rodata .rodata.*)
        . = ALIGN(4);

        PROVIDE(__init_array_start = .);
        KEEP(*(SORT(.init_array.*)))
        KEEP(*(.init_array))
        PROVIDE(__init_array_end = .);

        . = ALIGN(4);
        _etext = .;
    } > flash

    .data :
    {
        _data = .;
        *(vtable)
        *(.data*)
        . = ALIGN(4) ;
        _edata = .;
    } > ram AT > flash

    .bss :
    {
        _bss = .;
        *(.bss*)
        *(COMMON)
        . = ALIGN(4) ;
        _ebss = .;
        PROVIDE(end = .);
    } > ram

    PROVIDE(_pvHeapStart = .);
    PROVIDE(_vStackTop = ORIGIN(ram) + LENGTH(ram));
}
