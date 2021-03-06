﻿/*
  MIDI note player

  This sketch shows how to use the serial transmit pin (pin 1) to send MIDI note data.
  If this circuit is connected to a MIDI synth, it will play the notes
  F#-0 (0x1E) to F#-5 (0x5A) in sequence.

  The circuit:
  - digital in 1 connected to MIDI jack pin 5
  - MIDI jack pin 2 connected to ground
  - MIDI jack pin 4 connected to +5V through 220 ohm resistor
  - Attach a MIDI cable to the jack, then to a MIDI synth, and play music.

  created 13 Jun 2006
  modified 13 Aug 2012
  by Tom Igoe

  This example code is in the public domain.

  http://www.arduino.cc/en/Tutorial/Midi
*/

void setup() {
	// Set MIDI baud rate:
	Serial.begin(38400);
	delay(1000);
	//program change
	programChange();
}

void loop() {
	// play notes from F#-0 (0x1E) to F#-5 (0x5A):
	for (int channel = 7; channel < 11; channel++)
	{
		//Volicity low
		for (int note =(0x3c); note < (24+0x3c); note++) {
			//Note on channel 1 (0x90), some note value (note), middle velocity (0x30):
			noteOn(0x90 | channel, note, 0x30);
			delay(100);
			//Note on channel 1 (0x90), some note value (note), silent velocity (0x00):
			noteOn(0x90 | channel, note, 0x00);
			delay(100);
		}

	}

	//Volicity High
	for (int channel = 7; channel < 11; channel++)
	{
		for (int note = (0x3c); note < (24 + 0x3c); note++) {
			//Note on channel 1 (0x90), some note value (note), middle velocity (0x7F):
			noteOn(0x90 | channel, note, 0x7f);
			delay(150);
			//Note on channel 1 (0x90), some note value (note), silent velocity (0x00):
			noteOn(0x90 | channel, note, 0x00);
			delay(150);
		}

	}
}

// plays a MIDI note. Doesn't check to see that cmd is greater than 127, or that
// data values are less than 127:
void noteOn(int cmd, int pitch, int velocity) {
	Serial.write(cmd);
	Serial.write(pitch);
	Serial.write(velocity);
}

//Change instrument in channel
void programChange()
{
	//0x41=65=Instrument 66=Alto Sax
	Serial.write(0xC7);
	Serial.write(0x41);
	//27. Electric Guitar (jazz)
	Serial.write(0xC8);
	//26=instrument27 Electric Guitar (jazz)
	Serial.write(0x1A);

	//Drum
	Serial.write(0xC9);
	Serial.write(0x00);
	//25. Acoustic Guitar (nylon)
	Serial.write(0xCA);
	Serial.write(25-1);

}