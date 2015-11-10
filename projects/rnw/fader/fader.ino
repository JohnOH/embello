// Fade the on-board LED of the HY-TinySTM103 in and out.

const int LED = PA1; // inverted logic

void setup () {
    pinMode(LED, OUTPUT);
}

void loop () {
    // 255 is fully off, 0 is maximally on
    for (int i = 255; i >= 0; --i) {
        analogWrite(LED, i);
        delay(5);
    }
    for (int i = 0; i <= 255; ++i) {
        analogWrite(LED, i);
        delay(5);
    }
}
