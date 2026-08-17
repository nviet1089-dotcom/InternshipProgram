#nullable disable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MediaBrushes = System.Windows.Media.Brushes;
using Path = System.IO.Path;

using Point = System.Drawing.Point;
using PointF = System.Drawing.PointF;
using Size = System.Drawing.Size;
using System.Drawing; // Dùng cho System.Drawing.Rectangle của Emgu CV

namespace WpfSensorApp
{
    public class LogModel
    {
        public DateTime Timestamp { get; set; }
        public string TimeStr { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double WaterLevel { get; set; }
    }

    public partial class MainWindow : Window
    {
        private SerialPort _serialPort;
        private VideoCapture _capture;
        private SensorLogger _sensorLogger;

        private bool _isGrayscale = false;
        private bool _showOverlay = false;
        private bool _isBackgroundActive = false;
        private bool _isDarkMode = false;
        private bool _isViewActive = false;

        private double _currentScaleRatio = 0.0;

        private double _smoothedWaterY = -1;
        private double _lastDetectedWaterY = -1;
        private System.Drawing.Rectangle _smoothedContainer = System.Drawing.Rectangle.Empty;

        private double _smoothedX = -1;
        private double _smoothedY = -1;
        private double _smoothedW = -1;
        private double _smoothedH = -1;

        private int _frameCounter = 0;
        private const double MAX_WATER_HEIGHT_CM = 20.0;

        // BIẾN NGƯỠNG VÀ CẢNH BÁO
        private double _tempThreshold = 35.0;
        private double _humThreshold = 80.0;
        private double _waterThreshold = 8.0;

        private double _currentTemp = 0.0;
        private double _currentHum = 0.0;
        private double _currentWaterScale = 0.0;

        private bool _isTempAlarm = false;
        private bool _isHumAlarm = false;
        private bool _isWaterAlarm = false;

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

            // Khởi tạo Logger lưu dữ liệu định kỳ 5 phút/lần vào file CSV
            _sensorLogger = new SensorLogger(() => (
                ViewModel.Temperature,
                ViewModel.Humidity,
                ViewModel.WaterLevel
            ));
            _sensorLogger.Start();
        }

        private void btnCheckLogger_Click(object sender, RoutedEventArgs e)
        {
            ShowLogWindow();
        }

        #region GIAO DIỆN NHẬT KÝ & BIỂU ĐỒ CHUYÊN NGHIỆP (LOG & GRAPH WINDOW)

        private void ShowLogWindow()
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "sensor_data_log.csv");

            if (!File.Exists(logFilePath))
            {
                MessageBox.Show("Chưa có dữ liệu nhật ký được lưu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            List<LogModel> allLogs = LoadLogData(logFilePath);

            BrushConverter bc = new BrushConverter();
            bool isLoggerDarkMode = _isDarkMode; // Thừa hưởng cấu hình theme ban đầu

            Window logWindow = new Window
            {
                Title = "NHẬT KÝ VÀ BIỂU ĐỒ LỊCH SỬ DỮ LIỆU CẢM BIẾN",
                Width = 960,
                Height = 780,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = isLoggerDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#121212") : (System.Windows.Media.Brush)bc.ConvertFrom("#F4F6F9")
            };

            Grid mainGrid = new Grid { Margin = new Thickness(16) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Khung điều khiển & Bộ lọc
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Khung biểu đồ

            // 1. KHUNG BỘ LỌC THỜI GIAN & NÚT GẠT ĐỔI MÀU NỀN
            Border headerCard = new Border
            {
                Background = isLoggerDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#1E1E1E") : MediaBrushes.White,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 12),
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 10, ShadowDepth = 2, Opacity = 0.08 }
            };

            Grid headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Cụm 3 nút lọc bấm màu xanh lam (#2196F3)
            StackPanel timerPanel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            Button btnDay = CreateModernButton("DAY", "#2196F3");
            Button btnWeek = CreateModernButton("WEEK", "#2196F3");
            Button btnMonth = CreateModernButton("MONTH", "#2196F3");

            timerPanel.Children.Add(btnDay);
            timerPanel.Children.Add(btnWeek);
            timerPanel.Children.Add(btnMonth);

            // Nút gạt (Toggle Switch) đổi màu nền Logger đặt bên cạnh 3 nút lọc
            CheckBox toggleLoggerTheme = new CheckBox
            {
                Style = (Style)FindResource("PhoneToggleSwitchStyle"),
                IsChecked = isLoggerDarkMode,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 5, 0)
            };

            TextBlock lblLoggerTheme = new TextBlock
            {
                Text = isLoggerDarkMode ? "🌙 Nền Tối" : "☀️ Nền Sáng",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = isLoggerDarkMode ? MediaBrushes.White : (System.Windows.Media.Brush)bc.ConvertFrom("#333333"),
                VerticalAlignment = VerticalAlignment.Center
            };
            toggleLoggerTheme.Content = lblLoggerTheme;

            headerGrid.Children.Add(timerPanel);
            Grid.SetColumn(timerPanel, 0);
            headerGrid.Children.Add(toggleLoggerTheme);
            Grid.SetColumn(toggleLoggerTheme, 1);

            headerCard.Child = headerGrid;
            Grid.SetRow(headerCard, 0);

            // 2. KHUNG HIỂN THỊ BIỂU ĐỒ
            Border chartCard = new Border
            {
                Background = isLoggerDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#1E1E1E") : MediaBrushes.White,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 10, ShadowDepth = 2, Opacity = 0.08 }
            };

            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            StackPanel chartStack = new StackPanel();

            System.Windows.Media.Brush canvasBg = isLoggerDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#252526") : (System.Windows.Media.Brush)bc.ConvertFrom("#FAFAFA");

            Canvas canvasWater = new Canvas { Height = 180, Background = canvasBg, Margin = new Thickness(0, 0, 0, 16) };
            Canvas canvasTemp = new Canvas { Height = 180, Background = canvasBg, Margin = new Thickness(0, 0, 0, 16) };
            Canvas canvasHum = new Canvas { Height = 180, Background = canvasBg, Margin = new Thickness(0, 0, 0, 5) };

            chartStack.Children.Add(canvasWater);
            chartStack.Children.Add(canvasTemp);
            chartStack.Children.Add(canvasHum);

            scrollViewer.Content = chartStack;
            chartCard.Child = scrollViewer;
            Grid.SetRow(chartCard, 1);

            mainGrid.Children.Add(headerCard);
            mainGrid.Children.Add(chartCard);
            logWindow.Content = mainGrid;

            // Cập nhật giao diện động theo nút gạt
            Action applyThemeColors = () =>
            {
                System.Windows.Media.Brush bg = isLoggerDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#121212") : (System.Windows.Media.Brush)bc.ConvertFrom("#F4F6F9");
                System.Windows.Media.Brush cardBg = isLoggerDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#1E1E1E") : MediaBrushes.White;
                System.Windows.Media.Brush cBg = isLoggerDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#252526") : (System.Windows.Media.Brush)bc.ConvertFrom("#FAFAFA");

                logWindow.Background = bg;
                headerCard.Background = cardBg;
                chartCard.Background = cardBg;
                canvasWater.Background = cBg;
                canvasTemp.Background = cBg;
                canvasHum.Background = cBg;

                lblLoggerTheme.Text = isLoggerDarkMode ? "🌙 Nền Tối" : "☀️ Nền Sáng";
                lblLoggerTheme.Foreground = isLoggerDarkMode ? MediaBrushes.White : (System.Windows.Media.Brush)bc.ConvertFrom("#333333");
            };

            // Hàm cập nhật dữ liệu và vẽ biểu đồ với danh xưng chính quy
            Action<string> updateCharts = (mode) =>
            {
                DateTime now = DateTime.Now;
                List<LogModel> filtered = new List<LogModel>();

                if (mode == "DAY")
                    filtered = allLogs.Where(x => x.Timestamp >= now.AddDays(-1)).ToList();
                else if (mode == "WEEK")
                    filtered = allLogs.Where(x => x.Timestamp >= now.AddDays(-7)).ToList();
                else if (mode == "MONTH")
                    filtered = allLogs.Where(x => x.Timestamp >= now.AddDays(-30)).ToList();

                System.Windows.Media.Brush textBrush = isLoggerDarkMode ? MediaBrushes.White : (System.Windows.Media.Brush)bc.ConvertFrom("#212121");

                // 1. Biểu đồ Mực nước - Cột màu xanh nước biển (#0288D1)
                DrawBarChart(canvasWater, filtered.Select(x => x.WaterLevel).ToList(),
                             filtered.Select(x => x.TimeStr).ToList(),
                             "BIỂU ĐỒ GIÁM SÁT MỰC NƯỚC THEO THỜI GIAN THỰC (ĐƠN VỊ: CM)",
                             (System.Windows.Media.Brush)bc.ConvertFrom("#0288D1"), 20.0, textBrush);

                // 2. Biểu đồ Nhiệt độ - Đường màu đỏ (#D32F2F)
                DrawLineChart(canvasTemp, filtered.Select(x => x.Temperature).ToList(),
                              filtered.Select(x => x.TimeStr).ToList(),
                              "BIỂU ĐỒ GIÁM SÁT NHIỆT ĐỘ MÔI TRƯỜNG (ĐƠN VỊ: °C)",
                              (System.Windows.Media.Brush)bc.ConvertFrom("#D32F2F"), 50.0, textBrush);

                // 3. Biểu đồ Độ ẩm - Đường màu xanh lá cây (#388E3C)
                DrawLineChart(canvasHum, filtered.Select(x => x.Humidity).ToList(),
                              filtered.Select(x => x.TimeStr).ToList(),
                              "BIỂU ĐỒ GIÁM SÁT ĐỘ ẨM KHÔNG KHÍ (ĐƠN VỊ: %)",
                              (System.Windows.Media.Brush)bc.ConvertFrom("#388E3C"), 100.0, textBrush);
            };

            btnDay.Click += (s, e) => updateCharts("DAY");
            btnWeek.Click += (s, e) => updateCharts("WEEK");
            btnMonth.Click += (s, e) => updateCharts("MONTH");

            toggleLoggerTheme.Click += (s, e) =>
            {
                isLoggerDarkMode = toggleLoggerTheme.IsChecked ?? false;
                applyThemeColors();
                updateCharts("DAY");
            };

            logWindow.Loaded += (s, e) => updateCharts("DAY");
            logWindow.ShowDialog();
        }

        // Tạo Button giao diện phẳng bo góc Modern UI
        private Button CreateModernButton(string content, string hexColor)
        {
            BrushConverter bc = new BrushConverter();
            Button btn = new Button
            {
                Content = content,
                Width = 110,
                Height = 34,
                Margin = new Thickness(0, 0, 10, 0),
                Background = (System.Windows.Media.Brush)bc.ConvertFrom(hexColor),
                Foreground = MediaBrushes.White,
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));

            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(presenter);
            template.VisualTree = border;
            btn.Template = template;

            return btn;
        }

        private List<LogModel> LoadLogData(string filePath)
        {
            var list = new List<LogModel>();
            try
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    string[] parts = lines[i].Split(',');
                    if (parts.Length >= 4)
                    {
                        if (DateTime.TryParseExact(parts[0].Trim(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                        {
                            double.TryParse(parts[1].Trim().Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double t);
                            double.TryParse(parts[2].Trim().Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double h);
                            double.TryParse(parts[3].Trim().Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double w);

                            list.Add(new LogModel
                            {
                                Timestamp = dt,
                                TimeStr = dt.ToString("HH:mm dd/MM"),
                                Temperature = t,
                                Humidity = h,
                                WaterLevel = w
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi đọc file log: {ex.Message}");
            }
            return list;
        }

        // HÀM VẼ BIỂU ĐỒ CỘT (BAR CHART - MỰC NƯỚC)
        private void DrawBarChart(Canvas canvas, List<double> values, List<string> timeLabels, string title, System.Windows.Media.Brush barBrush, double maxY, System.Windows.Media.Brush textBrush)
        {
            canvas.Children.Clear();
            double width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 880;
            double height = canvas.ActualHeight > 0 ? canvas.ActualHeight : 180;
            double padLeft = 40, padBottom = 30, padTop = 30, padRight = 20;

            // Tiêu đề biểu đồ chuẩn chính quy
            TextBlock txtTitle = new TextBlock { Text = title, FontWeight = FontWeights.Bold, Foreground = textBrush, FontSize = 12 };
            Canvas.SetLeft(txtTitle, 10);
            Canvas.SetTop(txtTitle, 6);
            canvas.Children.Add(txtTitle);

            // Trục tọa độ
            System.Windows.Shapes.Line xAxis = new System.Windows.Shapes.Line { X1 = padLeft, Y1 = height - padBottom, X2 = width - padRight, Y2 = height - padBottom, Stroke = MediaBrushes.Gray, StrokeThickness = 1 };
            System.Windows.Shapes.Line yAxis = new System.Windows.Shapes.Line { X1 = padLeft, Y1 = padTop, X2 = padLeft, Y2 = height - padBottom, Stroke = MediaBrushes.Gray, StrokeThickness = 1 };
            canvas.Children.Add(xAxis);
            canvas.Children.Add(yAxis);

            if (values == null || values.Count == 0) return;

            double drawWidth = width - padLeft - padRight;
            double drawHeight = height - padTop - padBottom;
            int count = values.Count;
            double barWidth = Math.Max(2, Math.Min(25, (drawWidth / count) - 4));

            for (int i = 0; i < count; i++)
            {
                double val = Math.Min(maxY, Math.Max(0, values[i]));
                double barHeight = (val / maxY) * drawHeight;
                double x = padLeft + i * (drawWidth / count) + 2;
                double y = height - padBottom - barHeight;

                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = barBrush,
                    ToolTip = $"Thời gian: {timeLabels[i]}\nMực nước: {values[i]:F1} cm"
                };
                Canvas.SetLeft(rect, (int)x);
                Canvas.SetTop(rect, (int)y);
                canvas.Children.Add(rect);
            }
        }

        // HÀM VẼ BIỂU ĐỒ ĐƯỜNG (LINE CHART - NHIỆT ĐỘ & ĐỘ ẨM)
        private void DrawLineChart(Canvas canvas, List<double> values, List<string> timeLabels, string title, System.Windows.Media.Brush lineBrush, double maxY, System.Windows.Media.Brush textBrush)
        {
            canvas.Children.Clear();
            double width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 880;
            double height = canvas.ActualHeight > 0 ? canvas.ActualHeight : 180;
            double padLeft = 40, padBottom = 30, padTop = 30, padRight = 20;

            // Tiêu đề biểu đồ chuẩn chính quy
            TextBlock txtTitle = new TextBlock { Text = title, FontWeight = FontWeights.Bold, Foreground = textBrush, FontSize = 12 };
            Canvas.SetLeft(txtTitle, 10);
            Canvas.SetTop(txtTitle, 6);
            canvas.Children.Add(txtTitle);

            // Trục tọa độ
            System.Windows.Shapes.Line xAxis = new System.Windows.Shapes.Line { X1 = padLeft, Y1 = height - padBottom, X2 = width - padRight, Y2 = height - padBottom, Stroke = MediaBrushes.Gray, StrokeThickness = 1 };
            System.Windows.Shapes.Line yAxis = new System.Windows.Shapes.Line { X1 = padLeft, Y1 = padTop, X2 = padLeft, Y2 = height - padBottom, Stroke = MediaBrushes.Gray, StrokeThickness = 1 };
            canvas.Children.Add(xAxis);
            canvas.Children.Add(yAxis);

            if (values == null || values.Count == 0) return;

            double drawWidth = width - padLeft - padRight;
            double drawHeight = height - padTop - padBottom;
            int count = values.Count;

            System.Windows.Shapes.Polyline polyline = new System.Windows.Shapes.Polyline { Stroke = lineBrush, StrokeThickness = 2 };
            System.Windows.Media.PointCollection points = new System.Windows.Media.PointCollection();

            for (int i = 0; i < count; i++)
            {
                double val = Math.Min(maxY, Math.Max(0, values[i]));
                double x = padLeft + (count == 1 ? drawWidth / 2 : i * (drawWidth / (count - 1)));
                double y = height - padBottom - (val / maxY) * drawHeight;

                points.Add(new System.Windows.Point(x, y));

                System.Windows.Shapes.Ellipse dot = new System.Windows.Shapes.Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = lineBrush,
                    ToolTip = $"Thời gian: {timeLabels[i]}\nGiá trị: {values[i]:F1}"
                };
                Canvas.SetLeft(dot, x - 3);
                Canvas.SetTop(dot, y - 3);
                canvas.Children.Add(dot);
            }

            polyline.Points = points;
            canvas.Children.Add(polyline);
        }

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComPorts();
            StartWebcam();

            Dispatcher.BeginInvoke(new Action(() => {
                UpdateDangerBar();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void UpdateDangerBar()
        {
            if (gridBarContainer == null || rectUnfilledOverlay == null) return;

            double totalBarHeight = gridBarContainer.ActualHeight;
            if (totalBarHeight <= 0) return;

            double ratio = _isViewActive ? _currentScaleRatio : 0.0;
            double unfilledRatio = 1.0 - Math.Min(1.0, Math.Max(0.0, ratio));
            rectUnfilledOverlay.Height = totalBarHeight * unfilledRatio;
        }

        private void gridBarContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDangerBar();
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            _isViewActive = !_isViewActive;

            if (_isViewActive)
            {
                btnView.Content = "👁️ View";
                btnView.Background = (System.Windows.Media.Brush)new BrushConverter().ConvertFrom("#FFE53935");
            }
            else
            {
                btnView.Content = "👁️ View";
                btnView.Background = (System.Windows.Media.Brush)new BrushConverter().ConvertFrom("#FF2196F3");

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ViewModel.WaterLevel = "--.-- cm";
                    ViewModel.DangerLevel = "0/10";

                    BrushConverter bc = new BrushConverter();
                    ViewModel.WaterLevelColor = (System.Windows.Media.Brush)bc.ConvertFrom("#FF0288D1");
                    ViewModel.WaterLevelBgColor = _isDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#0F2D3C") : (System.Windows.Media.Brush)bc.ConvertFrom("#FFE1F5FE");

                    _currentScaleRatio = 0.0;
                    UpdateDangerBar();
                }));
            }
        }

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

            System.Windows.Media.Brush redBrush = (System.Windows.Media.Brush)bc.ConvertFrom("#FFE53935");
            System.Windows.Media.Brush whiteBrush = _isDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#1E1E1E") : MediaBrushes.White;

            if (_isTempAlarm)
            {
                cardTemp.Background = _isBlinkStateToggle ? redBrush : whiteBrush;
            }
            else
            {
                cardTemp.Background = _isDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#1E2A1E") : (System.Windows.Media.Brush)bc.ConvertFrom("#FFE8F5E9");
            }

            if (_isHumAlarm)
            {
                cardHum.Background = _isBlinkStateToggle ? redBrush : whiteBrush;
            }
            else
            {
                cardHum.Background = _isDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#1E2A1E") : (System.Windows.Media.Brush)bc.ConvertFrom("#FFE8F5E9");
            }

            cardDanger.Background = _isDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#1E1E1E") : (System.Windows.Media.Brush)bc.ConvertFrom("#FAFAFA");
        }

        private void txtThreshold_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtTempThreshold != null)
            {
                string tempStr = txtTempThreshold.Text.Replace(',', '.');
                if (double.TryParse(tempStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double temp))
                {
                    _tempThreshold = temp;
                }
            }

            if (txtHumThreshold != null)
            {
                string humStr = txtHumThreshold.Text.Replace(',', '.');
                if (double.TryParse(humStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double hum))
                {
                    _humThreshold = hum;
                }
            }

            if (txtWaterThreshold != null)
            {
                string waterStr = txtWaterThreshold.Text.Replace(',', '.');
                if (double.TryParse(waterStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double water))
                {
                    _waterThreshold = water;
                }
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
            btnToggleGrayscale.Content = _isGrayscale ? "Tắt Ảnh Xám (Gray)" : "Bật Ảnh Xám (Gray)";
            btnToggleGrayscale.Background = _isGrayscale ? _colorRed : _colorGreen;
        }

        private void btnToggleBackground_Click(object sender, RoutedEventArgs e)
        {
            _isBackgroundActive = !_isBackgroundActive;
            btnToggleBackground.Content = _isBackgroundActive ? "Tắt Background" : "Bật Background";
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
                this.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#121212");
                grpSerial.Foreground = MediaBrushes.White;
                lblComPort.Foreground = MediaBrushes.White;
                lblDarkMode.Foreground = MediaBrushes.White;

                if (grpThresholds != null) grpThresholds.Foreground = MediaBrushes.White;
                if (lblTempThreshold != null) lblTempThreshold.Foreground = MediaBrushes.White;
                if (lblHumThreshold != null) lblHumThreshold.Foreground = MediaBrushes.White;
                if (lblWaterThreshold != null) lblWaterThreshold.Foreground = MediaBrushes.White;

                cardHum.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#1E2A1E");
                cardDanger.BorderBrush = (System.Windows.Media.Brush)bc.ConvertFrom("#333333");
                cardCamera.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#251A2C");
                cardCamera.BorderBrush = (System.Windows.Media.Brush)bc.ConvertFrom("#4A154B");

                rectUnfilledOverlay.Fill = (System.Windows.Media.Brush)bc.ConvertFrom("#1E1E1E");
                txtDangerLevel.Foreground = MediaBrushes.White;
                sbStatus.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#1E1E1E");
            }
            else
            {
                this.Background = MediaBrushes.White;
                grpSerial.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#333333");
                lblComPort.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#333333");
                lblDarkMode.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#333333");

                if (grpThresholds != null) grpThresholds.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#333333");
                if (lblTempThreshold != null) lblTempThreshold.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#333333");
                if (lblHumThreshold != null) lblHumThreshold.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#333333");
                if (lblWaterThreshold != null) lblWaterThreshold.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#333333");

                cardHum.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFE8F5E9");
                cardDanger.BorderBrush = (System.Windows.Media.Brush)bc.ConvertFrom("#E0E0E0");
                cardCamera.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFF3E5F5");
                cardCamera.BorderBrush = (System.Windows.Media.Brush)bc.ConvertFrom("#FFCE93D8");

                rectUnfilledOverlay.Fill = (System.Windows.Media.Brush)bc.ConvertFrom("#FAFAFA");
                txtDangerLevel.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#333333");
                sbStatus.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFF5F5F5");
            }
        }

        private void btnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_capture != null && _capture.IsOpened)
                {
                    using (Mat rawFrame = new Mat())
                    {
                        _capture.Retrieve(rawFrame);
                        if (rawFrame.IsEmpty) return;

                        Mat originalMat = rawFrame.Clone();
                        BitmapImage originalBitmap = ConvertMatToBitmapImage(originalMat);

                        Mat correctedMat = CorrectPerspectiveFrame(rawFrame);
                        BitmapImage correctedBitmap = ConvertMatToBitmapImage(correctedMat);

                        string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        string pathOrig = Path.Combine(folderPath, $"Original_Tilted_{timestamp}.png");
                        string pathCorrected = Path.Combine(folderPath, $"Corrected_Straight_{timestamp}.png");

                        SaveBitmapImageToFile(originalBitmap, pathOrig);
                        SaveBitmapImageToFile(correctedBitmap, pathCorrected);

                        ShowDualImagePreviewWindow(originalBitmap, correctedBitmap, pathOrig, pathCorrected);

                        originalMat.Dispose();
                        correctedMat.Dispose();
                    }
                }
                else
                {
                    MessageBox.Show("Không có hình ảnh từ Camera để chụp!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chụp và hiệu chỉnh ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Mat CorrectPerspectiveFrame(Mat src)
        {
            Mat corrected = src.Clone();

            using (Mat gray = new Mat())
            using (Mat edges = new Mat())
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.CvtColor(src, gray, ColorConversion.Bgr2Gray);
                CvInvoke.GaussianBlur(gray, gray, new Size(5, 5), 0);
                CvInvoke.Canny(gray, edges, 35, 110);

                CvInvoke.FindContours(edges, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                RotatedRect maxRotatedRect = new RotatedRect();
                double maxArea = 0;
                double minAreaThreshold = src.Width * src.Height * 0.008;

                for (int i = 0; i < contours.Size; i++)
                {
                    RotatedRect rRect = CvInvoke.MinAreaRect(contours[i]);
                    double area = rRect.Size.Width * rRect.Size.Height;

                    if (area > minAreaThreshold && area > maxArea)
                    {
                        maxArea = area;
                        maxRotatedRect = rRect;
                    }
                }

                if (maxArea > 0)
                {
                    PointF[] srcPts = maxRotatedRect.GetVertices();
                    PointF[] orderedPts = OrderPoints(srcPts);

                    float width = maxRotatedRect.Size.Width;
                    float height = maxRotatedRect.Size.Height;

                    if (width > height && maxRotatedRect.Angle < -45)
                    {
                        float temp = width;
                        width = height;
                        height = temp;
                    }

                    if (width < 10 || height < 10) return corrected;

                    PointF[] dstPts = new PointF[]
                    {
                        new PointF(0, 0),
                        new PointF(width - 1, 0),
                        new PointF(width - 1, height - 1),
                        new PointF(0, height - 1)
                    };

                    using (Mat M = CvInvoke.GetPerspectiveTransform(orderedPts, dstPts))
                    {
                        Mat warped = new Mat();
                        CvInvoke.WarpPerspective(src, warped, M, new Size((int)width, (int)height));
                        return warped;
                    }
                }
            }

            return corrected;
        }

        private PointF[] OrderPoints(PointF[] pts)
        {
            PointF[] ordered = new PointF[4];

            var sumList = pts.Select(p => p.X + p.Y).ToArray();
            var diffList = pts.Select(p => p.Y - p.X).ToArray();

            ordered[0] = pts[Array.IndexOf(sumList, sumList.Min())];
            ordered[2] = pts[Array.IndexOf(sumList, sumList.Max())];
            ordered[1] = pts[Array.IndexOf(diffList, diffList.Min())];
            ordered[3] = pts[Array.IndexOf(diffList, diffList.Max())];

            return ordered;
        }

        private void ShowDualImagePreviewWindow(BitmapSource origImg, BitmapSource correctedImg, string origPath, string correctedPath)
        {
            Window previewWindow = new Window
            {
                Title = $"So Sánh Ảnh Chụp Camera - [{Path.GetFileName(origPath)}]",
                Width = 1000,
                Height = 550,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = (System.Windows.Media.Brush)new BrushConverter().ConvertFrom("#121212")
            };

            Grid mainGrid = new Grid { Margin = new Thickness(10) };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            GroupBox grpOriginal = new GroupBox
            {
                Header = "📷 ÁNH GỐC CAMERA (BỊ NGHIÊNG)",
                Foreground = MediaBrushes.OrangeRed,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5)
            };
            System.Windows.Controls.Image imgOrigControl = new System.Windows.Controls.Image
            {
                Source = origImg,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5)
            };
            grpOriginal.Content = imgOrigControl;
            Grid.SetColumn(grpOriginal, 0);

            GroupBox grpCorrected = new GroupBox
            {
                Header = "✨ ÁNH ĐÃ HIỆU CHỈNH (PERSPECTIVE CORRECTION)",
                Foreground = MediaBrushes.LimeGreen,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5)
            };
            System.Windows.Controls.Image imgCorrectedControl = new System.Windows.Controls.Image
            {
                Source = correctedImg,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5)
            };
            grpCorrected.Content = imgCorrectedControl;
            Grid.SetColumn(grpCorrected, 1);

            mainGrid.Children.Add(grpOriginal);
            mainGrid.Children.Add(grpCorrected);

            previewWindow.Content = mainGrid;
            previewWindow.Show();
        }

        private void SaveBitmapImageToFile(BitmapSource bitmap, string filePath)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(stream);
            }
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

                    _isWaterAlarm = _isViewActive && (_currentWaterScale >= _waterThreshold);

                    double scaleRatio = scaleLevel / 10.0;
                    _currentScaleRatio = scaleRatio;

                    double waterHeightCm = scaleRatio * MAX_WATER_HEIGHT_CM;

                    BitmapImage bitmapImage = ConvertMatToBitmapImage(processedFrame);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (_isViewActive)
                        {
                            ViewModel.WaterLevel = $"{waterHeightCm:F1} cm";
                            ViewModel.DangerLevel = $"Scale: {scaleLevel:F1} / 10";

                            BrushConverter bc = new BrushConverter();

                            if (scaleLevel >= 8.0)
                            {
                                ViewModel.WaterLevelColor = (System.Windows.Media.Brush)bc.ConvertFrom("#FFE53935");
                                ViewModel.WaterLevelBgColor = _isDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#3E0F0F") : (System.Windows.Media.Brush)bc.ConvertFrom("#FFFFEBEE");
                            }
                            else if (scaleLevel >= 5.0)
                            {
                                ViewModel.WaterLevelColor = (System.Windows.Media.Brush)bc.ConvertFrom("#FFF57F17");
                                ViewModel.WaterLevelBgColor = _isDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#3E2E04") : (System.Windows.Media.Brush)bc.ConvertFrom("#FFFDE0B2");
                            }
                            else if (scaleLevel >= 2.5)
                            {
                                ViewModel.WaterLevelColor = (System.Windows.Media.Brush)bc.ConvertFrom("#FF4CAF50");
                                ViewModel.WaterLevelBgColor = _isDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#0F3E18") : (System.Windows.Media.Brush)bc.ConvertFrom("#FFE8F5E9");
                            }
                            else
                            {
                                ViewModel.WaterLevelColor = (System.Windows.Media.Brush)bc.ConvertFrom("#FF0288D1");
                                ViewModel.WaterLevelBgColor = _isDarkMode ? (System.Windows.Media.Brush)bc.ConvertFrom("#0F2D3C") : (System.Windows.Media.Brush)bc.ConvertFrom("#FFE1F5FE");
                            }

                            UpdateDangerBar();
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

                    System.Drawing.Rectangle currentContainer = System.Drawing.Rectangle.Empty;
                    double minDistanceToCamera = double.MaxValue;
                    double minAreaThreshold = image.Width * image.Height * 0.008;

                    Point cameraAnchor = new Point(image.Width / 2, image.Height);

                    using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                    {
                        CvInvoke.FindContours(edges, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                        for (int i = 0; i < contours.Size; i++)
                        {
                            System.Drawing.Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                            double area = rect.Width * rect.Height;

                            if (rect.X <= 2 || rect.Y <= 2 || rect.Right >= image.Width - 2 || rect.Bottom >= image.Height - 2)
                                continue;

                            if (area > minAreaThreshold)
                            {
                                Point rectBottomCenter = new Point(rect.X + rect.Width / 2, rect.Bottom);
                                double dist = Math.Sqrt(Math.Pow(rectBottomCenter.X - cameraAnchor.X, 2) + Math.Pow(rectBottomCenter.Y - cameraAnchor.Y, 2));

                                if (dist < minDistanceToCamera)
                                {
                                    minDistanceToCamera = dist;
                                    currentContainer = rect;
                                }
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
                            double alphaPos = 0.03;
                            double alphaSize = 0.025;

                            _smoothedX = _smoothedX * (1.0 - alphaPos) + currentContainer.X * alphaPos;
                            _smoothedY = _smoothedY * (1.0 - alphaPos) + currentContainer.Y * alphaPos;
                            _smoothedW = _smoothedW * (1.0 - alphaSize) + currentContainer.Width * alphaSize;
                            _smoothedH = _smoothedH * (1.0 - alphaSize) + currentContainer.Height * alphaSize;
                        }

                        _smoothedContainer = new System.Drawing.Rectangle((int)_smoothedX, (int)_smoothedY, (int)_smoothedW, (int)_smoothedH);
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

                    if (_frameCounter % 10 == 0 || _lastDetectedWaterY < 0)
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
                        _smoothedWaterY = _smoothedWaterY * 0.985 + currentWaterY * 0.015;
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

        private System.Drawing.Rectangle ClampRectangle(System.Drawing.Rectangle rect, Size imageSize)
        {
            int x = Math.Max(0, Math.Min(rect.X, imageSize.Width - 1));
            int y = Math.Max(0, Math.Min(rect.Y, imageSize.Height - 1));
            int width = Math.Max(1, Math.Min(rect.Width, imageSize.Width - x));
            int height = Math.Max(1, Math.Min(rect.Height, imageSize.Height - y));
            return new System.Drawing.Rectangle(x, y, width, height);
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

                    if (double.TryParse(tempStr.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedTemp))
                    {
                        _currentTemp = parsedTemp;
                        _isTempAlarm = _currentTemp >= _tempThreshold;
                    }

                    if (double.TryParse(humStr.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedHum))
                    {
                        _currentHum = parsedHum;
                        _isHumAlarm = _currentHum >= _humThreshold;
                    }
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _sensorLogger?.Stop();
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