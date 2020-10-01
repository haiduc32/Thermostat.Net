#include <Arduino.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <Adafruit_AHTX0.h>



#ifdef ESP8266
extern "C" {
#include "user_interface.h"
}
#endif

extern "C" void esp_yield();

////////////////////////////////////////////////////////////////////////////////
// CONFIGURE THESE VALUES FOR YOUR SETUP
// for a basic setup you don't have to change anything else
//
// ZONE is the name under which the temperature sensor will send data
// IT IS CASE SENSITIVE, AND SHOULD BE SAME AS IN THE SERVER CONFIGURATION
#define ZONE "Office";
// ISCONTROLLER indicates if it is connected to a heater
#define ISCONTROLLER true
// CHANNELS defines the channels for the heater
// Multiple Relays should be connected to D1, D2, D3 consecutively
// (ammend the code to support more zones- limited to 3 now)
#define CHANNELS '0' //for multiple zones write: '0','1' or '0','2','5'
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
// Choose the used sensor. Uncomment only the temperature sensor that is used
//#define TMP35
#define AHT10
////////////////////////////////////////////////////////////////////////////////

// These are correction values for when using the Wemos D1 board with TMP35 sensor
//#define D1MINIA0CORRECTION 3.3f
// This is the Wemos D1 Mini hacked analog to bypass the 220k resisotr with TMP35 sensor
// it should be 0.0f, but this might be calibrated for each individual board as the resistors differ in absolute value
#define D1MINIA0CORRECTION 1.06f

// used or TMP35
#define NUMREADINGS 101


#ifdef AHT10
Adafruit_AHTX0 aht;
#endif

char channels[] = { CHANNELS };

void setup() {
  //this is required to read ADC values reliably
  wifi_set_sleep_type(NONE_SLEEP_T);
  
  Serial.begin(57600);
  pinMode(D5, OUTPUT);
  pinMode(D6, OUTPUT);

  // delay is required only for debugging
  delay(3000);

  #ifdef AHT10
  if (! aht.begin()) {
    Serial.println("Could not find AHT? Check wiring");
    while (1) delay(10);
  }
  Serial.println("AHT10 or AHT20 found");
  #endif
  

  
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
  // Reading the temperature
  
  #ifdef TMP35
  float temp = readTempTMP35();
  #endif
  #ifdef AHT10
  sensors_event_t h, t;
  aht.getEvent(&h, &t);
  float temp = t.temperature;
  float humidity = h.relative_humidity;
  #endif


  Serial.println(temp);
  
  
  HTTPClient http;
  int httpCode = 0;
  
  //Build the payload
  String s = "{\"name\":\"";
  s += ZONE;
  s += "\",\"temperature\":";
  s += temp;
  #ifdef AHT10
  s +=",\"humidity\":";
  s += humidity;
  #endif
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
          String payload = http2.getString();
          Serial.println(payload);
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
  if (!ISCONTROLLER) {
    ESP.deepSleep(59000000, WAKE_RF_DEFAULT);
  } else {
    delay(59000);
  }
  //esp_yield();
}

float readTempTMP35() {
  int tempValues[NUMREADINGS];
  for (int i = 0; i < NUMREADINGS; i++) {
    int analog = analogRead(A0);
    tempValues[i] = analog;
    delay(10);
  }
  
  //bubble sort
  int out, in, swapper;
  for(out=0 ; out < NUMREADINGS; out++) {  // outer loop
    for(in=out; in<(NUMREADINGS-1); in++)  {  // inner loop
      if( tempValues[in] > tempValues[in+1] ) {   // out of order?
        // swap them:
        swapper = tempValues[in];
        tempValues[in] = tempValues[in+1];
        tempValues[in+1] = swapper;
      }
    }
  }
  
  int medianValue = tempValues[NUMREADINGS/2];
  
  int count = 0;
  float sumOfTemps = 0.0f;
  for (int i = NUMREADINGS / 4; i < NUMREADINGS / 4 * 3; i++) {
    count++;
    sumOfTemps += (tempValues[i] * 100.0f / 1024.0f * D1MINIA0CORRECTION);
  } 
  float temp = sumOfTemps / count;

  return temp;
}

// transforms the index of the channel to the digital pin to drive the Relay
// add more cases to support more channels
uint8_t D(uint8_t index) {
  switch (index) {
    case 0: return D5;
    case 1: return D6;
  }
}
   