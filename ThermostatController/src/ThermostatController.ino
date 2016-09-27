#include <Arduino.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>

#ifdef ESP8266
extern "C" {
#include "user_interface.h"
}
#endif

////////////////////////////////////////////////////////////////////////////////
// CONFIGURE THESE VALUES FOR YOUR SETUP
// for a basic setup you don't have to change anything else
//
// ZONE is the name under which the temperature sensor will send data
#define ZONE "Office";
// ISCONTROLLER indicates if it is connected to a heater
#define ISCONTROLLER true
// CHANNELS defines the channels for the heater
// Multiple Relays should be connected to D1, D2, D3 consecutively
// (ammend the code to support more zones- limited to 3 now)
// SSID - the name of the WiFi network
#define SSID "***"
// PASSWORD - the password for the WiFi network
#define PASSWORD "***"
// URLROOT - the root of the API address - with no '/' at the end!
#define URLROOT "http://***/thermostat"
// FREEZESAFE - the temperature under which the heater will be started
// with no controller from the server, the temperature is the one that 
// is read by this device and should be adjusted to what is deemed safe
#define FREEZESAFE 12.0
////////////////////////////////////////////////////////////////////////////////

// These are correction values for when using the Wemos D1 board with TMP35 sensor
#define D1MINIA0CORRECTION 3.3f
#define TEMPOFFSET (-1.35)

// When using a board without voltage divider the following values should be used:
//#define D1MINIA0CORRECTION 1.0f
//#define TEMPOFFSET 0


char channels[] = { CHANNELS };

void setup() {
  //this is required to read ADC values reliably
  wifi_set_sleep_type(NONE_SLEEP_T);
  
  Serial.begin(57600);
  pinMode(D1, OUTPUT);
  pinMode(D2, OUTPUT);
  pinMode(D3, OUTPUT);
  
  // delay is required only for debugging
  delay(2000);
  
  WiFi.mode(WIFI_STA);
}

void loop() {
  int retries = 0;
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("Not connected to the WiFi.");
    WiFi.begin(SSID, PASSWORD);
    Serial.println("after wifi begin");
    
    while ( retries < 30 ) {
      Serial.println("loop");
      if (WiFi.status() == WL_CONNECTED) {
        break;
      }
      delay(1000);
      retries++;
    }
    Serial.println("Exiting loop() for another wifi try.");
    return;
  }
  else {
    Serial.println("Connected to WIFI!");
  }
  
  //Serial.println(WiFi.localIP());
  float totalTemp = 0.0;
  for (int i = 0; i < 100; i++) {
    int analog = analogRead(A0);
    
    totalTemp += (analog * 100.0f / 1024.0f * D1MINIA0CORRECTION) + TEMPOFFSET;
    delay(2);
  }
  float temp = totalTemp / 100.0;
  Serial.println(temp);
  
  
  HTTPClient http;
  int httpCode = 0;
  
  //Build the payload
  String s = "{\"name\":\"";
  s += ZONE;
  s += "\",\"temperature\":";
  s += temp;
  s += "}";
  Serial.println(s);
  
  Serial.println(URLROOT"/api/temperature");
  http.begin(URLROOT"/api/temperature");
  http.setTimeout(30000);
  http.addHeader("Content-Type", "application/json");
  httpCode = http.POST(s);
  
  if (httpCode == HTTP_CODE_OK) {
    Serial.println("Successfully posted temperature.");
  } else {
    Serial.println("Unsuccessfully posted temperature.");
    Serial.println(httpCode);
  }
  
  HTTPClient http2;
  if (ISCONTROLLER) {
    for (uint8_t i = 0; i < sizeof(channels); i++) {
      String url = URLROOT"/api/";
      url += channels[i];
      Serial.println(url);
      http2.begin(url);
      http2.setTimeout(30000);
      httpCode = http2.GET();
      
      bool successfulCall = false;
      if (httpCode > 0) {
        if (httpCode == HTTP_CODE_OK) {
          String payload = http.getString();
          
          successfulCall = true;
          bool heating = payload == "ON";
          Serial.println(payload);
          digitalWrite(D(i), heating);
        }
      }
      
      if (!successfulCall) {
        //there was a problem accessing the API, stop the heating to prevent overheating
        digitalWrite(D(i), LOW);
        Serial.println("Unsuccessful call to the API!");
        Serial.println(httpCode);
      }
      
      // ensure the house does not freeze even if the server is not 
      // available to control the heating
      if (temp < FREEZESAFE) {
        Serial.println("Stop freeze!");
        digitalWrite(D(i), HIGH);
      }
    }
  }
  
  //every ~1 minute
  delay(59000);
}

// transforms the index of the channel to the digital pin to drive the Relay
// add more cases to support more channels
uint8_t D(uint8_t index) {
  switch (index) {
    case 0: return D1;
    case 1: return D2;
    case 2: return D3;
  }
}
   