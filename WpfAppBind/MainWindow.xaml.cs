using System;
using System.Collections.Generic;
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

namespace WpfAppBind
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MyData myDataObject;
        public MainWindow()
        {
            InitializeComponent();

            // Make a new source.
            myDataObject = new MyData(DateTime.Now);
            Binding myBinding = new Binding("MyDataProperty");
            myBinding.Source = myDataObject;
            // Bind the new data source to the myText TextBlock control's Text dependency property.
            myText.SetBinding(TextBlock.TextProperty, myBinding);
            txtBox.SetBinding(TextBox.TextProperty, myBinding);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            myDataObject.MyDataProperty = DateTime.Now.ToString();
        }
    }
}
