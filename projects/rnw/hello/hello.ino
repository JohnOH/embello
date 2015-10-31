// Periodically send the timer value to the HY-TinySTM103 serial port.

void setup () {
    Serial.begin(115200);
    Serial.print("[hello]");
}

void loop () {
    Serial.println(millis());
    delay(1000);
}
