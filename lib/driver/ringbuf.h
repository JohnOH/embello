template < int SIZE >
class RingBuf {
  volatile uint8_t in, out, buf [SIZE];
public:
  RingBuf () : in (0), out (0) {}

  bool isEmpty () const { return in == out; }
  bool isFull () const { return (in + 1 - out) % SIZE == 0; }

  void put (uint8_t data) {
    if (in >= SIZE)
      in = 0;
    buf[in++] = data;
  }

  uint8_t get () {
    if (out >= SIZE)
      out = 0;
    return buf[out++];
  }
};
