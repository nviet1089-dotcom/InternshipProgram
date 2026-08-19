using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace WpfSensorApp
{
    public class SensorLogger
    {
        private readonly Func<(string Temp, string Hum, string Water)> _getDataDelegate;
        private DispatcherTimer? _logTimer;
        private readonly string _logFolderPath;
        private readonly string _logFilePath;

        public SensorLogger(Func<(string Temp, string Hum, string Water)> getDataDelegate)
        {
            _getDataDelegate = getDataDelegate;
            _logFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            _logFilePath = Path.Combine(_logFolderPath, "sensor_data_log.csv");
        }

        public void Start()
        {
            if (!Directory.Exists(_logFolderPath))
            {
                Directory.CreateDirectory(_logFolderPath);
            }

            if (!File.Exists(_logFilePath))
            {
                string header = "Timestamp,Temperature,Humidity,WaterLevel\n";
                File.WriteAllText(_logFilePath, header, Encoding.UTF8);
            }

            _logTimer = new DispatcherTimer();
            _logTimer.Interval = TimeSpan.FromMinutes(5); // Ghi log 5 phút/lần
            _logTimer.Tick += (s, e) => LogCurrentData();
            _logTimer.Start();
        }

        public void LogCurrentData()
        {
            try
            {
                if (_getDataDelegate == null) return;

                (string Temp, string Hum, string Water) data = ("", "", "");

                if (Application.Current != null && Application.Current.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        data = _getDataDelegate();
                    });
                }
                else
                {
                    data = _getDataDelegate();
                }

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                string temp = data.Temp?.Replace(" °C", "").Replace(',', '.').Trim() ?? "--";
                string hum = data.Hum?.Replace(" %", "").Replace(',', '.').Trim() ?? "--";
                string water = data.Water?.Replace(" cm", "").Replace(',', '.').Trim() ?? "--";

                if (temp == "--" || hum == "--" || water == "--.--") return;

                string csvLine = $"{timestamp},{temp},{hum},{water}\n";
                File.AppendAllText(_logFilePath, csvLine, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi ghi log: {ex.Message}");
            }
        }

        public void Stop()
        {
            _logTimer?.Stop();
        }
    }
}