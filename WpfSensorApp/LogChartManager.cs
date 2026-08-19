using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfSensorApp
{
    // Lớp mô hình dữ liệu Log
    public class LogModel
    {
        public DateTime Timestamp { get; set; }
        public string TimeStr { get; set; } = "";
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double WaterLevel { get; set; }
    }

    public static class LogChartManager
    {
        // Overload 1: Hỗ trợ delegate 1 tham số
        public static void ShowLogWindow(Window ownerWindow, bool isDarkMode, Func<string, Button> createModernButtonFunc)
        {
            ShowLogWindow(ownerWindow, isDarkMode, (text, color) => createModernButtonFunc(text));
        }

        // Overload 2: Hỗ trợ delegate 2 tham số (tương thích với MainWindow)
        public static void ShowLogWindow(Window ownerWindow, bool isDarkMode, Func<string, string, Button> createModernButtonFunc)
        {
            string logFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "sensor_data_log.csv");

            if (!System.IO.File.Exists(logFilePath))
            {
                MessageBox.Show("Chưa có dữ liệu nhật ký được lưu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            List<LogModel> allLogs = LoadLogData(logFilePath);

            BrushConverter bc = new BrushConverter();
            bool isLoggerDarkMode = isDarkMode;
            string currentMode = "DAY";

            Window logWindow = new Window
            {
                Title = "TRUY XUẤT VÀ KIỂM TRA LỊCH SỬ DỮ LIỆU CẢM BIẾN",
                Width = 1200,
                Height = 850,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = ownerWindow,
                Background = (Brush)(bc.ConvertFrom(isLoggerDarkMode ? "#121212" : "#F4F6F9") ?? Brushes.White)
            };

            // Grid chính: Dòng 1 (1/7), Dòng 2 (6/7)
            Grid mainGrid = new Grid { Margin = new Thickness(12) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(6, GridUnitType.Star) });

            // ==========================================
            // 1. THANH NGANG TRÊN CÙNG (1/7)
            // ==========================================
            Border headerCard = new Border
            {
                Background = (Brush)(bc.ConvertFrom(isLoggerDarkMode ? "#1E1E1E" : "#FFFFFF") ?? Brushes.White),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 0, 10),
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 10, ShadowDepth = 2, Opacity = 0.08 }
            };

            Grid headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            UniformGrid timerGrid = new UniformGrid
            {
                Rows = 1,
                Columns = 3,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 20, 0)
            };

            // FIX 1: Truyền mã màu hex mặc định "#2196F3" thay vì chuỗi rỗng ""
            Button btnDay = createModernButtonFunc("DAY", "#2196F3");
            Button btnWeek = createModernButtonFunc("WEEK", "#2196F3");
            Button btnMonth = createModernButtonFunc("MONTH", "#2196F3");

            btnDay.Width = double.NaN;
            btnWeek.Width = double.NaN;
            btnMonth.Width = double.NaN;
            btnDay.HorizontalAlignment = HorizontalAlignment.Stretch;
            btnWeek.HorizontalAlignment = HorizontalAlignment.Stretch;
            btnMonth.HorizontalAlignment = HorizontalAlignment.Stretch;

            btnDay.Margin = new Thickness(0, 0, 5, 0);
            btnWeek.Margin = new Thickness(5, 0, 5, 0);
            btnMonth.Margin = new Thickness(5, 0, 0, 0);

            timerGrid.Children.Add(btnDay);
            timerGrid.Children.Add(btnWeek);
            timerGrid.Children.Add(btnMonth);

            CheckBox toggleLoggerTheme = new CheckBox
            {
                Style = (Style)ownerWindow.FindResource("PhoneToggleSwitchStyle"),
                IsChecked = isLoggerDarkMode,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 5, 0)
            };

            TextBlock lblLoggerTheme = new TextBlock
            {
                Text = isLoggerDarkMode ? "🌙 Nền Tối" : "☀️ Nền Sáng",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = isLoggerDarkMode ? Brushes.White : (Brush)(bc.ConvertFrom("#333333") ?? Brushes.Black),
                VerticalAlignment = VerticalAlignment.Center
            };
            toggleLoggerTheme.Content = lblLoggerTheme;

            headerGrid.Children.Add(timerGrid);
            Grid.SetColumn(timerGrid, 0);
            headerGrid.Children.Add(toggleLoggerTheme);
            Grid.SetColumn(toggleLoggerTheme, 1);

            headerCard.Child = headerGrid;
            Grid.SetRow(headerCard, 0);

            // ==========================================
            // 2. PHẦN THÂN DƯỚI (6/7)
            // ==========================================
            Grid contentGrid = new Grid();
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(7, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

            // Cột trái (7/10)
            Border chartCard = new Border
            {
                Background = (Brush)(bc.ConvertFrom(isLoggerDarkMode ? "#1E1E1E" : "#FFFFFF") ?? Brushes.White),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 8, 0),
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 10, ShadowDepth = 2, Opacity = 0.08 }
            };

            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            StackPanel chartStack = new StackPanel();
            Brush canvasBg = (Brush)(bc.ConvertFrom(isLoggerDarkMode ? "#252526" : "#FAFAFA") ?? Brushes.White);

            Canvas canvasWater = new Canvas { Height = 210, Background = canvasBg, Margin = new Thickness(0, 0, 0, 12), HorizontalAlignment = HorizontalAlignment.Stretch };
            Canvas canvasTemp = new Canvas { Height = 210, Background = canvasBg, Margin = new Thickness(0, 0, 0, 12), HorizontalAlignment = HorizontalAlignment.Stretch };
            Canvas canvasHum = new Canvas { Height = 210, Background = canvasBg, Margin = new Thickness(0, 0, 0, 5), HorizontalAlignment = HorizontalAlignment.Stretch };

            chartStack.Children.Add(canvasWater);
            chartStack.Children.Add(canvasTemp);
            chartStack.Children.Add(canvasHum);

            scrollViewer.Content = chartStack;
            chartCard.Child = scrollViewer;
            Grid.SetColumn(chartCard, 0);

            // Cột phải (3/10)
            Border checkCard = new Border
            {
                Background = (Brush)(bc.ConvertFrom(isLoggerDarkMode ? "#1E1E1E" : "#FFFFFF") ?? Brushes.White),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(8, 0, 0, 0),
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 10, ShadowDepth = 2, Opacity = 0.08 }
            };

            StackPanel checkPanel = new StackPanel();

            TextBlock checkTitle = new TextBlock
            {
                Text = "🔍 TRA CỨU LOG DAY",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = isLoggerDarkMode ? Brushes.LightBlue : Brushes.Navy,
                Margin = new Thickness(0, 0, 0, 15)
            };
            checkPanel.Children.Add(checkTitle);

            TextBlock lblCheckDay = new TextBlock { Text = "CHECK DAY (dd/MM/yyyy):", FontWeight = FontWeights.SemiBold, FontSize = 11, Foreground = isLoggerDarkMode ? Brushes.White : Brushes.Black, Margin = new Thickness(0, 0, 0, 4) };
            TextBox txtCheckDay = new TextBox { Height = 28, VerticalContentAlignment = VerticalAlignment.Center, Padding = new Thickness(5, 0, 5, 0), Text = DateTime.Now.ToString("dd/MM/yyyy") };

            TextBlock txtErrorStatus = new TextBlock
            {
                Text = "",
                Foreground = Brushes.Red,
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Margin = new Thickness(0, 8, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Button btnExecuteCheck = new Button
            {
                Content = "🔍 TRUY XUẤT DỮ LIỆU",
                Height = 32,
                Background = (Brush)(bc.ConvertFrom("#009688") ?? Brushes.Teal),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 5, 0, 15),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            StackPanel resultPanel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
            TextBlock txtResHeader = new TextBlock { Text = "📊 KẾT QUẢ THỐNG KÊ NGÀY", FontWeight = FontWeights.Bold, FontSize = 12, Foreground = isLoggerDarkMode ? Brushes.Orange : Brushes.DarkOrange, Margin = new Thickness(0, 0, 0, 8) };
            
            TextBlock txtWaterMax = new TextBlock { Text = "• Mực nước cao nhất: -- cm", FontSize = 11, Foreground = isLoggerDarkMode ? Brushes.White : Brushes.Black, Margin = new Thickness(0, 2, 0, 2) };
            TextBlock txtTempMax = new TextBlock { Text = "• Nhiệt độ cao nhất: -- °C", FontSize = 11, Foreground = isLoggerDarkMode ? Brushes.White : Brushes.Black, Margin = new Thickness(0, 2, 0, 2) };
            TextBlock txtHumMax = new TextBlock { Text = "• Độ ẩm cao nhất: -- %", FontSize = 11, Foreground = isLoggerDarkMode ? Brushes.White : Brushes.Black, Margin = new Thickness(0, 2, 0, 2) };

            resultPanel.Children.Add(txtResHeader);
            resultPanel.Children.Add(txtWaterMax);
            resultPanel.Children.Add(txtTempMax);
            resultPanel.Children.Add(txtHumMax);

            checkPanel.Children.Add(lblCheckDay);
            checkPanel.Children.Add(txtCheckDay);
            checkPanel.Children.Add(txtErrorStatus);
            checkPanel.Children.Add(btnExecuteCheck);
            checkPanel.Children.Add(resultPanel);

            checkCard.Child = checkPanel;
            Grid.SetColumn(checkCard, 1);

            contentGrid.Children.Add(chartCard);
            contentGrid.Children.Add(checkCard);
            Grid.SetRow(contentGrid, 1);

            mainGrid.Children.Add(headerCard);
            mainGrid.Children.Add(contentGrid);
            logWindow.Content = mainGrid;

            // Xóa thông báo lỗi khi ô nhập được tương tác
            RoutedEventHandler clearErrorAction = (s, e) => txtErrorStatus.Text = "";
            txtCheckDay.GotFocus += clearErrorAction;
            txtCheckDay.PreviewMouseDown += (s, e) => txtErrorStatus.Text = "";

            Action applyThemeColors = () =>
            {
                Brush bg = (Brush)(bc.ConvertFrom(isLoggerDarkMode ? "#121212" : "#F4F6F9") ?? Brushes.White);
                Brush cardBg = (Brush)(bc.ConvertFrom(isLoggerDarkMode ? "#1E1E1E" : "#FFFFFF") ?? Brushes.White);
                Brush cBg = (Brush)(bc.ConvertFrom(isLoggerDarkMode ? "#252526" : "#FAFAFA") ?? Brushes.White);
                Brush textBrush = isLoggerDarkMode ? Brushes.White : Brushes.Black;

                logWindow.Background = bg;
                headerCard.Background = cardBg;
                chartCard.Background = cardBg;
                checkCard.Background = cardBg;

                canvasWater.Background = cBg;
                canvasTemp.Background = cBg;
                canvasHum.Background = cBg;

                lblLoggerTheme.Text = isLoggerDarkMode ? "🌙 Nền Tối" : "☀️ Nền Sáng";
                lblLoggerTheme.Foreground = textBrush;
                lblCheckDay.Foreground = textBrush;
                checkTitle.Foreground = isLoggerDarkMode ? Brushes.LightBlue : Brushes.Navy;

                txtWaterMax.Foreground = textBrush;
                txtTempMax.Foreground = textBrush;
                txtHumMax.Foreground = textBrush;
            };

            Action<string> updateCharts = (mode) =>
            {
                currentMode = mode;
                DateTime now = DateTime.Now;
                List<string> xLabels = new List<string>();
                List<double?> avgWater = new List<double?>();
                List<double?> avgTemp = new List<double?>();
                List<double?> avgHum = new List<double?>();

                if (mode == "DAY")
                {
                    DateTime today = now.Date;
                    for (int h = 0; h < 24; h++)
                    {
                        xLabels.Add($"{h:D2}:00");
                        var hourLogs = allLogs.Where(x => x.Timestamp.Date == today && x.Timestamp.Hour == h).ToList();
                        avgWater.Add(hourLogs.Any() ? hourLogs.Average(x => x.WaterLevel) : (double?)null);
                        avgTemp.Add(hourLogs.Any() ? hourLogs.Average(x => x.Temperature) : (double?)null);
                        avgHum.Add(hourLogs.Any() ? hourLogs.Average(x => x.Humidity) : (double?)null);
                    }
                }
                else if (mode == "WEEK")
                {
                    for (int i = 6; i >= 0; i--)
                    {
                        DateTime dayDate = now.Date.AddDays(-i);
                        xLabels.Add(dayDate.ToString("dd/MM"));

                        var dayLogs = allLogs.Where(x => x.Timestamp.Date == dayDate).ToList();
                        avgWater.Add(dayLogs.Any() ? dayLogs.Average(x => x.WaterLevel) : (double?)null);
                        avgTemp.Add(dayLogs.Any() ? dayLogs.Average(x => x.Temperature) : (double?)null);
                        avgHum.Add(dayLogs.Any() ? dayLogs.Average(x => x.Humidity) : (double?)null);
                    }
                }
                else if (mode == "MONTH")
                {
                    int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                    for (int d = 1; d <= daysInMonth; d++)
                    {
                        DateTime monthDate = new DateTime(now.Year, now.Month, d);
                        xLabels.Add(monthDate.ToString("dd/MM"));

                        var dayLogs = allLogs.Where(x => x.Timestamp.Date == monthDate).ToList();
                        avgWater.Add(dayLogs.Any() ? dayLogs.Average(x => x.WaterLevel) : (double?)null);
                        avgTemp.Add(dayLogs.Any() ? dayLogs.Average(x => x.Temperature) : (double?)null);
                        avgHum.Add(dayLogs.Any() ? dayLogs.Average(x => x.Humidity) : (double?)null);
                    }
                }

                Brush textBrush = isLoggerDarkMode ? Brushes.White : (Brush)(bc.ConvertFrom("#212121") ?? Brushes.Black);
                Brush waterBrush = (Brush)(bc.ConvertFrom("#0288D1") ?? Brushes.Blue);
                Brush tempBrush = (Brush)(bc.ConvertFrom("#D32F2F") ?? Brushes.Red);
                Brush humBrush = (Brush)(bc.ConvertFrom("#388E3C") ?? Brushes.Green);

                DrawBarChart(canvasWater, avgWater, xLabels, $"BIỂU ĐỒ MỰC NƯỚC TRUNG BÌNH ({mode}) - ĐƠN VỊ: CM", waterBrush, 20.0, textBrush);
                DrawLineChart(canvasTemp, avgTemp, xLabels, $"BIỂU ĐỒ NHIỆT ĐỘ TRUNG BÌNH ({mode}) - ĐƠN VỊ: °C", tempBrush, 50.0, textBrush);
                DrawLineChart(canvasHum, avgHum, xLabels, $"BIỂU ĐỒ ĐỘ ẨM TRUNG BÌNH ({mode}) - ĐƠN VỊ: %", humBrush, 100.0, textBrush);
            };

            btnExecuteCheck.Click += (s, e) =>
            {
                txtErrorStatus.Text = "";

                // FIX 2: Phương thức reset lại các giá trị thống kê về mặc định khi bị lỗi
                void ResetLabels()
                {
                    txtWaterMax.Text = "• Mực nước cao nhất: -- cm";
                    txtTempMax.Text = "• Nhiệt độ cao nhất: -- °C";
                    txtHumMax.Text = "• Độ ẩm cao nhất: -- %";
                }

                string dateInput = txtCheckDay.Text.Trim();
                string[] validFormats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" };

                if (!DateTime.TryParseExact(dateInput, validFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime targetDate))
                {
                    txtErrorStatus.Text = "NOT FOUND";
                    ResetLabels();
                    return;
                }

                var dayLogs = allLogs.Where(x => x.Timestamp.Date == targetDate.Date).ToList();

                if (dayLogs.Count == 0)
                {
                    txtErrorStatus.Text = "CAN NOT FOUND";
                    ResetLabels();
                    return;
                }

                double maxWater = dayLogs.Max(x => x.WaterLevel);
                double maxTemp = dayLogs.Max(x => x.Temperature);
                double maxHum = dayLogs.Max(x => x.Humidity);

                txtWaterMax.Text = $"• Mực nước cao nhất: {maxWater:F1} cm";
                txtTempMax.Text = $"• Nhiệt độ cao nhất: {maxTemp:F1} °C";
                txtHumMax.Text = $"• Độ ẩm cao nhất: {maxHum:F1} %";

                List<string> xLabels = new List<string>();
                List<double?> hourWater = new List<double?>();
                List<double?> hourTemp = new List<double?>();
                List<double?> hourHum = new List<double?>();

                for (int h = 0; h < 24; h++)
                {
                    xLabels.Add($"{h:D2}:00");
                    var hLogs = dayLogs.Where(x => x.Timestamp.Hour == h).ToList();
                    hourWater.Add(hLogs.Any() ? hLogs.Average(x => x.WaterLevel) : (double?)null);
                    hourTemp.Add(hLogs.Any() ? hLogs.Average(x => x.Temperature) : (double?)null);
                    hourHum.Add(hLogs.Any() ? hLogs.Average(x => x.Humidity) : (double?)null);
                }

                Brush textBrush = isLoggerDarkMode ? Brushes.White : (Brush)(bc.ConvertFrom("#212121") ?? Brushes.Black);
                Brush waterBrush = (Brush)(bc.ConvertFrom("#0288D1") ?? Brushes.Blue);
                Brush tempBrush = (Brush)(bc.ConvertFrom("#D32F2F") ?? Brushes.Red);
                Brush humBrush = (Brush)(bc.ConvertFrom("#388E3C") ?? Brushes.Green);

                // FIX 3: Chuyển Temp và Hum về DrawLineChart để đồng nhất giao diện
                DrawBarChart(canvasWater, hourWater, xLabels, $"BIỂU ĐỒ MỰC NƯỚC NGÀY {targetDate:dd/MM/yyyy} (CM)", waterBrush, 20.0, textBrush);
                DrawLineChart(canvasTemp, hourTemp, xLabels, $"BIỂU ĐỒ NHIỆT ĐỘ NGÀY {targetDate:dd/MM/yyyy} (°C)", tempBrush, 50.0, textBrush);
                DrawLineChart(canvasHum, hourHum, xLabels, $"BIỂU ĐỒ ĐỘ ẨM NGÀY {targetDate:dd/MM/yyyy} (%)", humBrush, 100.0, textBrush);
            };

            btnDay.Click += (s, e) => updateCharts("DAY");
            btnWeek.Click += (s, e) => updateCharts("WEEK");
            btnMonth.Click += (s, e) => updateCharts("MONTH");

            toggleLoggerTheme.Click += (s, e) =>
            {
                isLoggerDarkMode = toggleLoggerTheme.IsChecked ?? false;
                applyThemeColors();
                updateCharts(currentMode);
            };

            logWindow.SizeChanged += (s, e) =>
            {
                if (logWindow.IsLoaded)
                {
                    updateCharts(currentMode);
                }
            };

            logWindow.Loaded += (s, e) => updateCharts("DAY");
            logWindow.ShowDialog();
        }

        private static List<LogModel> LoadLogData(string path)
        {
            List<LogModel> list = new List<LogModel>();
            try
            {
                // FIX 4: Mở file dạng FileShare.ReadWrite để tránh lỗi đụng độ tiến trình ghi file ở SensorLogger
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fs))
                {
                    string? header = reader.ReadLine();
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var parts = line.Split(',');
                        if (parts.Length >= 4)
                        {
                            if (DateTime.TryParseExact(parts[0].Trim(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ts) &&
                                double.TryParse(parts[1].Trim().Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double temp) &&
                                double.TryParse(parts[2].Trim().Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double hum) &&
                                double.TryParse(parts[3].Trim().Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double water))
                            {
                                list.Add(new LogModel
                                {
                                    Timestamp = ts,
                                    TimeStr = ts.ToString("HH:mm dd/MM"),
                                    Temperature = temp,
                                    Humidity = hum,
                                    WaterLevel = water
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi đọc log: {ex.Message}");
            }
            return list;
        }

        private static void DrawBarChart(Canvas canvas, List<double?>? values, List<string>? timeLabels, string title, Brush barBrush, double maxY, Brush textBrush)
        {
            canvas.Children.Clear();
            double width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 600;
            double height = canvas.ActualHeight > 0 ? canvas.ActualHeight : 210;
            double padLeft = 40, padBottom = 35, padTop = 28, padRight = 15;

            TextBlock txtTitle = new TextBlock { Text = title, FontWeight = FontWeights.Bold, Foreground = textBrush, FontSize = 11 };
            Canvas.SetLeft(txtTitle, 10);
            Canvas.SetTop(txtTitle, 5);
            canvas.Children.Add(txtTitle);

            Line xAxis = new Line { X1 = padLeft, Y1 = height - padBottom, X2 = width - padRight, Y2 = height - padBottom, Stroke = Brushes.Gray, StrokeThickness = 1 };
            Line yAxis = new Line { X1 = padLeft, Y1 = padTop, X2 = padLeft, Y2 = height - padBottom, Stroke = Brushes.Gray, StrokeThickness = 1 };
            canvas.Children.Add(xAxis);
            canvas.Children.Add(yAxis);

            if (values == null || values.Count == 0 || timeLabels == null) return;

            double drawWidth = width - padLeft - padRight;
            double drawHeight = height - padTop - padBottom;
            int count = values.Count;
            double slotWidth = drawWidth / count;
            double barWidth = Math.Max(2.0, slotWidth * 0.55);

            for (int i = 0; i < count; i++)
            {
                double slotCenterX = padLeft + i * slotWidth + slotWidth / 2.0;

                Line tick = new Line { X1 = slotCenterX, Y1 = height - padBottom, X2 = slotCenterX, Y2 = height - padBottom + 4, Stroke = Brushes.Gray, StrokeThickness = 1 };
                canvas.Children.Add(tick);

                string labelText = i < timeLabels.Count ? timeLabels[i] : "";
                TextBlock lbl = new TextBlock
                {
                    Text = labelText,
                    FontSize = count > 20 ? 8.0 : 9.5,
                    Foreground = textBrush,
                    TextAlignment = TextAlignment.Center
                };

                if (count > 20)
                {
                    lbl.RenderTransform = new RotateTransform(-45);
                    lbl.RenderTransformOrigin = new Point(0.5, 0.5);
                }

                lbl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(lbl, slotCenterX - (lbl.DesiredSize.Width / 2.0));
                Canvas.SetTop(lbl, height - padBottom + 5);
                canvas.Children.Add(lbl);

                double? valNullable = values[i];
                if (valNullable.HasValue)
                {
                    double val = Math.Min(maxY, Math.Max(0, valNullable.Value));
                    double barHeight = (val / maxY) * drawHeight;
                    double x = slotCenterX - (barWidth / 2.0);
                    double y = height - padBottom - barHeight;

                    Rectangle rect = new Rectangle
                    {
                        Width = barWidth,
                        Height = barHeight,
                        Fill = barBrush,
                        ToolTip = $"Thời gian: {labelText}\nGiá trị: {valNullable.Value:F1}"
                    };
                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                    canvas.Children.Add(rect);
                }
            }
        }

        private static void DrawLineChart(Canvas canvas, List<double?>? values, List<string>? timeLabels, string title, Brush lineBrush, double maxY, Brush textBrush)
        {
            canvas.Children.Clear();
            double width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 600;
            double height = canvas.ActualHeight > 0 ? canvas.ActualHeight : 210;
            double padLeft = 40, padBottom = 35, padTop = 28, padRight = 15;

            TextBlock txtTitle = new TextBlock { Text = title, FontWeight = FontWeights.Bold, Foreground = textBrush, FontSize = 11 };
            Canvas.SetLeft(txtTitle, 10);
            Canvas.SetTop(txtTitle, 5);
            canvas.Children.Add(txtTitle);

            Line xAxis = new Line { X1 = padLeft, Y1 = height - padBottom, X2 = width - padRight, Y2 = height - padBottom, Stroke = Brushes.Gray, StrokeThickness = 1 };
            Line yAxis = new Line { X1 = padLeft, Y1 = padTop, X2 = padLeft, Y2 = height - padBottom, Stroke = Brushes.Gray, StrokeThickness = 1 };
            canvas.Children.Add(xAxis);
            canvas.Children.Add(yAxis);

            if (values == null || values.Count == 0 || timeLabels == null) return;

            double drawWidth = width - padLeft - padRight;
            double drawHeight = height - padTop - padBottom;
            int count = values.Count;
            double slotWidth = drawWidth / count;

            Polyline polyline = new Polyline { Stroke = lineBrush, StrokeThickness = 2 };
            PointCollection points = new PointCollection();
            List<UIElement> overlayDots = new List<UIElement>();

            for (int i = 0; i < count; i++)
            {
                double slotCenterX = padLeft + i * slotWidth + slotWidth / 2.0;

                Line tick = new Line { X1 = slotCenterX, Y1 = height - padBottom, X2 = slotCenterX, Y2 = height - padBottom + 4, Stroke = Brushes.Gray, StrokeThickness = 1 };
                canvas.Children.Add(tick);

                string labelText = i < timeLabels.Count ? timeLabels[i] : "";
                TextBlock lbl = new TextBlock
                {
                    Text = labelText,
                    FontSize = count > 20 ? 8.0 : 9.5,
                    Foreground = textBrush,
                    TextAlignment = TextAlignment.Center
                };

                if (count > 20)
                {
                    lbl.RenderTransform = new RotateTransform(-45);
                    lbl.RenderTransformOrigin = new Point(0.5, 0.5);
                }

                lbl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(lbl, slotCenterX - (lbl.DesiredSize.Width / 2.0));
                Canvas.SetTop(lbl, height - padBottom + 5);
                canvas.Children.Add(lbl);

                double? valNullable = values[i];
                if (valNullable.HasValue)
                {
                    double val = Math.Min(maxY, Math.Max(0, valNullable.Value));
                    double y = height - padBottom - (val / maxY) * drawHeight;

                    points.Add(new Point(slotCenterX, y));

                    Ellipse dot = new Ellipse
                    {
                        Width = count > 20 ? 4 : 6,
                        Height = count > 20 ? 4 : 6,
                        Fill = lineBrush,
                        ToolTip = $"Thời gian: {labelText}\nGiá trị: {valNullable.Value:F1}"
                    };
                    Canvas.SetLeft(dot, slotCenterX - (dot.Width / 2.0));
                    Canvas.SetTop(dot, y - (dot.Height / 2.0));
                    overlayDots.Add(dot);
                }
            }

            polyline.Points = points;
            canvas.Children.Add(polyline);

            foreach (var dot in overlayDots)
            {
                canvas.Children.Add(dot);
            }
        }
    }
}