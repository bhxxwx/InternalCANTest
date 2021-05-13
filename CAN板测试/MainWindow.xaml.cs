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

using CANDriverLayer;

namespace CAN板测试
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private CanDriver canDriver = new CanDriver();

        public MainWindow()
        {
            InitializeComponent();

            canDriver.Info += CanDriver_Info;

            comboBox.Items.Clear();
            comboBox.Items.Add("0");
            comboBox.Items.Add("1");
            comboBox.SelectedIndex = 0;
            comboBox.IsEnabled = false;

            comboBox1.Items.Clear();
            comboBox1.Items.Add("0");
            comboBox1.Items.Add("1");
            comboBox1.SelectedIndex = 1;

            CANDriverLayer.CanDriver.JWD_Data jWD_Data = new CanDriver.JWD_Data();

            CANDriverLayer.CanDriver.GT_Data vvv = new CanDriver.GT_Data();
            MessageBox.Show(vvv.Len.ToString());
            //CanDriver
        }

        private void CanDriver_Info(string infos)
        {
            this.Dispatcher.Invoke(() =>
            {
                recvtext.Text += infos + "\n";
            });
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            if (sendtext.Text != "")
                canDriver.Write(sendtext.Text);
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            //canDriver.OpenCAN(Convert.ToUInt32(comboBox.Text));
            //canDriver.OpenChannel(Convert.ToUInt32(comboBox1.Text));
            canDriver.OpenPort();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            canDriver.CloseCAN();
        }
    }
}