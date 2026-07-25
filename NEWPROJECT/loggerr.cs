using System;
using System.IO;
using System.Text;
using System.Globalization;

public static class Logger
{
    private static readonly string FilePath = Path.Combine( AppDomain.CurrentDomain.BaseDirectory,  "history.csv" );
    private static readonly object FileLock = new object();
    static Logger()
    {
        InitCsvHeader();
    }
    private static void InitCsvHeader()
    {
        try
        {
            lock (FileLock)
            {
                if(!File.Exists(FilePath))
                {
                    string header = "thời gian ; nhiệt độ ; mưucj nước " + Environment.NewLine;
                    File.WriteAllText(FilePath , header , Encoding.UTF8);
                }
            }
        }
        catch{ }
    }

    public static void LogCSV(DateTime thoiGian, double nhietDo, double mucNuoc)
    {
        lock (FileLock)
        {
            try
            {
                bool fileExists = File.Exists(FilePath);

                using (StreamWriter writer = new StreamWriter(FilePath, append: true))
                {
                    if (!fileExists)
                    {
                        writer.WriteLine("Thoi gian, Nhiet do, Muc Nuoc");
                    }

                    string logLine = string.Format(
                        CultureInfo.InvariantCulture,
                        "{0:yyyy-MM-dd HH:mm:ss},{1:F1},{2:F1}",
                        thoiGian,
                        nhietDo,
                        mucNuoc
                    );

                    writer.WriteLine(logLine);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[CSV LOGGER] Đã lưu: {logLine}");
                    Console.ResetColor();    
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[CSV LOGGER LỖI] Không thể ghi file: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}