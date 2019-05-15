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
#include <Streaming.h>
// These constants won't change. They're used to give names to the pins used:
const int analogInPin = A0;  // Analog input pin that the potentiometer is attached to
const int analogOutPin = 9; // Analog output pin that the LED is attached to

int sensorValue = 0;        // value read from the pot
int outputValue = 0;        // value output to the PWM (analog out)
#define DF_DataLen (3)
unsigned long prevTime;
#define DF_TimePeriod	(1000)
byte buffer[DF_DataLen];
byte bufCrc[8];
byte rxbuf[DF_DataLen];
byte rxIdx;

#define CRC8_POLY 0x2F // Polynomial=0x2F
#define CRC8_INIT 0xFF //Initial value=0xFF 
/* *
* Calculating CRC - 8in 'C'
* @para pInData, pointer of data
* @para length, length of data
* @return CRC value
*/
uint8_t CRC8(uint8_t *pInData, uint8_t length)
{
	uint8_t i = 0, crc = CRC8_INIT;
	while (length-- > 0)
	{
		crc ^= *pInData;
		pInData++;
		for (i = 0; i < 8; i++)
		{
			if (crc & 0x80)
			{
				crc = (crc << 1) ^ CRC8_POLY;
			} else
			{
			crc <<= 1;
			}
		}
	} 
	crc ^= CRC8_INIT;
	return crc;
}



void setup() {
	// initialize serial communications at 9600 bps:
	Serial.begin(38400);
	rxIdx = 0;

}

void CalcPowerCrc(byte enptc1,uint16_t pow)
{
	bufCrc[0] = enptc1;
	// moto bigendian
	bufCrc[1] = (uint8_t)((pow>>8)&0xff);
	bufCrc[2] = (uint8_t)(pow&0xff);
	bufCrc[3] = 0x00;
	bufCrc[4] = 0x00;
	bufCrc[5] = 0x00;
	bufCrc[6] = 0x00;
	bufCrc[7] = CRC8(bufCrc, 7);
	Serial << _HEX(bufCrc[0])<<"-"<<_HEX(bufCrc[1])<<"-";
	Serial << _HEX(bufCrc[2]) << "-" << _HEX(bufCrc[3]) << "-";
	Serial << _HEX(bufCrc[4]) << "-" << _HEX(bufCrc[5]) << "-";
	Serial << _HEX(bufCrc[6]) << "-" << _HEX(bufCrc[7]) << endl;

}

uint16_t RePackData(byte dat1, byte dat2)
{
	uint16_t ret;
	ret = dat2 & 0x7f;
	ret <<= 7;
	ret += (dat1 & 0x7f);
	return ret;
}


void loop() {
	uint16_t repack;
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

		//CalcPowerCrc(0, 0);
		//CalcPowerCrc(1, 2500);
	}
	if (Serial.available()>0)
	{
			int rxdata=Serial.read();
			//Serial.write(rxdata);
			if ((rxdata & 0x80) != 0)
			{
				rxIdx = 0;
				rxbuf[rxIdx] = (byte)rxdata;
				rxIdx++;
			}
			else if( (rxIdx == 1) 
				&& ((rxdata & 0x80) == 0))
			{
				rxbuf[rxIdx] = (byte)rxdata;
				rxIdx++;
			}
			else if((rxIdx == 2)
				&& ((rxdata & 0x80) == 0))
			{
				rxbuf[rxIdx] = (byte)rxdata;
				rxIdx++;
			}
			else {
				rxIdx = 0;
			}
			if (rxIdx == 3)
			{
				//Test reconstruct
				repack=RePackData(rxbuf[1], rxbuf[2]);
				Serial.write((byte*)&repack, 2);
			}
	}
	
}