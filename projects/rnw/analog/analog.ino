// Measure and report analog voltage on PA3 to the HY-TinySTM103 serial port.

void setup () {
    Serial.begin(115200);
    Serial.println("[analog]");
}

void loop () {
    Serial.println(analogRead(PA3));
    delay(1000);
}
