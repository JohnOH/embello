void set_system_clock(void)
{
	rcc_clock_setup_hsi(&hsi_8mhz[CLOCK_64MHZ]);
}

void setup_led_pins(void)
{
	rcc_periph_clock_enable(RCC_GPIOE);
	gpio_mode_setup(GPIOE, GPIO_MODE_OUTPUT, GPIO_PUPD_NONE, GPIO8 | GPIO9 | GPIO10 | GPIO11 |
																GPIO12 | GPIO13 | GPIO14 | GPIO15);
	gpio_set(GPIOE, GPIO8 | GPIO9 | GPIO10 | GPIO11 |
					GPIO12 | GPIO13 | GPIO14 | GPIO15);
}

int main()
{
	_disable_interrupts();
	set_system_clock();
	setup_led_pins();
	rcc_periph_clock_enable(RCC_GPIOD);
	gpio_mode_setup(GPIOD, GPIO_MODE_INPUT, GPIO_PUPD_NONE, GPIO5);

	exti_select_source(EXTI5, GPIOD);
	exti_set_trigger(EXTI5, EXTI_TRIGGER_RISING);
	exti_enable_request(EXTI5);

	while(1)
	{
		if(exti_get_flag_status(EXTI5))
		{
			exti_reset_request(EXTI5);
			gpio_toggle(GPIOE, GPIO8);
		}
	}
}