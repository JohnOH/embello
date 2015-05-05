CXXFLAGS += -I../arch-raspi -I../driver
LDLIBS = -lwiringPi -lwiringPiDev -lpthread

APPS = rf69spi

all: rf69spi

rf69spi.cpp: ../arch-raspi/spi.h ../driver/rf69.h

clean:
	rm -f *.o rf69spi
