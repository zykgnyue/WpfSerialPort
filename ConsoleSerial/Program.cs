using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSerial
{
    class Program
    {
        public static void Main()
        {
            SerialPort mySerialPort = new SerialPort("COM3");

            mySerialPort.BaudRate = 38400;
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = 8;
            mySerialPort.Handshake = Handshake.None;
            mySerialPort.RtsEnable = false ;
            mySerialPort.ReadTimeout = 200;

            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            mySerialPort.Open();

            Console.WriteLine("Press any key to continue...");
            Console.WriteLine();
            Console.ReadKey();
            mySerialPort.Close();
        }

        private static void DataReceivedHandler(
                            object sender,
                            SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int indata;
            Console.WriteLine("\n>{0:d}=",sp.BytesToRead);
            int m = sp.BytesToRead;
            for (int i=0;i<m;i++)
            {
                indata= sp.ReadByte();
                if((indata&0x80)!=0)
                {

                }
                Console.Write("{0:x2} ", indata);
            }

            //Console.WriteLine("Data Received:{0:x2},{1:d}",indata,sp.BytesToRead);
            //string indata = sp.ReadExisting();
            //Console.WriteLine(indata.Length+"::");

            //foreach (byte x in indata)
            //{
            //    Console.WriteLine(x.ToString("X2"));

            //}


        }
    }
}
