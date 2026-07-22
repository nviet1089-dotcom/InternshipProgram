
#include <DHT.h>

#define DHTPIN 2       
#define DHTTYPE DHT11  

DHT dht(DHTPIN, DHTTYPE);

void setup() {
  Serial.begin(9600);
  dht.begin();
}

void loop() {
  delay(2000);

  float h = dht.readHumidity();
  float t = dht.readTemperature();

 
  if (isnan(h) || isnan(t)) {
    return;
  }

  Serial.print("T:");
  Serial.print(t, 2);
  Serial.print("|H:");
  Serial.print(h, 2);
  Serial.println();
}