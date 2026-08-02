#nullable disable
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;
using MediaBrushes = System.Windows.Media.Brushes;

namespace WpfSensorApp
{
    public partial class MainWindow : Window
    {
        private SerialPort _serialPort;
        private VideoCapture _capture;
        private Mat _frame;

        public MainWindow()
        {
            InitializeComponent();
            _serialPort = new SerialPort();
            _serialPort.DataReceived += SerialPort_DataReceived;
            _frame = new Mat();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComPorts();
            StartWebcam();
        }

        private void StartWebcam()
        {
            try
            {
                _capture = new VideoCapture(0);
                _capture.ImageGrabbed += ProcessFrame;
                _capture.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở Webcam: {ex.Message}", "Lỗi Camera", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            if (_capture != null && _capture.Ptr != IntPtr.Zero)
            {
                _capture.Retrieve(_frame);
                if (!_frame.IsEmpty)
                {
                    using (Bitmap bitmap = _frame.ToBitmap())
                    {
                        BitmapImage bitmapImage = ConvertBitmapToBitmapImage(bitmap);
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            imgWebcam.Source = bitmapImage;
                        }));
                    }
                }
            }
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
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
                txtStatus.Foreground = MediaBrushes.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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
                txtStatus.Foreground = MediaBrushes.Gray;
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
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ParseAndDisplayData(line);
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi đọc serial: {ex.Message}");
            }
        }

        private void ParseAndDisplayData(string data)
        {
            if (data.Contains("T:") && data.Contains("H:") && data.Contains("|"))
            {
                if (data.StartsWith("$") && data.EndsWith("#"))
                {
                    data = data.Substring(1, data.Length - 2);
                }

                string[] parts = data.Split('|');
                if (parts.Length == 2)
                {
                    string tempStr = parts[0].Replace("T:", "").Trim();
                    string humStr = parts[1].Replace("H:", "").Trim();

                    txtTemperature.Text = $"{tempStr} °C";
                    txtHumidity.Text = $"{humStr} %";

                    if (double.TryParse(tempStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double tempValue))
                    {
                        if (tempValue > 35.0)
                        {
                            txtStatus.Text = $"CẢNH BÁO: Nhiệt độ vượt ngưỡng ({tempValue:F1} °C)!";
                            txtStatus.Foreground = MediaBrushes.Red;
                        }
                        else
                        {
                            txtStatus.Text = $"Trạng thái: Đã kết nối tới {_serialPort.PortName} | Hoạt động bình thường";
                            txtStatus.Foreground = MediaBrushes.Green;
                        }
                    }
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_capture != null)
            {
                _capture.Stop();
                _capture.Dispose();
            }

            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
            }

            base.OnClosed(e);
        }
    }
}