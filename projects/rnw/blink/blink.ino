// Blink the on-board LED of the HY-TinySTM103.

const int LED = PA1; // inverted logic

void setup () {
    pinMode(LED, OUTPUT);
}

void loop () {
    digitalWrite(LED, LOW);     // on!
    delay(500);
    digitalWrite(LED, HIGH);    // off!
    delay(500);
}
