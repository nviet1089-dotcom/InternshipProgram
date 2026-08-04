#nullable disable
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using MediaBrushes = System.Windows.Media.Brushes;

// Alias cố định kiểu Point và Size từ System.Drawing
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace WpfSensorApp
{
    public partial class MainWindow : Window
    {
        private SerialPort _serialPort;
        private VideoCapture _capture;
        private Mat _frame;
        private bool _isGrayscale = false;

        private const double Y_BOTTOM_PIXEL = 400.0; 
        private const double Y_TOP_PIXEL = 100.0;    
        private const double MAX_WATER_HEIGHT_CM = 20.0;

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

        private void btnToggleGrayscale_Click(object sender, RoutedEventArgs e)
        {
            _isGrayscale = !_isGrayscale;

            if (_isGrayscale)
            {
                btnToggleGrayscale.Content = "Hiện màu nguyên bản";
                btnToggleGrayscale.Background = MediaBrushes.DarkGray;
            }
            else
            {
                btnToggleGrayscale.Content = "Khử màu (Trắng/Đen)";
                btnToggleGrayscale.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#FF7E57C2");
            }
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            if (_capture != null && _capture.Ptr != IntPtr.Zero)
            {
                _capture.Retrieve(_frame);
                if (!_frame.IsEmpty)
                {
                    using (Mat processedFrame = _frame.Clone())
                    {
                        double detectedWaterYPixel = DetectWaterLevelY(processedFrame);
                        double waterHeightCm = CalculateWaterHeightCm(detectedWaterYPixel);

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            txtWaterLevel.Text = $"{waterHeightCm:F1} cm";
                        }));

                        if (_isGrayscale)
                        {
                            CvInvoke.CvtColor(processedFrame, processedFrame, ColorConversion.Bgr2Gray);
                        }

                        using (Bitmap bitmap = processedFrame.ToBitmap())
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
        }

        private double DetectWaterLevelY(Mat image)
        {
            using (Mat gray = new Mat())
            using (Mat blurred = new Mat())
            using (Mat edges = new Mat())
            {
                CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
                CvInvoke.GaussianBlur(gray, blurred, new Size(5, 5), 0);
                CvInvoke.Canny(blurred, edges, 50, 150);

                LineSegment2D[] lines = CvInvoke.HoughLinesP(
                    edges, 
                    1,                 
                    Math.PI / 180,     
                    50,                
                    80,                
                    10                 
                );

                double bestY = Y_BOTTOM_PIXEL; 

                foreach (var line in lines)
                {
                    if (Math.Abs(line.P1.Y - line.P2.Y) < 10)
                    {
                        double currentY = (line.P1.Y + line.P2.Y) / 2.0;
                        CvInvoke.Line(image, line.P1, line.P2, new MCvScalar(0, 0, 255), 2);

                        if (currentY >= Y_TOP_PIXEL && currentY <= Y_BOTTOM_PIXEL)
                        {
                            bestY = currentY;
                            break;
                        }
                    }
                }

                CvInvoke.PutText(image, $"Line Y: {bestY:F0}px", new Point(20, 40), 
                    FontFace.HersheySimplex, 0.7, new MCvScalar(0, 255, 0), 2);

                return bestY;
            }
        }

        private double CalculateWaterHeightCm(double yPixel)
        {
            if (yPixel > Y_BOTTOM_PIXEL) return 0.0;
            if (yPixel < Y_TOP_PIXEL) return MAX_WATER_HEIGHT_CM;

            double heightCm = ((Y_BOTTOM_PIXEL - yPixel) / (Y_BOTTOM_PIXEL - Y_TOP_PIXEL)) * MAX_WATER_HEIGHT_CM;
            return Math.Max(0.0, heightCm);
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
            foreach (string port in ports) cboComPorts.Items.Add(port);
            if (cboComPorts.Items.Count > 0) cboComPorts.SelectedIndex = 0;
            else txtStatus.Text = "Trạng thái: Không tìm thấy cổng COM!";
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e) => LoadComPorts();

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (cboComPorts.SelectedItem == null) return;
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
                if (_serialPort.IsOpen) _serialPort.Close();
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
                Dispatcher.BeginInvoke(new Action(() => ParseAndDisplayData(line)));
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
                if (data.StartsWith("$") && data.EndsWith("#")) data = data.Substring(1, data.Length - 2);

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
            if (_capture != null) { _capture.Stop(); _capture.Dispose(); }
            if (_serialPort != null && _serialPort.IsOpen) { _serialPort.Close(); _serialPort.Dispose(); }
            base.OnClosed(e);
        }
    }
}