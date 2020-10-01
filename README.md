# Thermostat.Net
A thermostat system for your home built with ESP8266, Arduino and .Net

## What you'll need
### Hardware
Some ESP8266 powered boards, if you're not proficient with ESP8266 then you should use Wemos D1 Mini. Some TMP35 sensors, one or more relays, again, if you're not proficient with it go for Wemos Relay for D1 Mini. Some Micro USB chargers (probably you've got plenty of those laying arround). That's all the hardware you will need (except the next point..). 

An always-on pc that you can use as server.

### Software
For building the Arduino to flash the ESP8266 boards you'll need Atom with Platform IO plugin.

For .net you got plenty of options, I suggest you go for the Visual Studio Community Edition (unless you got a better edition).

Windows installed on the PC and .Net Core installed.

### Raspberry PI
It's not possible to run the server side on Raspberry PI just yet.. Thought the code is inteded to be run on it as soon as .Net Core is supported. That is supposed to happen sometimes in early 2017, so fingers crossed!

### SSL Certificate
Interacting with Alexa requires an SSL certificate on your server. How to get this working on windows with Let's Encrypt: https://weblog.west-wind.com/posts/2016/feb/22/using-lets-encrypt-with-iis-on-windows#TheEasyWay:LetsEncrypt-Win-Simple

### How it works

TODO: this part is in work, more parts to come
