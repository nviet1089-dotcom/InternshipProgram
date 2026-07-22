using System;
using System.IO;
using System.Collections.Generic; 
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System;
using System.IO;
using System.Text;

public static class loggerr
{
    private static readonly object _fileLock = new object();
    private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_logs.txt");

    public static void Log(string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logEntry = $"[{timestamp}] {message}";

        lock (Console.Out)
        {
            Console.WriteLine(logEntry);
        }

        WriteToFile(logEntry);
    }

    private static void WriteToFile(string logEntry)
    {
        try
        {
            lock (_fileLock)
            {
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine, Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Không thể ghi file log: {ex.Message}");
        }
    }
}