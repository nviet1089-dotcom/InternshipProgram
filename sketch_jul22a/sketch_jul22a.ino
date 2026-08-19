#include <DHT.h>

#define DHTPIN 2        // Cắm chân OUT/DATA của DHT vào chân D2
#define DHTTYPE DHT11   // Đổi thành DHT22 nếu bạn dùng DHT22

DHT dht(DHTPIN, DHTTYPE);

void setup() {
  Serial.begin(9600);
  dht.begin();
}

void loop() {
  // Cảm biến DHT11 cần nghỉ 2 giây giữa mỗi lần đọc
  delay(2000); 

  float h = dht.readHumidity();
  float t = dht.readTemperature();

  // Nếu không đọc được từ cảm biến thì thông báo qua Serial để debug
  if (isnan(h) || isnan(t)) {
    Serial.println("Loi: Khong doc duoc tu cam bien DHT!");
    return;
  }

  // Gửi đúng định dạng cho App WPF nhận diện
  Serial.print("$T:");
  Serial.print(t, 1);
  Serial.print("|H:");
  Serial.print(h, 1);
  Serial.println("#");
}