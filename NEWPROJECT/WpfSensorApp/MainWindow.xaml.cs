using System;
using System.IO.Ports;
using System.Windows;
using System.Windows.Media;

namespace WpfSensorApp
{
    public partial class MainWindow : Window
    {
        private SerialPort _serialPort;

        public MainWindow()
        {
            InitializeComponent();
            _serialPort = new SerialPort();
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComPorts();
        }

        private void LoadComPorts()
        {
            cboComPorts.Items.Clear();
            string[] ports = SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                cboComPorts.Items.Add(port);
            }

            if (cboComPorts.Items.Count > 0)
            {
                cboComPorts.SelectedIndex = 0;
            }
            else
            {
                txtStatus.Text = "Trạng thái: Không tìm thấy cổng COM!";
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadComPorts();
        }

        // --- HÀM XỬ LÝ NÚT CONNECT (SỬA LỖI CS1061) ---
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (cboComPorts.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn cổng COM!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _serialPort.PortName = cboComPorts.SelectedItem.ToString();
                _serialPort.BaudRate = 9600;
                _serialPort.Open();

                btnConnect.IsEnabled = false;
                btnDisconnect.IsEnabled = true;
                cboComPorts.IsEnabled = false;
                btnRefresh.IsEnabled = false;

                txtStatus.Text = $"Trạng thái: Đã kết nối tới {_serialPort.PortName}";
                txtStatus.Foreground = Brushes.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- HÀM XỬ LÝ NÚT DISCONNECT (SỬA LỖI CS1061) ---
        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }

                btnConnect.IsEnabled = true;
                btnDisconnect.IsEnabled = false;
                cboComPorts.IsEnabled = true;
                btnRefresh.IsEnabled = true;

                txtStatus.Text = "Trạng thái: Đã ngắt kết nối";
                txtStatus.Foreground = Brushes.Gray;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi ngắt kết nối: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string line = _serialPort.ReadLine().Trim();

                Dispatcher.Invoke(() =>
                {
                    ParseAndDisplayData(line);
                });
            }
            catch
            {
                // Bỏ qua lỗi nhận dữ liệu không đầy đủ
            }
        }

        private void ParseAndDisplayData(string data)
        {
            if (data.Contains("T:") && data.Contains("H:") && data.Contains("|"))
            {
                string[] parts = data.Split('|');
                if (parts.Length == 2)
                {
                    string tempStr = parts[0].Replace("T:", "").Trim();
                    string humStr = parts[1].Replace("H:", "").Trim();

                    txtTemperature.Text = $"{tempStr} °C";
                    txtHumidity.Text = $"{humStr} %";
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            base.OnClosed(e);
        }
    }
}