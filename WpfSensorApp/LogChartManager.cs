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
    public static class LogChartManager
    {
        public static void ShowLogWindow(Window ownerWindow, bool isDarkMode, Func<string, Button> createModernButtonFunc)
        {
            string logFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "sensor_data_log.csv");

            if (!File.Exists(logFilePath))
            {
                MessageBox.Show("Chưa có dữ liệu nhật ký được lưu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            List<LogModel> allLogs = LoadLogData(logFilePath);

            BrushConverter bc = new BrushConverter();
            bool isLoggerDarkMode = isDarkMode;
            string currentMode = "DAY"; // Lưu chế độ lọc hiện tại

            Window logWindow = new Window
            {
                Title = "NHẬT KÝ VÀ BIỂU ĐỒ LỊCH SỬ DỮ LIỆU CẢM BIẾN",
                Width = 1100,
                Height = 820,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = ownerWindow,
                Background = (Brush)(bc.ConvertFrom(isLoggerDarkMode ? "#121212" : "#F4F6F9") ?? Brushes.White)
            };

            Grid mainGrid = new Grid { Margin = new Thickness(16) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // 1. HEADER CARD
            Border headerCard = new Border
            {
                Background = (Brush)(bc.ConvertFrom(isLoggerDarkMode ? "#1E1E1E" : "#FFFFFF") ?? Brushes.White),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 12),
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

            Button btnDay = createModernButtonFunc("DAY");
            Button btnWeek = createModernButtonFunc("WEEK");
            Button btnMonth = createModernButtonFunc("MONTH");

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

            // 2. CHART AREA CARD
            Border chartCard = new Border
            {
                Background = (Brush)(bc.ConvertFrom(isLoggerDarkMode ? "#1E1E1E" : "#FFFFFF") ?? Brushes.White),
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
            Brush canvasBg = (Brush)(bc.ConvertFrom(isLoggerDarkMode ? "#252526" : "#FAFAFA") ?? Brushes.White);

            Canvas canvasWater = new Canvas { Height = 220, Background = canvasBg, Margin = new Thickness(0, 0, 0, 16), HorizontalAlignment = HorizontalAlignment.Stretch };
            Canvas canvasTemp = new Canvas { Height = 220, Background = canvasBg, Margin = new Thickness(0, 0, 0, 16), HorizontalAlignment = HorizontalAlignment.Stretch };
            Canvas canvasHum = new Canvas { Height = 220, Background = canvasBg, Margin = new Thickness(0, 0, 0, 5), HorizontalAlignment = HorizontalAlignment.Stretch };

            chartStack.Children.Add(canvasWater);
            chartStack.Children.Add(canvasTemp);
            chartStack.Children.Add(canvasHum);

            scrollViewer.Content = chartStack;
            chartCard.Child = scrollViewer;
            Grid.SetRow(chartCard, 1);

            mainGrid.Children.Add(headerCard);
            mainGrid.Children.Add(chartCard);
            logWindow.Content = mainGrid;

            Action applyThemeColors = () =>
            {
                Brush bg = (Brush)(bc.ConvertFrom(isLoggerDarkMode ? "#121212" : "#F4F6F9") ?? Brushes.White);
                Brush cardBg = (Brush)(bc.ConvertFrom(isLoggerDarkMode ? "#1E1E1E" : "#FFFFFF") ?? Brushes.White);
                Brush cBg = (Brush)(bc.ConvertFrom(isLoggerDarkMode ? "#252526" : "#FAFAFA") ?? Brushes.White);

                logWindow.Background = bg;
                headerCard.Background = cardBg;
                chartCard.Background = cardBg;
                canvasWater.Background = cBg;
                canvasTemp.Background = cBg;
                canvasHum.Background = cBg;

                lblLoggerTheme.Text = isLoggerDarkMode ? "🌙 Nền Tối" : "☀️ Nền Sáng";
                lblLoggerTheme.Foreground = isLoggerDarkMode ? Brushes.White : (Brush)(bc.ConvertFrom("#333333") ?? Brushes.Black);
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

            btnDay.Click += (s, e) => updateCharts("DAY");
            btnWeek.Click += (s, e) => updateCharts("WEEK");
            btnMonth.Click += (s, e) => updateCharts("MONTH");

            toggleLoggerTheme.Click += (s, e) =>
            {
                isLoggerDarkMode = toggleLoggerTheme.IsChecked ?? false;
                applyThemeColors();
                updateCharts(currentMode);
            };

            // THÊM SỰ KIỆN TỰ VẼ LẠI KHI THAY ĐỔI KÍCH THƯỚC CỬA SỔ / PHÓNG TO
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
                var lines = File.ReadAllLines(path).Skip(1);
                foreach (var line in lines)
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi đọc log: {ex.Message}");
            }
            return list;
        }

        private static void DrawBarChart(Canvas canvas, List<double?>? values, List<string>? timeLabels, string title, Brush barBrush, double maxY, Brush textBrush)
        {
            canvas.Children.Clear();
            double width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 900;
            double height = canvas.ActualHeight > 0 ? canvas.ActualHeight : 220;
            double padLeft = 45, padBottom = 40, padTop = 30, padRight = 20;

            TextBlock txtTitle = new TextBlock { Text = title, FontWeight = FontWeights.Bold, Foreground = textBrush, FontSize = 12 };
            Canvas.SetLeft(txtTitle, 10);
            Canvas.SetTop(txtTitle, 6);
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
                    FontSize = count > 20 ? 8.0 : 10.0, 
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
                Canvas.SetTop(lbl, height - padBottom + 6);
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
                        ToolTip = $"Thời gian: {labelText}\nMực nước trung bình: {valNullable.Value:F1} cm"
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
            double width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 900;
            double height = canvas.ActualHeight > 0 ? canvas.ActualHeight : 220;
            double padLeft = 45, padBottom = 40, padTop = 30, padRight = 20;

            TextBlock txtTitle = new TextBlock { Text = title, FontWeight = FontWeights.Bold, Foreground = textBrush, FontSize = 12 };
            Canvas.SetLeft(txtTitle, 10);
            Canvas.SetTop(txtTitle, 6);
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
                    FontSize = count > 20 ? 8.0 : 10.0, 
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
                Canvas.SetTop(lbl, height - padBottom + 6);
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
                        ToolTip = $"Thời gian: {labelText}\nGiá trị trung bình: {valNullable.Value:F1}"
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

    public class LogModel
    {
        public DateTime Timestamp { get; set; }
        public string TimeStr { get; set; } = string.Empty;
        public double WaterLevel { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
    }
}