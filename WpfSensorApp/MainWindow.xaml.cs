#nullable disable
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MediaBrushes = System.Windows.Media.Brushes;

using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Rectangle = System.Drawing.Rectangle;

namespace WpfSensorApp
{
    public partial class MainWindow : Window
    {
        private SerialPort _serialPort;
        private VideoCapture _capture;
        private Mat _frame;

        private bool _isGrayscale = false;
        private bool _showOverlay = false;

        private double _smoothedWaterY = -1;
        private double _lastDetectedWaterY = -1;
        private Rectangle _smoothedContainer = Rectangle.Empty;
        private int _frameCounter = 0;

        private const double MAX_WATER_HEIGHT_CM = 20.0;

        public MainViewModel ViewModel { get; set; } = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = ViewModel;

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
            btnToggleGrayscale.Content = "Gray";
            btnToggleGrayscale.Background = _isGrayscale 
                ? MediaBrushes.DimGray 
                : (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#FF757575");
        }

        private void btnToggleOverlay_Click(object sender, RoutedEventArgs e)
        {
            _showOverlay = !_showOverlay;
            btnToggleOverlay.Content = _showOverlay ? "Ẩn Vạch Mực Nước" : "Hiển thị Vạch Mực Nước";
            btnToggleOverlay.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom(_showOverlay ? "#FFE53935" : "#FF0288D1");
        }

        private void btnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (imgWebcam.Source is BitmapSource bitmapSource)
                {
                    string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string fileName = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    string filePath = Path.Combine(folderPath, fileName);

                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }

                    ShowImagePreviewWindow(bitmapSource, filePath);
                }
                else
                {
                    MessageBox.Show("Không có hình ảnh từ Camera để chụp!", 
                                    "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chụp màn hình: {ex.Message}", 
                                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowImagePreviewWindow(BitmapSource imageSource, string filePath)
        {
            Window previewWindow = new Window
            {
                Title = $"Xem Ảnh Chụp - {Path.GetFileName(filePath)}",
                Width = 680,
                Height = 520,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = MediaBrushes.Black
            };

            System.Windows.Controls.Image imgControl = new System.Windows.Controls.Image
            {
                Source = imageSource,
                Stretch = System.Windows.Media.Stretch.Uniform,
                Margin = new Thickness(10)
            };

            previewWindow.Content = imgControl;
            previewWindow.Show();
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
                        if (_isGrayscale)
                        {
                            CvInvoke.CvtColor(processedFrame, processedFrame, ColorConversion.Bgr2Gray);
                            CvInvoke.CvtColor(processedFrame, processedFrame, ColorConversion.Gray2Bgr);
                        }

                        double waterHeightCm = ProcessContainerAndWaterLevel(processedFrame);

                        double dangerRatio = waterHeightCm / MAX_WATER_HEIGHT_CM;
                        double dangerLevel = Math.Min(10.0, Math.Max(0.0, dangerRatio * 10.0));

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ViewModel.WaterLevel = $"{waterHeightCm:F1} cm";
                            ViewModel.DangerLevel = $"{dangerLevel:F1}/10";
                        }));

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

        private double ProcessContainerAndWaterLevel(Mat image)
        {
            _frameCounter++;

            using (Mat gray = new Mat())
            using (Mat blurred = new Mat())
            using (Mat edges = new Mat())
            {
                CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
                CvInvoke.GaussianBlur(gray, blurred, new Size(5, 5), 0);
                CvInvoke.Canny(blurred, edges, 50, 150);

                Rectangle currentContainer = Rectangle.Empty;
                double maxArea = 0;

                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(edges, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                    // 1. TỰ ĐỘNG LỰA CHỌN VẬT THỂ LỚN NHẤT (GẦN CAMERA NHẤT)
                    for (int i = 0; i < contours.Size; i++)
                    {
                        Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                        double area = rect.Width * rect.Height;

                        if (rect.Height > 80 && rect.Width > 40 && rect.Height > rect.Width && area > maxArea)
                        {
                            maxArea = area;
                            currentContainer = rect;
                        }
                    }
                }

                if (currentContainer.IsEmpty)
                {
                    if (_showOverlay)
                    {
                        CvInvoke.PutText(image, "Khong tim thay binh nuoc...", new Point(20, 40),
                            FontFace.HersheySimplex, 0.6, new MCvScalar(0, 165, 255), 2);
                    }
                    return 0.0;
                }

                // Làm mịn vị trí chủ thể (Low-pass Filter)
                if (_smoothedContainer.IsEmpty)
                {
                    _smoothedContainer = currentContainer;
                }
                else
                {
                    _smoothedContainer.X = (int)(_smoothedContainer.X * 0.92 + currentContainer.X * 0.08);
                    _smoothedContainer.Y = (int)(_smoothedContainer.Y * 0.92 + currentContainer.Y * 0.08);
                    _smoothedContainer.Width = (int)(_smoothedContainer.Width * 0.92 + currentContainer.Width * 0.08);
                    _smoothedContainer.Height = (int)(_smoothedContainer.Height * 0.92 + currentContainer.Height * 0.08);
                }

                // 2. GIỮ LẠI CHỦ THỂ CHÍNH VÀ CHUYỂN TOÀN BỘ NỀN XUNG QUANH THÀNH MÀU TRẮNG
                using (Mat whiteBg = new Mat(image.Size, DepthType.Cv8U, 3))
                using (Mat mask = new Mat(image.Size, DepthType.Cv8U, 1))
                {
                    whiteBg.SetTo(new MCvScalar(255, 255, 255)); // Đã sửa SetValue -> SetTo
                    mask.SetTo(new MCvScalar(0));                // Mặt nạ mặc định che hết

                    // Tạo vùng chọn cho chủ thể gần nhất
                    CvInvoke.Rectangle(mask, _smoothedContainer, new MCvScalar(255), -1);

                    // Đè chủ thể lên nền trắng
                    image.CopyTo(whiteBg, mask);
                    whiteBg.CopyTo(image);
                }

                // 3. ĐO MỰC NƯỚC (GIẢM TẦN SUẤT XỬ LÝ XUỐNG ~15 FPS ĐỂ GIẢM NHIỄU)
                double currentWaterY = _smoothedContainer.Bottom;

                if (_frameCounter % 2 == 0 || _lastDetectedWaterY < 0)
                {
                    LineSegment2D[] lines = CvInvoke.HoughLinesP(edges, 1, Math.PI / 180, 30, 30, 10);

                    foreach (var line in lines)
                    {
                        if (line.P1.X >= _smoothedContainer.Left - 10 && line.P2.X <= _smoothedContainer.Right + 10 &&
                            Math.Abs(line.P1.Y - line.P2.Y) < 12)
                        {
                            double lineY = (line.P1.Y + line.P2.Y) / 2.0;
                            if (lineY > _smoothedContainer.Top && lineY < _smoothedContainer.Bottom)
                            {
                                currentWaterY = lineY;
                                break;
                            }
                        }
                    }
                    _lastDetectedWaterY = currentWaterY;
                }
                else
                {
                    currentWaterY = _lastDetectedWaterY;
                }

                // 4. LÀM MƯỢT CHUYỂN ĐỘNG VẠCH ĐỎ (DEADBAND & LOW-PASS FILTER)
                if (_smoothedWaterY < 0)
                {
                    _smoothedWaterY = currentWaterY;
                }
                else
                {
                    if (Math.Abs(currentWaterY - _smoothedWaterY) > 2.5)
                    {
                        _smoothedWaterY = _smoothedWaterY * 0.96 + currentWaterY * 0.04;
                    }
                }

                // 5. HIỂN THỊ VẠCH ĐO NƯỚC NẰM GỌN TRONG CHỦ THỂ
                if (_showOverlay)
                {
                    // Vạch màu đỏ ngang lòng bình nước
                    CvInvoke.Line(image, 
                        new Point(_smoothedContainer.Left, (int)_smoothedWaterY), 
                        new Point(_smoothedContainer.Right, (int)_smoothedWaterY), 
                        new MCvScalar(0, 0, 255), 3);

                    // Khung viền xanh lá khóa vị trí chủ thể
                    CvInvoke.Rectangle(image, _smoothedContainer, new MCvScalar(0, 200, 0), 2);
                }

                double waterHeightCm = ((_smoothedContainer.Bottom - _smoothedWaterY) / _smoothedContainer.Height) * MAX_WATER_HEIGHT_CM;
                return Math.Max(0.0, waterHeightCm);
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
            foreach (string port in ports) cboComPorts.Items.Add(port);
            if (cboComPorts.Items.Count > 0) cboComPorts.SelectedIndex = 0;
            else 
            {
                ViewModel.StatusText = "Trạng thái: Không tìm thấy cổng COM!";
                ViewModel.StatusColor = MediaBrushes.Red;
            }
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

                ViewModel.StatusText = $"Trạng thái: Đã kết nối tới {_serialPort.PortName}";
                ViewModel.StatusColor = MediaBrushes.Green;
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

                ViewModel.StatusText = "Trạng thái: Đã ngắt kết nối";
                ViewModel.StatusColor = MediaBrushes.Gray;
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

                    ViewModel.Temperature = $"{tempStr} °C";
                    ViewModel.Humidity = $"{humStr} %";
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_capture != null) { _capture.Stop(); _capture.Dispose(); }
            if (_serialPort != null && _serialPort.IsOpen) { _serialPort.Close(); _serialPort.Dispose(); } base.OnClosed(e);
        }
    }
}