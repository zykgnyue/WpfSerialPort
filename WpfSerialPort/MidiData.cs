using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfSerialPort
{
    public class MidiData
    {
        private byte[] serialdatas;
        public byte[] SerialDatas {
            get {
                return serialdatas;
            }
            set {
                serialdatas = value;
            }
        }
        public int DataIdx { get; set; }

        public MidiData()
        {
            serialdatas = new byte[3];
            serialdatas[0] = 0;
            serialdatas[1] = 0;
            serialdatas[2] = 0;
            DataIdx = 0;
        }
    }
}
