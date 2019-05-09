using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfSerialPort
{
    public class MidiData:INotifyPropertyChanged
    {
        private byte[] serialdatas;

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public byte[] SerialDatas {
            get {
                return serialdatas;
            }
            set {
                serialdatas = value;
                NotifyPropertyChanged();
            }
        }

        
        public int DataIdx { get; set; }

        private string framemsg;
        public  string FrameMsg
        {
            get
            {
                return framemsg;
            }
            set
            {
                if (framemsg != value)
                {
                    framemsg = value;
                    NotifyPropertyChanged();

                }
            }
        }
        
        public int RealData {
            get
            {
                return ((int)serialdatas[2] << 7)+serialdatas[1] ;
            }

        }
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
