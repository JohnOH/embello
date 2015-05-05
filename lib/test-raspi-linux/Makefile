CXXFLAGS += -I../arch-raspi -I../driver
LDLIBS = -lwiringPi -lwiringPiDev -lpthread

all: rf69spi

rf69spi: ../arch-raspi/spi.h ../driver/rf69.h

clean:
	rm -f *.o rf69spi
