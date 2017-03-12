/*
 * This code is derived from example code in the libopencm3 project:
 *
 *  https://github.com/libopencm3/libopencm3-examples/tree/master/
 *          examples/stm32/f1/stm32-h103/usart_irq_printf
 *  and     examples/stm32/f1/stm32-h103/usb_cdcacm
 *
 * Copyright (C) 2009 Uwe Hermann <uwe@hermann-uwe.de>,
 * Copyright (C) 2010, 2013 Gareth McMullin <gareth@blacksphere.co.nz>
 * Copyright (C) 2011 Piotr Esden-Tempski <piotr@esden.net>
 * Copyright (C) 2016 Jean-Claude Wippler <jc@wippler.nl>
 *
 * This code is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This code is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this code.  If not, see <http://www.gnu.org/licenses/>.
 */

#include <stdlib.h>
#include <libopencm3/stm32/rcc.h>
#include <libopencm3/stm32/gpio.h>
#include <libopencm3/stm32/usart.h>
#include <libopencm3/cm3/nvic.h>
#include <libopencm3/cm3/systick.h>
#include <libopencm3/usb/usbd.h>
#include <libopencm3/usb/cdc.h>

// HyTiny
#define GPIO_USB1   GPIOA
#define PIN_USB1    GPIO0
#define GPIO_LED1   GPIOA
#define PIN_LED1    GPIO1

// BluePill
#define GPIO_USB2   GPIOA
#define PIN_USB2    GPIO12
#define GPIO_LED2   GPIOC
#define PIN_LED2    GPIO13

#define GPIO_RTS    GPIOA
#define GPIO_DTR    GPIOA
// HyTiny
#define PIN_RTS1    GPIO13
#define PIN_DTR1    GPIO14
// Blue Pill
#define PIN_RTS2    GPIO2
#define PIN_DTR2    GPIO3

uint32_t gpio_led, pin_led, gpio_usb, pin_usb, pin_rts, pin_dtr;

/******************************************************************************
 * Simple ringbuffer implementation from open-bldc's libgovernor that
 * you can find at:
 * https://github.com/open-bldc/open-bldc/tree/master/source/libgovernor
 *****************************************************************************/

typedef int32_t ring_size_t;

struct ring {
    uint8_t *data;
    ring_size_t size;
    uint32_t begin;
    uint32_t end;
};

#define RING_SIZE(RING)  ((RING)->size - 1)
#define RING_DATA(RING)  (RING)->data
#define RING_EMPTY(RING) ((RING)->begin == (RING)->end)

static void ring_init(struct ring *ring, uint8_t *buf, ring_size_t size)
{
    ring->data = buf;
    ring->size = size;
    ring->begin = 0;
    ring->end = 0;
}

static int32_t ring_write_ch(struct ring *ring, uint8_t ch)
{
    if (((ring->end + 1) % ring->size) != ring->begin) {
        ring->data[ring->end++] = ch;
        ring->end %= ring->size;
        return (uint32_t)ch;
    }

    return -1;
}

static int32_t ring_write(struct ring *ring, uint8_t *data, ring_size_t size)
{
    int32_t i;

    for (i = 0; i < size; i++) {
        if (ring_write_ch(ring, data[i]) < 0)
            return -i;
    }

    return i;
}

static int32_t ring_read_ch(struct ring *ring, uint8_t *ch)
{
    int32_t ret = -1;

    if (ring->begin != ring->end) {
        ret = ring->data[ring->begin++];
        ring->begin %= ring->size;
        if (ch)
            *ch = ret;
    }

    return ret;
}

static int32_t ring_read(struct ring *ring, uint8_t *data, ring_size_t size)
{
    int32_t i;

    for (i = 0; i < size; i++) {
        if (ring_read_ch(ring, data + i) < 0)
            return i;
    }

    return -i;
}

/*****************************************************************************/

#define BUFFER_SIZE 256

struct ring input_ring, output_ring;
uint8_t input_ring_buffer[BUFFER_SIZE], output_ring_buffer[BUFFER_SIZE];
volatile uint32_t ticks;

static void clock_setup(void)
{
    rcc_clock_setup_in_hse_8mhz_out_72mhz();

    rcc_periph_clock_enable(RCC_GPIOA);
    rcc_periph_clock_enable(RCC_GPIOC);
    rcc_periph_clock_enable(RCC_AFIO);
    rcc_periph_clock_enable(RCC_USART1);
}

static void gpio_setup(void)
{
    // several pin choices depend on the actual board this is running on

    if (*(const uint16_t*) 0x1FFFF7E0 == 128) { // only HyTiny has 128K flash
        gpio_led = GPIO_LED1;
        pin_led = PIN_LED1;
        gpio_usb = GPIO_USB1;
        pin_usb = PIN_USB1;
        pin_rts = PIN_RTS1;
        pin_dtr = PIN_DTR1;
    } else {
        gpio_led = GPIO_LED2;
        pin_led = PIN_LED2;
        gpio_usb = GPIO_USB2;
        pin_usb = PIN_USB2;
        pin_rts = PIN_RTS2;
        pin_dtr = PIN_DTR2;
    }

    gpio_set(gpio_led, pin_led);
    gpio_set_mode(gpio_led, GPIO_MODE_OUTPUT_2_MHZ,
            GPIO_CNF_OUTPUT_PUSHPULL, pin_led);

    // careful, PA1 uses negative logic, PA12 uses positive logic!
    (pin_usb == PIN_USB1 ? gpio_set : gpio_clear)(gpio_usb, pin_usb);

    gpio_set_mode(gpio_usb, GPIO_MODE_OUTPUT_2_MHZ,
            GPIO_CNF_OUTPUT_PUSHPULL, pin_usb);

    /* start with DTR and RTS in their default state: DTR high, RTS low */
    gpio_set(GPIO_DTR, pin_dtr);
    gpio_set_mode(GPIO_DTR, GPIO_MODE_OUTPUT_2_MHZ,
            GPIO_CNF_OUTPUT_OPENDRAIN, pin_dtr);
    gpio_clear(GPIO_RTS, pin_rts);
    gpio_set_mode(GPIO_RTS, GPIO_MODE_OUTPUT_2_MHZ,
            GPIO_CNF_OUTPUT_PUSHPULL, pin_rts);
}

static void usart_setup(void)
{
    /* Initialize input and output ring buffers. */
    ring_init(&input_ring, input_ring_buffer, BUFFER_SIZE);
    ring_init(&output_ring, output_ring_buffer, BUFFER_SIZE);

    /* Enable the USART1 interrupt. */
    nvic_enable_irq(NVIC_USART1_IRQ);

    /* Setup GPIO pin GPIO_USART1_RE_TX on GPIO port A for transmit. */
    gpio_set_mode(GPIOA, GPIO_MODE_OUTPUT_50_MHZ,
            GPIO_CNF_OUTPUT_ALTFN_PUSHPULL, GPIO_USART1_TX);

    /* Setup GPIO pin GPIO_USART1_RE_RX on GPIO port A for receive. */
    gpio_set(GPIOA, GPIO_USART1_RX); /* weak pull-up avoids picking up noise */
    gpio_set_mode(GPIOA, GPIO_MODE_INPUT,
            GPIO_CNF_INPUT_PULL_UPDOWN, GPIO_USART1_RX);

    /* Setup UART parameters. */
    usart_set_baudrate(USART1, 115200);
    usart_set_databits(USART1, 8);
    usart_set_stopbits(USART1, USART_STOPBITS_1);
    usart_set_parity(USART1, USART_PARITY_NONE);
    usart_set_flow_control(USART1, USART_FLOWCONTROL_NONE);
    usart_set_mode(USART1, USART_MODE_TX_RX);

    /* Enable USART1 Receive interrupt. */
    USART_CR1(USART1) |= USART_CR1_RXNEIE;

    /* Finally enable the USART. */
    usart_enable(USART1);
}

/* telnet escape codes and special values: */
enum {
    IAC=255, WILL=251, SB=250, SE=240,
    CPO=44, SETPAR=3, SETCTL=5,
    PAR_NONE=1, PAR_ODD=2, PAR_EVEN=3,
    DTR_ON=8, DTR_OFF=9, RTS_ON=11, RTS_OFF=12,
};

void usart1_isr(void)
{
    /* Check if we were called because of RXNE. */
    if (((USART_CR1(USART1) & USART_CR1_RXNEIE) != 0) &&
            ((USART_SR(USART1) & USART_SR_RXNE) != 0)) {

        /* Indicate that we got data. */
        gpio_toggle(gpio_led, pin_led);

        /* Retrieve the data from the peripheral. */
        uint8_t c = usart_recv(USART1);
        ring_write_ch(&input_ring, c);

        /* telnet: escape the escape character, i.e. send it twice */
        if (c == IAC)
            ring_write_ch(&input_ring, c);
    }

    /* Check if we were called because of TXE. */
    if (((USART_CR1(USART1) & USART_CR1_TXEIE) != 0) &&
            ((USART_SR(USART1) & USART_SR_TXE) != 0)) {

        int32_t data = ring_read_ch(&output_ring, NULL);

        if (data == -1) {
            /* Disable the TXE interrupt, it's no longer needed. */
            USART_CR1(USART1) &= ~USART_CR1_TXEIE;
        } else {
            /* state machine to decode telnet request before sending it on */
            static int state = 0;

            switch (state) {
                default: // default state
                    if (data == IAC)
                        state = 1;
                    else
                        usart_send(USART1, data);
                    break;

                case 1: // IAC seen
                    state = 0;
                    if (data == IAC)
                        usart_send(USART1, data);
                    else
                        state = data == SB ? 3 : data >= WILL ? 2 : 0;
                    break;

                case 2: // IAC, WILL (or WONT/DO/DONT) seen
                    state = 0;
                    break;

                case 3: // IAC, SB seen
                    state = data == CPO ? 4 : 5;
                    break;

                case 4: // IAC, SB, CPO seen
                    state = data == SETPAR ? 7 :
                        data == SETCTL ? 8 : 5;
                    break;

                case 5: // wait for IAC + SE
                    if (data == IAC)
                        state = 6;
                    break;

                case 6: // wait for SE
                    if (data != IAC)
                        state = data == SE ? 0 : data == SB ? 3 : 5;
                    break;

                case 7: // set parity
                    state = 5;
                    switch (data) {
                        case PAR_NONE:
                            usart_set_databits(USART1, 8);
                            usart_set_parity(USART1, USART_PARITY_NONE); break;
                        case PAR_ODD:
                            usart_set_databits(USART1, 9);
                            usart_set_parity(USART1, USART_PARITY_ODD); break;
                        case PAR_EVEN:
                            usart_set_databits(USART1, 9);
                            usart_set_parity(USART1, USART_PARITY_EVEN); break;
                            break;
                    }
                    break;

                case 8: // set control
                    state = 5;
                    switch (data) {
                        case DTR_ON:
                            gpio_clear(GPIO_DTR, pin_dtr);
                            break;
                        case DTR_OFF:
                            gpio_set(GPIO_DTR, pin_dtr);
                            break;
                        case RTS_ON:
                            gpio_clear(GPIO_RTS, pin_rts);
                            break;
                        case RTS_OFF:
                            gpio_set(GPIO_RTS, pin_rts);
                            break;
                    }
                    break;
            }
        }
    }
}

static void systick_setup(void)
{
    /* 72MHz / 8 => 9000000 counts per second. */
    systick_set_clocksource(STK_CSR_CLKSOURCE_AHB_DIV8);

    /* 9000000/9000 = 1000 overflows per second - every 1ms one interrupt */
    /* SysTick interrupt every N clock pulses: set reload to N-1 */
    systick_set_reload(8999);

    systick_interrupt_enable();

    /* Start counting. */
    systick_counter_enable();
}

void sys_tick_handler(void)
{
    ++ticks;
}

/*****************************************************************************/

static const struct usb_device_descriptor dev = {
    .bLength = USB_DT_DEVICE_SIZE,
    .bDescriptorType = USB_DT_DEVICE,
    .bcdUSB = 0x0200,
    .bDeviceClass = USB_CLASS_CDC,
    .bDeviceSubClass = 0,
    .bDeviceProtocol = 0,
    .bMaxPacketSize0 = 64,
    .idVendor = 0x0483,
    .idProduct = 0x5740,
    .bcdDevice = 0x0200,
    .iManufacturer = 1,
    .iProduct = 2,
    .iSerialNumber = 3,
    .bNumConfigurations = 1,
};

/*
 * This notification endpoint isn't implemented. According to CDC spec its
 * optional, but its absence causes a NULL pointer dereference in Linux
 * cdc_acm driver.
 */
static const struct usb_endpoint_descriptor comm_endp[] = {{
    .bLength = USB_DT_ENDPOINT_SIZE,
        .bDescriptorType = USB_DT_ENDPOINT,
        .bEndpointAddress = 0x83,
        .bmAttributes = USB_ENDPOINT_ATTR_INTERRUPT,
        .wMaxPacketSize = 16,
        .bInterval = 255,
}};

static const struct usb_endpoint_descriptor data_endp[] = {{
    .bLength = USB_DT_ENDPOINT_SIZE,
        .bDescriptorType = USB_DT_ENDPOINT,
        .bEndpointAddress = 0x01,
        .bmAttributes = USB_ENDPOINT_ATTR_BULK,
        .wMaxPacketSize = 64,
        .bInterval = 1,
}, {
    .bLength = USB_DT_ENDPOINT_SIZE,
        .bDescriptorType = USB_DT_ENDPOINT,
        .bEndpointAddress = 0x82,
        .bmAttributes = USB_ENDPOINT_ATTR_BULK,
        .wMaxPacketSize = 64,
        .bInterval = 1,
}};

static const struct {
    struct usb_cdc_header_descriptor header;
    struct usb_cdc_call_management_descriptor call_mgmt;
    struct usb_cdc_acm_descriptor acm;
    struct usb_cdc_union_descriptor cdc_union;
} __attribute__((packed)) cdcacm_functional_descriptors = {
    .header = {
        .bFunctionLength = sizeof(struct usb_cdc_header_descriptor),
        .bDescriptorType = CS_INTERFACE,
        .bDescriptorSubtype = USB_CDC_TYPE_HEADER,
        .bcdCDC = 0x0110,
    },
    .call_mgmt = {
        .bFunctionLength =
            sizeof(struct usb_cdc_call_management_descriptor),
        .bDescriptorType = CS_INTERFACE,
        .bDescriptorSubtype = USB_CDC_TYPE_CALL_MANAGEMENT,
        .bmCapabilities = 0,
        .bDataInterface = 1,
    },
    .acm = {
        .bFunctionLength = sizeof(struct usb_cdc_acm_descriptor),
        .bDescriptorType = CS_INTERFACE,
        .bDescriptorSubtype = USB_CDC_TYPE_ACM,
        .bmCapabilities = 0,
    },
    .cdc_union = {
        .bFunctionLength = sizeof(struct usb_cdc_union_descriptor),
        .bDescriptorType = CS_INTERFACE,
        .bDescriptorSubtype = USB_CDC_TYPE_UNION,
        .bControlInterface = 0,
        .bSubordinateInterface0 = 1,
    },
};

static const struct usb_interface_descriptor comm_iface[] = {{
    .bLength = USB_DT_INTERFACE_SIZE,
        .bDescriptorType = USB_DT_INTERFACE,
        .bInterfaceNumber = 0,
        .bAlternateSetting = 0,
        .bNumEndpoints = 1,
        .bInterfaceClass = USB_CLASS_CDC,
        .bInterfaceSubClass = USB_CDC_SUBCLASS_ACM,
        .bInterfaceProtocol = USB_CDC_PROTOCOL_AT,
        .iInterface = 0,

        .endpoint = comm_endp,

        .extra = &cdcacm_functional_descriptors,
        .extralen = sizeof(cdcacm_functional_descriptors),
}};

static const struct usb_interface_descriptor data_iface[] = {{
    .bLength = USB_DT_INTERFACE_SIZE,
        .bDescriptorType = USB_DT_INTERFACE,
        .bInterfaceNumber = 1,
        .bAlternateSetting = 0,
        .bNumEndpoints = 2,
        .bInterfaceClass = USB_CLASS_DATA,
        .bInterfaceSubClass = 0,
        .bInterfaceProtocol = 0,
        .iInterface = 0,

        .endpoint = data_endp,
}};

static const struct usb_interface ifaces[] = {{
    .num_altsetting = 1,
        .altsetting = comm_iface,
}, {
    .num_altsetting = 1,
        .altsetting = data_iface,
}};

static const struct usb_config_descriptor config = {
    .bLength = USB_DT_CONFIGURATION_SIZE,
    .bDescriptorType = USB_DT_CONFIGURATION,
    .wTotalLength = 0,
    .bNumInterfaces = 2,
    .bConfigurationValue = 1,
    .iConfiguration = 0,
    .bmAttributes = 0x80,
    .bMaxPower = 0x32,

    .interface = ifaces,
};

static char serial_no[9];

static const char *usb_strings[] = {
    "JeeLabs",
    "SerPlus",
    serial_no,
};

/* Buffer to be used for control requests. */
uint8_t usbd_control_buffer[128];

static int cdcacm_control_request(usbd_device *usbd_dev, struct usb_setup_data *req, uint8_t **buf,
        uint16_t *len, void (**complete)(usbd_device *usbd_dev, struct usb_setup_data *req))
{
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

static void cdcacm_data_rx_cb(usbd_device *usbd_dev, uint8_t ep)
{
    (void)ep;
    (void)usbd_dev;

    // back pressure: don't read the packet if there's not enough room in ring
    if ((output_ring.begin - (output_ring.end+1)) % BUFFER_SIZE <= 64)
        return;

    uint8_t buf[64];
    int len = usbd_ep_read_packet(usbd_dev, 0x01, buf, sizeof buf);

    if (len) {
        /* Retrieve the data from the peripheral. */
        ring_write(&output_ring, buf, len);

        /* Enable usart transmit interrupt so it sends out the data. */
        USART_CR1(USART1) |= USART_CR1_TXEIE;
    }
}

static void cdcacm_set_config(usbd_device *usbd_dev, uint16_t wValue)
{
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

static char *get_dev_unique_id(char *s)
{
#if defined(STM32F4) || defined(STM32F2)
#	define UNIQUE_SERIAL_R 0x1FFF7A10
#	define FLASH_SIZE_R    0x1fff7A22
#elif defined(STM32F3)
#	define UNIQUE_SERIAL_R 0x1FFFF7AC
#	define FLASH_SIZE_R    0x1fff77cc
#elif defined(STM32L1)
#	define UNIQUE_SERIAL_R 0x1ff80050
#	define FLASH_SIZE_R    0x1FF8004C
#else
#	define UNIQUE_SERIAL_R 0x1FFFF7E8;
#	define FLASH_SIZE_R    0x1ffff7e0
#endif
	volatile uint32_t *unique_id_p = (volatile uint32_t *)UNIQUE_SERIAL_R;
	uint32_t unique_id = *unique_id_p ^  // was "+" in original BMP
			*(unique_id_p + 1) ^ // was "+" in original BMP
			*(unique_id_p + 2);
	int i;

	/* Calculated the upper flash limit from the exported data
	   in theparameter block*/
	//max_address = (*(uint32_t *) FLASH_SIZE_R) <<10;
	/* Fetch serial number from chip's unique ID */
	for(i = 0; i < 8; i++) {
		s[7-i] = ((unique_id >> (4*i)) & 0xF) + '0';
	}
	for(i = 0; i < 8; i++)
		if(s[i] > '9')
			s[i] += 'A' - '9' - 1;
	s[8] = 0;

	return s;
}

int main(void)
{
    clock_setup();

    // disable the SWD pins, since they are being re-used for DTR & RTS
    AFIO_MAPR = (AFIO_MAPR & ~(7<<24)) | (4<<24);

    gpio_setup();
    systick_setup();

    for (int i = 0; i < 10000000; i++)
        __asm__("");
    gpio_toggle(gpio_usb, pin_usb);

    get_dev_unique_id(serial_no);

    usbd_device *usbd_dev = usbd_init(&st_usbfs_v1_usb_driver, &dev, &config,
            usb_strings, 3, usbd_control_buffer, sizeof(usbd_control_buffer));
    usbd_register_set_config_callback(usbd_dev, cdcacm_set_config);

    for (int i = 0; i < 10000000; i++)
        __asm__("");

    usart_setup(); // late config to allow USB setup to complete first

    while (1) {
        // poll USB while waiting for 2 ms to elapse
        // it takes 2.7 ms to send 64 bytes at 230400 baud 8N1
        for (int i = 0; i < 2; ++i) {
            uint32_t lastTick = ticks;
            while (ticks == lastTick)
                usbd_poll(usbd_dev);
        }

        // put up to 64 pending bytes into the USB send packet buffer
        uint8_t buf[64];
        int len = ring_read(&input_ring, buf, sizeof buf);
        if (len > 0) {
            usbd_ep_write_packet(usbd_dev, 0x82, buf, len);
            //buf[len] = 0;
        }
    }
}
