# build the .bin file corresponding to each .hex file
# the list is constructed from all the "f-*.fs" source files

SRCS = $(wildcard f-*.fs)
BINS = $(SRCS:f-%.fs=usb-%.bin)

all: $(BINS)

%.bin: %.hex
	srec_cat -Output $@ -Binary $< -Intel

clean:
	rm -f $(BINS)
