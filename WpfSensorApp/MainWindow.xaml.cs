#nullable disable
using System;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading; // DispatcherTimer cho Day 27
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MediaBrushes = System.Windows.Media.Brushes;
using Path = System.IO.Path;

using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Rectangle = System.Drawing.Rectangle;

namespace WpfSensorApp
{
    public partial class MainWindow : Window
    {
        private SerialPort _serialPort;
        private VideoCapture _capture;

        private bool _isGrayscale = false;
        private bool _showOverlay = false;
        private bool _isBackgroundActive = false;
        private bool _isDarkMode = false;

        private double _smoothedWaterY = -1;
        private double _lastDetectedWaterY = -1;
        private Rectangle _smoothedContainer = Rectangle.Empty;

        // Các biến số thực phục vụ làm mượt và khóa khung nhận diện
        private double _smoothedX = -1;
        private double _smoothedY = -1;
        private double _smoothedW = -1;
        private double _smoothedH = -1;

        private int _frameCounter = 0;
        private const double MAX_WATER_HEIGHT_CM = 20.0;

        // --- DAY 25 & 26: NGƯỠNG VÀ TRẠNG THÁI CẢNH BÁO ---
        private double _tempThreshold = 35.0;
        private double _waterThreshold = 8.0;
        private double _currentTemp = 0.0;
        private double _currentWaterScale = 0.0;

        private bool _isTempAlarm = false;
        private bool _isWaterAlarm = false;

        // --- DAY 27: TIMER HIỆU ỨNG NHẤP NHÁY ---
        private DispatcherTimer _blinkTimer;
        private bool _isBlinkStateToggle = false;

        private readonly System.Windows.Media.Brush _colorGreen = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#FF4CAF50");
        private readonly System.Windows.Media.Brush _colorRed = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#FFE53935");

        public MainViewModel ViewModel { get; set; } = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = ViewModel;

            _serialPort = new SerialPort();
            _serialPort.DataReceived += SerialPort_DataReceived;

            InitBlinkTimer();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComPorts();
            StartWebcam();
        }

        // --- DAY 27: KHỞI TẠO TIMER NHẤP NHÁY (400ms) ---
        private void InitBlinkTimer()
        {
            _blinkTimer = new DispatcherTimer();
            _blinkTimer.Interval = TimeSpan.FromMilliseconds(400);
            _blinkTimer.Tick += BlinkTimer_Tick;
            _blinkTimer.Start();
        }

        private void BlinkTimer_Tick(object sender, EventArgs e)
        {
            _isBlinkStateToggle = !_isBlinkStateToggle;
            BrushConverter bc = new BrushConverter();

            Brush redBrush = (Brush)bc.ConvertFrom("#FFE53935");
            Brush whiteBrush = _isDarkMode ? (Brush)bc.ConvertFrom("#1E1E1E") : MediaBrushes.White;

            // 1. Nhấp nháy cảnh báo Nhiệt độ
            if (_isTempAlarm)
            {
                cardTemp.Background = _isBlinkStateToggle ? redBrush : whiteBrush;
            }
            else
            {
                cardTemp.Background = _isDarkMode ? (Brush)bc.ConvertFrom("#1E2A1E") : (Brush)bc.ConvertFrom("#FFE8F5E9");
            }

            // 2. Nhấp nháy cảnh báo Mực nước
            if (_isWaterAlarm)
            {
                cardDanger.Background = _isBlinkStateToggle ? redBrush : whiteBrush;
            }
            else
            {
                cardDanger.Background = _isDarkMode ? (Brush)bc.ConvertFrom("#1E1E1E") : (Brush)bc.ConvertFrom("#FAFAFA");
            }
        }

        // --- DAY 25: LẤY DỮ LIỆU TỪ Ô NHẬP NGƯỠNG ---
        private void txtThreshold_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtTempThreshold != null && double.TryParse(txtTempThreshold.Text, out double temp))
            {
                _tempThreshold = temp;
            }

            if (txtWaterThreshold != null && double.TryParse(txtWaterThreshold.Text, out double water))
            {
                _waterThreshold = water;
            }
        }

        private void StartWebcam()
        {
            try
            {
                _capture = new VideoCapture(0, VideoCapture.API.Any);
                if (_capture.IsOpened)
                {
                    _capture.ImageGrabbed += ProcessFrame;
                    _capture.Start();
                }
                else
                {
                    ViewModel.StatusText = "Trạng thái: Không mở được Webcam (Index 0)";
                    ViewModel.StatusColor = MediaBrushes.Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở Webcam: {ex.Message}", "Lỗi Camera", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnToggleGrayscale_Click(object sender, RoutedEventArgs e)
        {
            _isGrayscale = !_isGrayscale;
            btnToggleGrayscale.Background = _isGrayscale ? _colorRed : _colorGreen;
        }

        private void btnToggleBackground_Click(object sender, RoutedEventArgs e)
        {
            _isBackgroundActive = !_isBackgroundActive;
            btnToggleBackground.Background = _isBackgroundActive ? _colorRed : _colorGreen;
        }

        private void btnToggleOverlay_Click(object sender, RoutedEventArgs e)
        {
            _showOverlay = !_showOverlay;
            btnToggleOverlay.Content = _showOverlay ? "Ẩn Vạch Mực Nước" : "Hiển thị Vạch Mực Nước";
            btnToggleOverlay.Background = _showOverlay ? _colorRed : _colorGreen;
        }

        private void toggleDarkMode_Click(object sender, RoutedEventArgs e)
        {
            _isDarkMode = toggleDarkMode.IsChecked ?? false;
            ApplyTheme(_isDarkMode);
        }

        private void ApplyTheme(bool isDark)
        {
            BrushConverter bc = new BrushConverter();

            if (isDark)
            {
                this.Background = (Brush)bc.ConvertFrom("#121212");
                grpSerial.Foreground = MediaBrushes.White;
                lblComPort.Foreground = MediaBrushes.White;
                lblDarkMode.Foreground = MediaBrushes.White;

                if (grpThresholds != null) grpThresholds.Foreground = MediaBrushes.White;
                if (lblTempThreshold != null) lblTempThreshold.Foreground = MediaBrushes.White;
                if (lblWaterThreshold != null) lblWaterThreshold.Foreground = MediaBrushes.White;

                cardHum.Background = (Brush)bc.ConvertFrom("#1E2A1E");
                cardDanger.BorderBrush = (Brush)bc.ConvertFrom("#333333");
                cardCamera.Background = (Brush)bc.ConvertFrom("#251A2C");
                cardCamera.BorderBrush = (Brush)bc.ConvertFrom("#4A154B");

                rectUnfilledOverlay.Fill = (Brush)bc.ConvertFrom("#1E1E1E");
                txtDangerLevel.Foreground = MediaBrushes.White;
                sbStatus.Background = (Brush)bc.ConvertFrom("#1E1E1E");
            }
            else
            {
                this.Background = MediaBrushes.White;
                grpSerial.Foreground = (Brush)bc.ConvertFrom("#333333");
                lblComPort.Foreground = (Brush)bc.ConvertFrom("#333333");
                lblDarkMode.Foreground = (Brush)bc.ConvertFrom("#333333");

                if (grpThresholds != null) grpThresholds.Foreground = (Brush)bc.ConvertFrom("#333333");
                if (lblTempThreshold != null) lblTempThreshold.Foreground = (Brush)bc.ConvertFrom("#333333");
                if (lblWaterThreshold != null) lblWaterThreshold.Foreground = (Brush)bc.ConvertFrom("#333333");

                cardHum.Background = (Brush)bc.ConvertFrom("#FFE8F5E9");
                cardDanger.BorderBrush = (Brush)bc.ConvertFrom("#E0E0E0");
                cardCamera.Background = (Brush)bc.ConvertFrom("#FFF3E5F5");
                cardCamera.BorderBrush = (Brush)bc.ConvertFrom("#FFCE93D8");

                rectUnfilledOverlay.Fill = (Brush)bc.ConvertFrom("#FAFAFA");
                txtDangerLevel.Foreground = (Brush)bc.ConvertFrom("#333333");
                sbStatus.Background = (Brush)bc.ConvertFrom("#FFF5F5F5");
            }
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
            if (_capture == null || !_capture.IsOpened) return;

            using (Mat localFrame = new Mat())
            {
                _capture.Retrieve(localFrame);
                if (localFrame.IsEmpty) return;

                using (Mat processedFrame = localFrame.Clone())
                {
                    if (_isGrayscale)
                    {
                        using (Mat grayMat = new Mat())
                        {
                            CvInvoke.CvtColor(processedFrame, grayMat, ColorConversion.Bgr2Gray);
                            CvInvoke.CvtColor(grayMat, processedFrame, ColorConversion.Gray2Bgr);
                        }
                    }

                    double scaleLevel = ProcessContainerAndWaterLevel(processedFrame);
                    _currentWaterScale = scaleLevel;

                    // DAY 26: Logic so sánh liên tục ngưỡng mực nước
                    _isWaterAlarm = _currentWaterScale >= _waterThreshold;

                    double scaleRatio = scaleLevel / 10.0;
                    double waterHeightCm = scaleRatio * MAX_WATER_HEIGHT_CM;

                    BitmapImage bitmapImage = ConvertMatToBitmapImage(processedFrame);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ViewModel.WaterLevel = $"{waterHeightCm:F1} cm";
                        ViewModel.DangerLevel = $"Scale: {scaleLevel:F1} / 10";

                        BrushConverter bc = new BrushConverter();
                        
                        if (scaleLevel >= 8.0)
                        {
                            ViewModel.WaterLevelColor = (Brush)bc.ConvertFrom("#FFE53935");
                            ViewModel.WaterLevelBgColor = _isDarkMode ? (Brush)bc.ConvertFrom("#3E0F0F") : (Brush)bc.ConvertFrom("#FFFFEBEE");
                        }
                        else if (scaleLevel >= 5.0)
                        {
                            ViewModel.WaterLevelColor = (Brush)bc.ConvertFrom("#FFF57F17");
                            ViewModel.WaterLevelBgColor = _isDarkMode ? (Brush)bc.ConvertFrom("#3E2E04") : (Brush)bc.ConvertFrom("#FFFDE0B2");
                        }
                        else if (scaleLevel >= 2.5)
                        {
                            ViewModel.WaterLevelColor = (Brush)bc.ConvertFrom("#FF4CAF50");
                            ViewModel.WaterLevelBgColor = _isDarkMode ? (Brush)bc.ConvertFrom("#0F3E18") : (Brush)bc.ConvertFrom("#FFE8F5E9");
                        }
                        else
                        {
                            ViewModel.WaterLevelColor = (Brush)bc.ConvertFrom("#FF0288D1");
                            ViewModel.WaterLevelBgColor = _isDarkMode ? (Brush)bc.ConvertFrom("#0F2D3C") : (Brush)bc.ConvertFrom("#FFE1F5FE");
                        }

                        if (gridBarContainer != null && rectUnfilledOverlay != null)
                        {
                            double totalBarHeight = gridBarContainer.ActualHeight;
                            if (totalBarHeight > 0)
                            {
                                double unfilledRatio = 1.0 - Math.Min(1.0, Math.Max(0.0, scaleRatio));
                                rectUnfilledOverlay.Height = totalBarHeight * unfilledRatio;
                            }
                        }

                        if (imgWebcam != null)
                        {
                            imgWebcam.Source = bitmapImage;
                        }
                    }));
                }
            }
        }

        private double ProcessContainerAndWaterLevel(Mat image)
        {
            _frameCounter++;

            using (Mat gray = new Mat())
            {
                CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
                CvInvoke.GaussianBlur(gray, gray, new Size(5, 5), 0);

                using (Mat edges = new Mat())
                {
                    CvInvoke.Canny(gray, edges, 35, 110);

                    Rectangle currentContainer = Rectangle.Empty;
                    double maxArea = 0;
                    double minAreaThreshold = image.Width * image.Height * 0.008; 

                    using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                    {
                        CvInvoke.FindContours(edges, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                        for (int i = 0; i < contours.Size; i++)
                        {
                            Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                            double area = rect.Width * rect.Height;

                            if (rect.X <= 2 || rect.Y <= 2 || rect.Right >= image.Width - 2 || rect.Bottom >= image.Height - 2)
                                continue;

                            if (area > minAreaThreshold && area > maxArea)
                            {
                                maxArea = area;
                                currentContainer = rect;
                            }
                        }
                    }

                    if (!currentContainer.IsEmpty)
                    {
                        if (_smoothedW < 0)
                        {
                            _smoothedX = currentContainer.X;
                            _smoothedY = currentContainer.Y;
                            _smoothedW = currentContainer.Width;
                            _smoothedH = currentContainer.Height;
                        }
                        else
                        {
                            double deltaX = Math.Abs(currentContainer.X - _smoothedX);
                            double deltaY = Math.Abs(currentContainer.Y - _smoothedY);
                            double deltaW = Math.Abs(currentContainer.Width - _smoothedW);
                            double deltaH = Math.Abs(currentContainer.Height - _smoothedH);

                            bool isStationary = (deltaX < 12 && deltaY < 12 && deltaW < 15 && deltaH < 15);

                            if (!isStationary)
                            {
                                double totalDelta = deltaX + deltaY + deltaW + deltaH;
                                double alpha = totalDelta > 80 ? 0.22 : 0.08;

                                _smoothedX = _smoothedX * (1.0 - alpha) + currentContainer.X * alpha;
                                _smoothedY = _smoothedY * (1.0 - alpha) + currentContainer.Y * alpha;
                                _smoothedW = _smoothedW * (1.0 - alpha) + currentContainer.Width * alpha;
                                _smoothedH = _smoothedH * (1.0 - alpha) + currentContainer.Height * alpha;
                            }
                        }

                        _smoothedContainer = new Rectangle((int)_smoothedX, (int)_smoothedY, (int)_smoothedW, (int)_smoothedH);
                    }

                    _smoothedContainer = ClampRectangle(_smoothedContainer, image.Size);

                    if (_isBackgroundActive && !_smoothedContainer.IsEmpty)
                    {
                        using (Mat blurredBg = new Mat())
                        using (Mat mask = new Mat(image.Size, DepthType.Cv8U, 1))
                        {
                            CvInvoke.GaussianBlur(image, blurredBg, new Size(45, 45), 0);
                            mask.SetTo(new MCvScalar(0));
                            CvInvoke.Rectangle(mask, _smoothedContainer, new MCvScalar(255), -1);

                            image.CopyTo(blurredBg, mask);
                            blurredBg.CopyTo(image);
                        }
                    }

                    if (_smoothedContainer.IsEmpty) return 0.0;

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

                    if (_smoothedWaterY < 0)
                    {
                        _smoothedWaterY = currentWaterY;
                    }
                    else
                    {
                        if (Math.Abs(currentWaterY - _smoothedWaterY) > 3.0)
                        {
                            _smoothedWaterY = _smoothedWaterY * 0.88 + currentWaterY * 0.12;
                        }
                    }

                    _smoothedWaterY = Math.Max(_smoothedContainer.Top, Math.Min(_smoothedContainer.Bottom, _smoothedWaterY));

                    if (_showOverlay && !_smoothedContainer.IsEmpty)
                    {
                        CvInvoke.Rectangle(image, _smoothedContainer, new MCvScalar(0, 220, 0), 2);
                        CvInvoke.Line(image, 
                            new Point(0, (int)_smoothedWaterY), 
                            new Point(image.Width - 1, (int)_smoothedWaterY), 
                            new MCvScalar(0, 0, 255), 3);
                    }

                    double waterPixels = _smoothedContainer.Bottom - _smoothedWaterY;
                    double scaleLevel = (waterPixels / (double)_smoothedContainer.Height) * 10.0;
                    return Math.Min(10.0, Math.Max(0.0, scaleLevel));
                }
            }
        }

        private Rectangle ClampRectangle(Rectangle rect, Size imageSize)
        {
            int x = Math.Max(0, Math.Min(rect.X, imageSize.Width - 1));
            int y = Math.Max(0, Math.Min(rect.Y, imageSize.Height - 1));
            int width = Math.Max(1, Math.Min(rect.Width, imageSize.Width - x));
            int height = Math.Max(1, Math.Min(rect.Height, imageSize.Height - y));
            return new Rectangle(x, y, width, height);
        }

        private BitmapImage ConvertMatToBitmapImage(Mat mat)
        {
            using (VectorOfByte buffer = new VectorOfByte())
            {
                CvInvoke.Imencode(".bmp", mat, buffer);
                using (MemoryStream stream = new MemoryStream(buffer.ToArray()))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    return bitmapImage;
                }
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

                    // DAY 26: Logic so sánh liên tục ngưỡng nhiệt độ từ Serial
                    if (double.TryParse(tempStr, out double parsedTemp))
                    {
                        _currentTemp = parsedTemp;
                        _isTempAlarm = _currentTemp >= _tempThreshold;
                    }
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_blinkTimer != null) _blinkTimer.Stop();
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