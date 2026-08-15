#nullable disable
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Threading;

namespace WpfSensorApp
{
    public class SensorLogger
    {
        private readonly DispatcherTimer _loggerTimer;
        private readonly Func<(string temp, string hum, string water)> _getSensorDataFunc;
        private readonly string _logFolderPath;
        private readonly string _logFilePath;

        public SensorLogger(Func<(string temp, string hum, string water)> getSensorDataFunc)
        {
            _getSensorDataFunc = getSensorDataFunc;
            _logFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            _logFilePath = Path.Combine(_logFolderPath, "sensor_data_log.csv");

            EnsureLogFileExists();

            _loggerTimer = new DispatcherTimer();
            _loggerTimer.Interval = TimeSpan.FromMinutes(5);
            _loggerTimer.Tick += LoggerTimer_Tick;
        }

        public void Start()
        {
            _loggerTimer.Start();
            WriteLog();
        }

        public void Stop()
        {
            _loggerTimer.Stop();
        }

        private void LoggerTimer_Tick(object sender, EventArgs e)
        {
            WriteLog();
        }

        private void EnsureLogFileExists()
        {
            try
            {
                if (!Directory.Exists(_logFolderPath))
                {
                    Directory.CreateDirectory(_logFolderPath);
                }

                if (!File.Exists(_logFilePath))
                {
                    string header = "Thời Gian,Nhiệt Độ (°C),Độ Ẩm (%),Mực Nước (cm)" + Environment.NewLine;
                    File.WriteAllText(_logFilePath, header, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi tạo file log: {ex.Message}");
            }
        }

        public void WriteLog()
        {
            try
            {
                var data = _getSensorDataFunc?.Invoke();
                if (data == null) return;

                string tempVal = data.Value.temp.Replace("°C", "").Trim();
                string humVal = data.Value.hum.Replace("%", "").Trim();
                string waterVal = data.Value.water.Replace("cm", "").Trim();

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logLine = $"{timestamp},{tempVal},{humVal},{waterVal}" + Environment.NewLine;

                File.AppendAllText(_logFilePath, logLine, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi ghi file log: {ex.Message}");
            }
        }

        public void OpenLogFile()
        {
            EnsureLogFileExists();
            try
            {
                if (File.Exists(_logFilePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _logFilePath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi mở file log: {ex.Message}");
            }
        }
    }
}