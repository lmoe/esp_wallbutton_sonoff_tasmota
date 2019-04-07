#include <ESP8266WiFi.h>

extern "C"
{
#include <espnow.h>
}

#define WIFI_CHANNEL 1
#define SERIAL_DEBUG true

byte broadcastDevice[6] = {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
byte switchMessage[6] = {6, 6, 6, 1, 2, 3};

void setup()
{

  if (SERIAL_DEBUG)
  {
    Serial.begin(115200);
    Serial.println("Bootin up");
  }

  WiFi.mode(WIFI_STA);
  WiFi.begin();

  if (SERIAL_DEBUG)
  {
    Serial.println(WiFi.macAddress());
  }

  sendSwitchImpulse();

  if (SERIAL_DEBUG)
  {
    Serial.println("n8");
  }

  ESP.deepSleep(0);
  delay(100);
}

void sendSwitchImpulse()
{
  if (esp_now_init() != 0)
  {
    if (SERIAL_DEBUG)
    {
      Serial.println("ESP Now init failed. :(");
    }

    ESP.restart();
  }

  esp_now_set_self_role(ESP_NOW_ROLE_CONTROLLER);
  esp_now_add_peer(NULL, ESP_NOW_ROLE_CONTROLLER, WIFI_CHANNEL, NULL, 0);
  esp_now_send(broadcastDevice, switchMessage, sizeof(switchMessage));
}

void loop() {}
