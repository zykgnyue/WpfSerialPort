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
using ZedGraph;

namespace WpfSerialPort
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort mySerialPort=null;
        MidiData Rxdata = null;
        MidiData TxData = null;
        int tickStart = 0;
        public MainWindow()
        {
            InitializeComponent();
            Rxdata =new MidiData();
            TxData = new MidiData();

            SetGraph();
            Binding binding = new Binding("FrameMsg");
            binding.Source = Rxdata;
            //myBinding.Mode = BindingMode.TwoWay;

            /////myBinding.ElementName = "FrameMsg";
            //// Bind the new data source to the myText TextBlock control's Text dependency property.
            txtRxDataReal.SetBinding(TextBox.TextProperty, binding);

            //binding Txdata
            binding = new Binding("TxString");
            binding.Source = TxData;
            txtTxData.SetBinding(TextBox.TextProperty, binding);

            //SerialPort
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
            CloseSerialPort();
        }


        void CloseSerialPort()
        {
            if (mySerialPort != null)
            {
                mySerialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);
                mySerialPort.Close();
                Console.WriteLine("Closed:" + mySerialPort.ToString());
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
                    Rxdata.FrameMsg = string.Format("ADVal=0x{0:X4}={0:d}", Rxdata.RealData);
                    ///Add data point into ZedGraph
                    AddDataPoint(Rxdata.RealData);
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


        private void SetGraph()
        {
            GraphPane myPane = zedgraph.GraphPane;
            //zedgraph.GraphPane.Title.Text = "This is a dynamic chart";

            /// 设置标题
            myPane.Title.Text = "Test of Dynamic Data Update with ZedGraph " + "(After  25 seconds the graph scrolls)";
            /// 设置X轴说明文字
            myPane.XAxis.Title.Text = "Time, Seconds";
            /// 设置Y轴文字
            myPane.YAxis.Title.Text = "Sample Potential, Volts1";
            myPane.Y2Axis.Title.Text = "Sample Potential, Volts2";
            /// Save 1200 points. At 50 ms sample rate, this is one minute 
            /// The RollingPointPairList is an efficient storage class that always 
            /// keeps a rolling set of point data without needing to shift any data values 
            /// 设置1200个点，假设每50毫秒更新一次，刚好检测1分钟
            /// 一旦构造后将不能更改这个值
            //RollingPointPairList 
            //IPointList 
            RollingPointPairList list1 = new RollingPointPairList(1200);
            RollingPointPairList list2 = new RollingPointPairList(1200);
            /// Initially, a curve is added with no data points (list is empty) 
            /// Color is blue,  and there will be no symbols 
            /// 开始，增加的线是没有数据点的(也就是list为空)   
            ///增加一条名称 :Voltage ，颜色 Color.Bule ，无符号，无数据的空线条

            LineItem curve1 = myPane.AddCurve("Voltage1", list1, System.Drawing.Color.Blue, SymbolType.None/*.Diamond*/ );
            LineItem curve2 = myPane.AddCurve("Voltage2", list2, System.Drawing.Color.Red, SymbolType.None);

            curve2.IsY2Axis = true;
            myPane.Y2Axis.IsVisible = true;
            myPane.Y2Axis.Scale.Min = -5.0;
            myPane.Y2Axis.Scale.Max = 5.0;
            // Align the Y2 axis labels so they are flush to the axis
            myPane.Y2Axis.Scale.Align = AlignP.Inside;
            //curve2.YAxisIndex = 1;

            myPane.Y2Axis.Scale.MaxAuto = false;
            myPane.Y2Axis.Scale.MinAuto = false;

            /// Just manually control the X axis range so it scrolls continuously 
            /// instead of discrete step-sized jumps 
            /// X 轴最小值 0  

            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.MaxGrace = 0.01;
            myPane.XAxis.Scale.MinGrace = 0.01;
            /// X轴最大30 

            myPane.XAxis.Scale.Max = 30;

            /// X轴小步长1,也就是小间隔


            myPane.XAxis.Scale.MinorStep = 1;

            /// X轴大步长为5，也就是显示文字的大间隔


            myPane.XAxis.Scale.MajorStep = 5;

            /// Save the beginning time for reference 
            ///保存开始时间
            tickStart = Environment.TickCount;
            /// Scale the axes 
            /// 改变轴的刻度
            zedgraph.AxisChange();
        }

        void AddDataPoint(double dataX)
        {
            // Make sure that the curvelist has at least one curve 
            //确保CurveList不为空
            if (zedgraph.GraphPane.CurveList.Count <= 0) return;

            // Get the  first CurveItem in the graph 

            //取Graph第一个曲线，也就是第一步:在 GraphPane.CurveList 集合中查找 CurveItem 
            for (int idxList = 0; idxList < zedgraph.GraphPane.CurveList.Count; idxList++)
            {
                LineItem curve = zedgraph.GraphPane.CurveList[idxList] as LineItem;
                if (curve == null) return;

                // Get the PointPairList 
                //第二步:在CurveItem中访问PointPairList(或者其它的IPointList)，根据自己的需要增加新数据或修改已存在的数据

                IPointListEdit list = curve.Points as IPointListEdit;

                // If this is null, it means the reference at curve.Points does not  
                // support IPointListEdit, so we won't be able to modify it 

                if (list == null) return;


                // Time is measured in seconds 
                double time = (Environment.TickCount - tickStart) / 1000.0;
                // 3 seconds per cycle 

                list.Add(time, dataX);

                // Keep the X scale at a rolling 30 second interval, with one 

                // major step between the max X value and the end of the axis 
                Scale xScale = zedgraph.GraphPane.XAxis.Scale;
                if (time > xScale.Max - xScale.MajorStep)
                {
                    xScale.Max = time + xScale.MajorStep;
                    xScale.Min = xScale.Max - 30.0;
                }

            }
            // Make sure the Y axis is rescaled to accommodate actual data 
            //第三步:调用ZedGraphControl.AxisChange()方法更新X和Y轴的范围


            zedgraph.AxisChange(); // Force a redraw  
                                   //第四步:调用Form.Invalidate()方法更新图表


            zedgraph.Invalidate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseSerialPort();
        }

        private void BtnSendData_Click(object sender, RoutedEventArgs e)
        {
            byte[] txdat = TxData.TxBytes;
            if (txdat != null &&mySerialPort!=null)
            {
                mySerialPort.Write(txdat,0,txdat.Length);
            }

        }
    }
}
