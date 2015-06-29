template< int N, int S =0 >
class SpiDev {
public:
    struct Chunk {
        int count;
        const uint8_t* out;
        uint8_t* in;
        Chunk* next;
    };

    static bool master (int mhz) {
        wiringPiSetup();
        if (wiringPiSPISetup (N, mhz * 1000000) < 0) {
            printf("Can't open the SPI bus: %d\n", errno);
            return false;
        }
        return true;
    }

    // static void enable () {}
    // static void disable () {}
    // static uint8_t transfer (uint8_t val) {}
    
    static void pseudoDma (const Chunk* desc, int num) {
        const Chunk* descLimit = desc + num;

        // calculate transfer size
        int totalLength = 0;
        for (const Chunk* p = desc; p < descLimit; ++p)
            totalLength += p->count;
        if (totalLength <= 0)
            return;
        printf("num %d tl %d\n", num, totalLength);

        // fill buffer with outgoing data
        uint8_t* buf = (uint8_t*) malloc(totalLength);
        uint8_t* ofill = buf;
        for (const Chunk* p = desc; p < descLimit; ++p) {
            if (p->out != 0)
                memcpy(ofill, p->out, p->count);
            else
                memset(ofill, 0, p->count);
            ofill += p->count;
        }

        // perform the SPI transfer
        if (wiringPiSPIDataRW (N, buf, totalLength) == -1)
            printf("SPI error\n");
        else {
            // copy incoming data back to the chunks
            uint8_t* ifill = buf;
            for (const Chunk* p = desc; p < descLimit; ++p) {
                if (p->in != 0)
                    memcpy(p->in, ifill, p->count);
                ifill += p->count;
            }
        }

        free(buf);
    }

    static uint8_t rwReg (uint8_t cmd, uint8_t val) {
        uint8_t data[2] = { cmd, val };
#if 1
        if (wiringPiSPIDataRW (N, data, 2) == -1) {
            printf("SPI error\n");
            return 0;
        }
#else
        Chunk xfer = { 2, data, data };
        pseudoDma(&xfer, 1);
#endif
        return data[1];
    }

    //static LPC_SPI_T* addr () { return N == 0 ? LPC_SPI0 : LPC_SPI1; }
};

typedef SpiDev<0> SpiDev0;
typedef SpiDev<1> SpiDev1;
