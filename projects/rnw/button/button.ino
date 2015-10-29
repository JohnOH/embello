// Control the on-board LED of the HY-TinySTM103 via a button.

const int LED = PA1;    // inverted logic
const int BUTTON = PB0; // inverted logic

void setup () {
    pinMode(LED, OUTPUT);
    pinMode(BUTTON, INPUT_PULLUP);
}

void loop () {
    digitalWrite(LED, digitalRead(BUTTON));
}
