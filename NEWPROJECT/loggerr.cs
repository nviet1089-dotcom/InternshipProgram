using System;
using System.IO;
using System.Collections.Generic; 
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
public static class Loggerr
{
    private static readonly string FilePath = "historyy.csv";
    private static readonly string FileLock = new object();
    public static void LogCSV(DateTime thoiGian, double nhietDo, double mucNuoc);
}