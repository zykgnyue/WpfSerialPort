using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfSerialPort
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort mySerialPort=null;
        MidiData Rxdata = null;
        public MainWindow()
        {
            InitializeComponent();
            Rxdata =new MidiData();

            //Binding myBinding = new Binding("FrameMsg");
            //myBinding.Source = Rxdata.FrameMsg;
            //myBinding.Mode = BindingMode.TwoWay;
           
            /////myBinding.ElementName = "FrameMsg";
            //// Bind the new data source to the myText TextBlock control's Text dependency property.
            //txtRxDataReal.SetBinding(TextBox.TextProperty, myBinding);

            comboBoxBaud.Items.Clear();
            comboBoxBaud.Items.Add("9600");
            comboBoxBaud.Items.Add("19200");
            comboBoxBaud.Items.Add("38400");
            comboBoxBaud.Items.Add("57600");
            comboBoxBaud.Items.Add("115200");
            comboBoxBaud.Items.Add("921600");
            comboBoxBaud.SelectedItem = comboBoxBaud.Items[2];
        }

        private void ComboBoxPortName_DropDownOpened(object sender, EventArgs e)
        {
            string[] portnames = SerialPort.GetPortNames();
            Console.WriteLine(portnames.Length);
            ComboBox x = sender as ComboBox;
            x.Items.Clear();
            foreach (string xx in portnames)
            {
                Console.WriteLine(xx);
                x.Items.Add(xx);
            }
        }

        private void ComboBoxBaud_DropDownOpened(object sender, EventArgs e)
        {

        }

        private void ComboBoxBaud_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox x = sender as ComboBox;
            Console.WriteLine("Selected:"+x.SelectedItem);
        }

        private void BtnCloseSerialPort_Click(object sender, RoutedEventArgs e)
        {
            if(mySerialPort!=null)
            {
                mySerialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);
                mySerialPort.Close();
                Console.WriteLine("Closed:"+mySerialPort.ToString());
            }
        }

        private void BtnOpenSerialPort_Click(object sender, RoutedEventArgs e)
        {
            if(comboBoxPortName.SelectedItem !=null)
            {
                if (mySerialPort != null)
                {
                    mySerialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);
                    mySerialPort.Close();
                }
                mySerialPort = new SerialPort(comboBoxPortName.SelectedItem.ToString());
                
                mySerialPort.BaudRate = int.Parse(comboBoxBaud.SelectedItem.ToString());
                mySerialPort.Parity = Parity.None;
                mySerialPort.StopBits = StopBits.One;
                mySerialPort.DataBits = 8;
                mySerialPort.Handshake = Handshake.None;
                mySerialPort.RtsEnable = false;
                mySerialPort.ReceivedBytesThreshold = 1;
                mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

                mySerialPort.Open();
                Console.WriteLine("Open Selected Port:" + comboBoxPortName.SelectedItem);
            }
            else
            {
                Console.WriteLine("No Selected Serial Port:");
                MessageBox.Show("Please select serial port", "Warning");

            }

        }

        private  void DataReceivedHandler(
                    object sender,
                    SerialDataReceivedEventArgs e)
        {
            //SerialPort sp = (SerialPort)sender;
            if (mySerialPort == null)
            {
                return;
            }
            int numOfByte = mySerialPort.BytesToRead;
            for(int i=0;i< numOfByte;i++)
            {

                int indata = mySerialPort.ReadByte(); 

                //Console.Write("\n{0:X2}\n ", indata);
                if ((indata & 0x80) !=0)
                {
         
                    //Console.Write("\n New Data Frame:");
                    Rxdata.DataIdx = 0;
                    Rxdata.SerialDatas[Rxdata.DataIdx] =(byte) indata;
                    Rxdata.DataIdx++;
                }else if(Rxdata.DataIdx < Rxdata.SerialDatas.Length)
                {
                    //Console.Write("{0:X2} ", indata);
                    Rxdata.SerialDatas[Rxdata.DataIdx] = (byte)indata;
                    Rxdata.DataIdx++;
                }

                if (Rxdata.DataIdx >= 3)
                {

                    //Output
                    //Console.Write("\n OneFrame:{0:X2}-{1:X2}-{2:X2}", Rxdata.SerialDatas[0], 
                    //    Rxdata.SerialDatas[1], 
                    //    Rxdata.SerialDatas[2]);
                    string msg = string.Format("\n OneFrame:{0:X2}-{1:X2}-{2:X2}, RealData=0x{3:X4}", Rxdata.SerialDatas[0],
                        Rxdata.SerialDatas[1],
                        Rxdata.SerialDatas[2],
                        Rxdata.RealData);
                    SetTextInTextBox(txtRxData,msg);
                    Rxdata.FrameMsg = msg;

                }
            }




        }

        private delegate void SetTextCallback(TextBox control, string text);
        public void SetTextInTextBox(TextBox control, string msg)
        {
            
            if (txtRxData.Dispatcher.CheckAccess())
            {
                txtRxData.AppendText(msg);
                txtRxData.ScrollToEnd();
            }
            else
            {
                SetTextCallback d = new SetTextCallback(SetTextInTextBox);
                Dispatcher.Invoke(d, new object[] { control, msg });
            }
        }
    }
}
