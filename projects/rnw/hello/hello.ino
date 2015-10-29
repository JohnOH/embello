// Send a periodic greeting over the HY-TinySTM103 serial port.

void setup () {
    Serial.begin(115200);
}

void loop () {
    Serial.print("hello ");
    Serial.println(millis());
    delay(1000);
}
