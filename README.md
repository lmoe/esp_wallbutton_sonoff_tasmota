# Wallbutton Listener

I'm currently building up a smart home with devices like WiFi power plugs and Wifi light bulbs.

All of these devices were flashed with Sonoff-Tasmota and are connected to a local Mqtt server.

The lightbulbs obviously have to be powered on all the time to be controllable via app.

Problems arise when I want to turn them off without an app. 

There are certainly smart home wall buttons that can do this stuff, but most of them have to be built into the wall and require a full 230v power source. As I'm living in a rented apartment I don't want to replace every usual wall switch with a smart wall switch.

So I came up with this idea:

* Get a few ESP-01 8266 as it has WiFi
* Get a proper power source (1A, 3.7V LiFo battery)
* Print a nice wall power case
* Stick them on with double tape

## Problems

My main goal was to save power. I don't want to charge the battery every month. 

The ESP 8266 has a deep sleep mode, that only uses 15-20µ amps. 

So the first idea was to connect to WiFi, send a Mqtt message, toggle the light and turn off the chip.

After googleing around I've found that it usually takes a bit of time to reconnect to WiFi. Also the power consumption would be too high as the handshake and the WiFi protocol takes time.

I came up with this solution:

## Solution

Instead of connecting to WiFi I kind of abuse the ESP NOW functionality.

It is a connectionless protocol over the 2.4 ghz WiFi standard and allows faster and cheaper WiFi communication between ESPs.

Instead of getting a second ESP to receive the message (which has to redirect the request), I've thought a bit further. And came to the conclusion that a WiFi adapter in Monitor Mode directly on my server gets the job done.

So I've programmed the ESP as follows:

* Boot up
* Power on the WiFi antenna
* Send a BROADCAST to FF:FF:FF:FF:FF:FF with the message [6,6,6,1,2,3]
* Deep Sleep

A button hooked on RESET and GND can reset the ESP chip and restart the process. This is basically the whole wall button.

The other part is a simple .NET Core application that uses PCAP (Currently only programmed for Linux).

It monitors the WiFi signals in range and sets up a connection to the Mqtt server.

I've tried to use Node.js and the pcap package, sadly it was too slow. 

The server application consists of two modes:

* Discovery
* Listen

### Discovery 

The discovery mode monitors the whole WiFi network in near range and filters for broadcast packages.

If a package was found, it inspects the payload and looks for the [6,6,6,1,2,3] message at the end of the payload.

It prints the mac address of the ESP and that's it.

### Listen

The listen mode also monitors the whole WiFi network, however it sets up a stronger filter consisting of all mac addresses set in the settings.json. I've uploaded a test settings.json. It should be self explainable.

These mac addresses have a mapping there, which has two more properties:

* Topic
* Message

If an ESP was detected the listener looks up the mapping, if it was found it sends a message to the Mqtt server with the Topic and the Message (Payload).

Sonoff Tasmota does the rest.

The application should be easily expandable for a broad wide of use cases.
