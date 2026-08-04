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

        // ================= XỬ LÝ CỤM 3: CAMERA & NÚT ĐIỀU KHIỂN =================
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

        // Nút: Khử màu (Trắng/Đen)
        private void btnToggleGrayscale_Click(object sender, RoutedEventArgs e)
        {
            _isGrayscale = !_isGrayscale;
            btnToggleGrayscale.Content = _isGrayscale ? "Hiện màu nguyên bản" : "Khử màu (Trắng/Đen)";
            btnToggleGrayscale.Background = _isGrayscale ? MediaBrushes.DarkGray : (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#FF7E57C2");
        }

        // Nút: Hiển thị Thước & Vạch Mực Nước
        private void btnToggleOverlay_Click(object sender, RoutedEventArgs e)
        {
            _showOverlay = !_showOverlay;
            btnToggleOverlay.Content = _showOverlay ? "Ẩn Thước & Vạch Mực Nước" : "Hiển thị Thước & Vạch Mực Nước";
            btnToggleOverlay.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom(_showOverlay ? "#FFE53935" : "#FF0288D1");
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

                        // Tính toán mực nước và vẽ thước lên Card Video
                        double waterHeightCm = ProcessContainerAndWaterLevel(processedFrame);

                        // Cập nhật CỤM 2: Card Mực nước
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            txtWaterLevel.Text = $"{waterHeightCm:F1} cm";
                        }));

                        // Cập nhật CỤM 3: Card Khung Video Camera
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
            using (Mat gray = new Mat())
            using (Mat blurred = new Mat())
            using (Mat edges = new Mat())
            {
                CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
                CvInvoke.GaussianBlur(gray, blurred, new Size(5, 5), 0);
                CvInvoke.Canny(blurred, edges, 50, 150);

                Rectangle bestContainer = Rectangle.Empty;
                double maxArea = 0;

                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(edges, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                    for (int i = 0; i < contours.Size; i++)
                    {
                        Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                        double area = rect.Width * rect.Height;

                        if (rect.Height > 80 && rect.Width > 40 && rect.Height > rect.Width && area > maxArea)
                        {
                            maxArea = area;
                            bestContainer = rect;
                        }
                    }
                }

                if (bestContainer.IsEmpty)
                {
                    if (_showOverlay)
                    {
                        CvInvoke.PutText(image, "Khong tim thay binh nuoc...", new Point(20, 40),
                            FontFace.HersheySimplex, 0.6, new MCvScalar(0, 165, 255), 2);
                    }
                    return 0.0;
                }

                LineSegment2D[] lines = CvInvoke.HoughLinesP(edges, 1, Math.PI / 180, 30, 30, 10);
                double waterY = bestContainer.Bottom;

                foreach (var line in lines)
                {
                    if (line.P1.X >= bestContainer.Left - 10 && line.P2.X <= bestContainer.Right + 10 &&
                        Math.Abs(line.P1.Y - line.P2.Y) < 12)
                    {
                        double currentY = (line.P1.Y + line.P2.Y) / 2.0;
                        if (currentY > bestContainer.Top && currentY < bestContainer.Bottom)
                        {
                            waterY = currentY;
                            break;
                        }
                    }
                }

                // Vẽ thước đo & vạch nước khi bấm nút ở Cụm 3
                if (_showOverlay)
                {
                    CvInvoke.Line(image, new Point(bestContainer.Left, bestContainer.Top), new Point(bestContainer.Left, bestContainer.Bottom), new MCvScalar(255, 255, 0), 3);
                    CvInvoke.Line(image, new Point(bestContainer.Right, bestContainer.Top), new Point(bestContainer.Right, bestContainer.Bottom), new MCvScalar(255, 255, 0), 3);
                    CvInvoke.Line(image, new Point(bestContainer.Left, (int)waterY), new Point(bestContainer.Right, (int)waterY), new MCvScalar(0, 0, 255), 3);

                    int rulerX = Math.Max(40, bestContainer.Left - 20);
                    CvInvoke.Line(image, new Point(rulerX, bestContainer.Bottom), new Point(rulerX, bestContainer.Top), new MCvScalar(0, 255, 255), 2);

                    for (int cm = 0; cm <= (int)MAX_WATER_HEIGHT_CM; cm += 5)
                    {
                        double tickY = bestContainer.Bottom - ((double)cm / MAX_WATER_HEIGHT_CM) * bestContainer.Height;
                        CvInvoke.Line(image, new Point(rulerX - 6, (int)tickY), new Point(rulerX, (int)tickY), new MCvScalar(0, 255, 255), 2);
                        CvInvoke.PutText(image, $"{cm}cm", new Point(rulerX - 38, (int)tickY + 4),
                            FontFace.HersheySimplex, 0.4, new MCvScalar(255, 255, 255), 1);
                    }

                    CvInvoke.Circle(image, new Point(rulerX, (int)waterY), 5, new MCvScalar(0, 0, 255), -1);
                }

                double waterHeightCm = ((bestContainer.Bottom - waterY) / bestContainer.Height) * MAX_WATER_HEIGHT_CM;
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

        // ================= XỬ LÝ CỤM 1: KẾT NỐI SERIAL & CỤM 4: STATUSBAR =================
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

                // Cập nhật CỤM 4: Thanh trạng thái
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

                // Cập nhật CỤM 4: Thanh trạng thái
                txtStatus.Text = "Trạng thái: Đã ngắt kết nối";
                txtStatus.Foreground = MediaBrushes.Gray;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi ngắt kết nối: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================= CẬP NHẬT CỤM 2: CARD NHIỆT ĐỘ & ĐỘ ẨM =================
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

                    // Ghi dữ liệu vào Card Nhiệt độ & Card Độ ẩm
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