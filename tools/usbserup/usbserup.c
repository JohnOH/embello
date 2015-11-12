/*
 * This file was adapted from the usb_cdcacm example in the libopencm3 project.
 * It use libopencm3 as is, the code below is released in the public domain.
 *
 * Copyright (C) 2010 Gareth McMullin <gareth@blacksphere.co.nz>
 * Copyright (C) 2015 Jean-Claude Wippler <jc@wippler.nl>
 */

#include <stdlib.h>
#include <libopencm3/stm32/rcc.h>
#include <libopencm3/stm32/gpio.h>
#include <libopencm3/stm32/flash.h>
#include <libopencm3/usb/usbd.h>
#include <libopencm3/usb/cdc.h>
#include <libopencm3/cm3/scb.h>

#include "upload.h"
#include "usbinfo.h"

static const char *usb_strings[] = {
	"JeeLabs",
	"USB Serial w/ Upload v0.1",
};

// Buffer to be used for control requests.
uint8_t usbd_control_buffer[128];

usbd_device *gusbd_dev;
char gbuf[64];
int gpos, glen;
uint8_t outBuf [64];
int outFill;

static int cdcacm_control_request (usbd_device *usbd_dev, struct usb_setup_data *req, uint8_t **buf,
		uint16_t *len, void (**complete)(usbd_device *usbd_dev, struct usb_setup_data *req)) {
	(void)complete;
	(void)buf;
	(void)usbd_dev;

	switch (req->bRequest) {
	case USB_CDC_REQ_SET_CONTROL_LINE_STATE: {
		/*
		 * This Linux cdc_acm driver requires this to be implemented
		 * even though it's optional in the CDC spec, and we don't
		 * advertise it in the ACM functional descriptor.
		 */
		char local_buf[10];
		struct usb_cdc_notification *notif = (void *)local_buf;

		/* We echo signals back to host as notification. */
		notif->bmRequestType = 0xA1;
		notif->bNotification = USB_CDC_NOTIFY_SERIAL_STATE;
		notif->wValue = 0;
		notif->wIndex = 0;
		notif->wLength = 2;
		local_buf[8] = req->wValue & 3;
		local_buf[9] = 0;
		// usbd_ep_write_packet(0x83, buf, 10);
		return 1;
		}
	case USB_CDC_REQ_SET_LINE_CODING:
		if (*len < sizeof(struct usb_cdc_line_coding))
			return 0;
		return 1;
	}
	return 0;
}

static void cdcacm_data_rx_cb (usbd_device *usbd_dev, uint8_t ep) {
	(void)ep;
	(void)usbd_dev;

    gpos = 0;
	glen = usbd_ep_read_packet(usbd_dev, 0x01, gbuf, sizeof gbuf);
}

static void cdcacm_set_config (usbd_device *usbd_dev, uint16_t wValue) {
	(void)wValue;
	(void)usbd_dev;

	usbd_ep_setup(usbd_dev, 0x01, USB_ENDPOINT_ATTR_BULK, 64, cdcacm_data_rx_cb);
	usbd_ep_setup(usbd_dev, 0x82, USB_ENDPOINT_ATTR_BULK, 64, NULL);
	usbd_ep_setup(usbd_dev, 0x83, USB_ENDPOINT_ATTR_INTERRUPT, 16, NULL);

	usbd_register_control_callback(
				usbd_dev,
				USB_REQ_TYPE_CLASS | USB_REQ_TYPE_INTERFACE,
				USB_REQ_TYPE_TYPE | USB_REQ_TYPE_RECIPIENT,
				cdcacm_control_request);
}

static void flush (void) {
    int i = 0;
    while (i < outFill)
        i += usbd_ep_write_packet(gusbd_dev, 0x82, outBuf + i, outFill - i);
    outFill = 0;
}

static uint8_t getByte (void) {
    flush();
    int i;
    for (i = 0; i < 100000000; ++i) {
        if (gpos < glen)
            return gbuf[gpos++];
        usbd_poll(gusbd_dev);
    }
    return 0xFF; // return with fake value after a certain amount of time
}

static void putByte (uint8_t b) {
	if (outFill >= (int) sizeof outBuf)
        flush();
	outBuf[outFill++] = b;
}

#if 0
const int pageBits = 10; // 1K pages on medium-density F103's
const int bootPages = 8; // first 8 KB of flash contains boot loader
const int totalPages = 128; // total flash memory size
const uint32_t flashStart = 0x08000000;  // start of flash memory

const int pageSize = 1 << pageBits;
const uint32_t userStart = flashStart + bootPages * pageSize;
const uint32_t userLimit = flashStart + totalPages * pageSize;
#else
#define pageBits 10
#define bootPages 8
#define totalPages 28
#define flashStart 0x08000000
#define pageSize (1 << pageBits)
#define userStart (flashStart + bootPages * pageSize)
#define userLimit (flashStart + totalPages * pageSize)
#endif

static void eraseFlash (void) {
    // don't mass erase, erase only all the user pages in a loop
    uint32_t addr;
    for (addr = userStart; addr < userLimit; addr += pageSize)
        flash_erase_page(addr);
    flash_wait_for_last_operation();
}

static void writeFlash (uint32_t addr, const uint8_t* ptr, int len) {
    if (addr < userStart)
        return; // don't overwrite the boot loader
    int i;
    for (i = 0; i < len; i += sizeof (uint32_t))
        flash_program_word(addr + i, *(const uint32_t*) (ptr + i));
    flash_wait_for_last_operation();
}

int main (void) {
	rcc_clock_setup_in_hse_8mhz_out_72mhz();
	rcc_periph_clock_enable(RCC_GPIOA);

	// Setup GPIOA Pin 0 to pull up the D+ high, so autodect works
	// with the bootloader.  The circuit is active low.
	gpio_set_mode(GPIOA, GPIO_MODE_OUTPUT_2_MHZ,
		      GPIO_CNF_OUTPUT_OPENDRAIN, GPIO0);
	gpio_clear(GPIOA, GPIO0);

	// Setup GPIOA Pin 1 for the LED, inverted logic
	//gpio_clear(GPIOA, GPIO1);
	//gpio_set_mode(GPIOA, GPIO_MODE_OUTPUT_2_MHZ,
	//	      GPIO_CNF_OUTPUT_PUSHPULL, GPIO1);

	gusbd_dev = usbd_init(&stm32f103_usb_driver, &dev, &config, usb_strings, 2, usbd_control_buffer, sizeof(usbd_control_buffer));
	usbd_register_set_config_callback(gusbd_dev, cdcacm_set_config);

	//gpio_set(GPIOA, GPIO1);

    if (initialSync()) {
        flash_unlock();
        while (uploadHandler())
            ;
        flash_lock();
        flush();
    }

	SCB_VTOR = userStart;
    uint32_t sp = ((const uint32_t*) userStart)[0];
    uint32_t ip = ((const uint32_t*) userStart)[1];
    
    // set up stack pointer, then jump to reset vector entry
    __asm volatile ("msr PSP, %0" : : "r" (sp));
    ((void (*)(void)) ip)();
}
