#nullable disable
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace WpfSensorApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _temperature = "-- °C";
        private string _humidity = "-- %";
        private string _waterLevel = "--.-- cm";
        private string _dangerLevel = "0/10";
        private string _statusText = "Trạng thái: Sẵn sàng";
        private Brush _statusColor = Brushes.Gray;
        private Brush _waterLevelColor = (Brush)new BrushConverter().ConvertFrom("#FF0288D1");
        private Brush _waterLevelBgColor = (Brush)new BrushConverter().ConvertFrom("#FFE1F5FE");

        public string Temperature
        {
            get => _temperature;
            set { _temperature = value; OnPropertyChanged(); }
        }

        public string Humidity
        {
            get => _humidity;
            set { _humidity = value; OnPropertyChanged(); }
        }

        public string WaterLevel
        {
            get => _waterLevel;
            set { _waterLevel = value; OnPropertyChanged(); }
        }

        public string DangerLevel
        {
            get => _dangerLevel;
            set { _dangerLevel = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public Brush StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(); }
        }

        public Brush WaterLevelColor
        {
            get => _waterLevelColor;
            set { _waterLevelColor = value; OnPropertyChanged(); }
        }

        public Brush WaterLevelBgColor
        {
            get => _waterLevelBgColor;
            set { _waterLevelBgColor = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}