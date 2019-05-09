/*
  Analog input, analog output, serial output

  Reads an analog input pin, maps the result to a range from 0 to 255 and uses
  the result to set the pulse width modulation (PWM) of an output pin.
  Also prints the results to the Serial Monitor.

  The circuit:
  - potentiometer connected to analog pin 0.
	Center pin of the potentiometer goes to the analog pin.
	side pins of the potentiometer go to +5V and ground
  - LED connected from digital pin 9 to ground

  created 29 Dec. 2008
  modified 9 Apr 2012
  by Tom Igoe

  This example code is in the public domain.

  http://www.arduino.cc/en/Tutorial/AnalogInOutSerial
*/

// These constants won't change. They're used to give names to the pins used:
const int analogInPin = A0;  // Analog input pin that the potentiometer is attached to
const int analogOutPin = 9; // Analog output pin that the LED is attached to

int sensorValue = 0;        // value read from the pot
int outputValue = 0;        // value output to the PWM (analog out)
#define DF_DataLen (3)
unsigned long prevTime;
#define DF_TimePeriod	(1000)
byte buffer[DF_DataLen];

void setup() {
	// initialize serial communications at 9600 bps:
	Serial.begin(38400);

}

void loop() {
	unsigned long currentime = millis();
	if ((currentime - prevTime) >= DF_TimePeriod)
	{
		prevTime = currentime;
		// read the analog in value:
		sensorValue = analogRead(analogInPin);
		// map it to the range of the analog out:
		outputValue = map(sensorValue, 0, 1023, 0, 255);
		// change the analog out value:
		analogWrite(analogOutPin, outputValue);

		// print the results to the Serial Monitor:
		//Serial.print("sensor = ");
		//Serial.print(sensorValue);
		//Serial.print("\t output = ");
		//Serial.println(outputValue);
		//Midi:0xE0, AD通道号(0..7)
		buffer[0] = 0xE0 | (analogInPin - 14);
		//Midi:Little endian ADbit0-6=>buffer[1]
		buffer[1] = (byte)(sensorValue & 0x7F);
		//Midi:Little endian ADbit7-9=>buffer[2]
		buffer[2] = (byte)((sensorValue >> 7) & 0x7F);
		Serial.write(buffer,3);
	}
	else {
		if (Serial.available())
		{
			int rxdata=Serial.read();
			Serial.write(rxdata);
		}
	}

	
}