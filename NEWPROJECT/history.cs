using System;
using System.IO;
using System.Collections.Generic; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
public static class CSVhisstory
{
    private static readonly object _csvLock = new object();
    private static readonly string CsvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.csv");

    static CSVhisstory()
    {
        InitCsvHeader();
    }

    private static void InitCsvHeader()
    {
        try
        {
            lock (_csvLock)
            {
                if (!File.Exists(CsvFilePath))
                {
                    string header = "Thoi gian, Nhiet do, Muc Nuoc" + Environment.NewLine;
                    File.WriteAllText(CsvFilePath, header, Encoding.UTF8);
                }
            }
        }
        catch { }
    }
    public static void Write(DateTime timestamp, double temp, double waterLevel)
    {
        try
        {
            lock (_csvLock)
            {
                // Format chuẩn dạng: 2026-07-16 00:00:00,26.1,17.0
                string timeStr = timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                string tempStr = temp.ToString("F1", CultureInfo.InvariantCulture);
                string waterStr = waterLevel.ToString("F1", CultureInfo.InvariantCulture);

                string csvLine = $"{timeStr},{tempStr},{waterStr}" + Environment.NewLine;

                File.AppendAllText(CsvFilePath, csvLine, Encoding.UTF8);
            }
        }
        catch { }
    }
}
