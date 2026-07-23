using System;
using System.IO;
using System.Collections.Generic; 
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
class Program
//dotnet run --project NEWPROJECT
{
    private static Queue<string> DATAqueue = new Queue<string>();
    private static readonly object DATAlock = new object();
    private static Dictionary<string, thietbido?> dictDATABASE = new Dictionary<string, thietbido?>();
    private static readonly object dictDATA = new object();
    private static BinhNuoc? BinhNuoc01;

   static async Task Main(string[] args)
    {
        //
         Console.WriteLine("======KHỞI TẠO BÌNH NƯỚC======");

        double succhua = 0;
        while (true)
        {
            Console.WriteLine("nhập dung tích của bình nước:");
            string input = Console.ReadLine() ?? string.Empty;
            if (double.TryParse(input, out succhua) && succhua > 0)
            {
                break;
            }
            //
             Console.WriteLine("thông số sai, vui lòng nhập lại:");
        }
        
        double  chieucao = 0;
        while (true)
        {
            Console.WriteLine("nhập chiều cao của bình nước:");
            string input = Console.ReadLine() ?? string.Empty;
            if (double.TryParse(input, out chieucao) && chieucao > 0)
            {
                break;
            }
             Console.WriteLine("thông số sai, vui lòng nhập lại:");
        }
        BinhNuoc01 = new BinhNuoc(succhua, chieucao);
        while(true)
        {
            Console.WriteLine($"nhập mức nước đã đổ vào (0 - {BinhNuoc01._dungtichtoida}):");
            string inputmucnuoc = Console.ReadLine() ?? string.Empty;
            if(double.TryParse(inputmucnuoc , out double mucnuoc ))
            {
                if(mucnuoc < 0)
                {
                    Console.WriteLine("thông tin nhập không đúng, vui lòng nhập lại:");
                }
                else  if(mucnuoc > BinhNuoc01._dungtichtoida)
                {
                    BinhNuoc01._mucnuochientai = mucnuoc;
                    Console.WriteLine("Cảnh báo bình đã đầy nước có thể tràn ra bên ngoài.!");
                }
                else
                {
                    BinhNuoc01._mucnuochientai = mucnuoc;
                    Console.WriteLine("vui lòng đợi xử lí thông tin.");
                    break;
                }
            }
            else
            {
                Console.WriteLine("thông tin sai vui lòng nhập lại!");
            }
        }

        Console.WriteLine("vui lòng đợi giây lát");
        Console.WriteLine($"dung tích tối đa của bình nước là: {BinhNuoc01._dungtichtoida}.lit");
        Console.WriteLine($"mực nước hiện tại trong bình nước là: {BinhNuoc01._mucnuochientai}.lit");


        Console.WriteLine("===============KIỂM TRA THIẾT BỊ===============");
        List<thietbido> danhsachthietbi = new List<thietbido>();
        danhsachthietbi.Add(new Cambietnhietdo {mathietbicambien = "TEMP--001"});
        danhsachthietbi.Add(new CameraTruoc {mathietbicamera = "CAMERA--001"});
        //
        lock (dictDATA)
        {
            dictDATABASE["TEMP--001"] = danhsachthietbi[0];
            dictDATABASE["CAM--001"] = danhsachthietbi[1];
        }
        //
        if (danhsachthietbi != null && danhsachthietbi.Count > 0)
        {
            foreach(var thietbi in danhsachthietbi)
            {
                Console.WriteLine("===============KHỚI TẠO THIẾT BỊ===============");
                thietbi.mathietbido();
                thietbi.trangthaicuathietbi();
                if(thietbi is IConnectable thietbiketnoi)
                {
                    thietbiketnoi.ConnectSerial();
                }
            }
        }
        Console.ForegroundColor = ConsoleColor.Yellow;
        // thay chay gia lap o day de ctrinh chay muot ma hon
        Console.WriteLine("\n======================================================");
        Console.WriteLine("      CHẠY SENSOR GIẢ LẬP        ");
        Console.WriteLine("\n======================================================");
        Console.ResetColor();
        Task taskSimulator = SensorSimulator();
        Task taskProcessor = BackgroundProcessor();
        Task.WaitAll(taskSimulator, taskProcessor);
    }

        public static bool ArduinoData(string rawData, out DateTime date, out double temp, out double waterlevel)
        {
            date = DateTime.MinValue;
            temp = 0.0;
            waterlevel = 0.0;

            if (string.IsNullOrWhiteSpace(rawData) || !rawData.StartsWith("$LOG,") || !rawData.EndsWith("#"))
            {
                return false;
            }

            try
            {
                string cleanData = rawData.TrimStart('$').TrimEnd('#');
                string[] parts = cleanData.Split(',');

                if (parts.Length != 4 || parts[0] != "LOG") {return false;}
                if (!DateTime.TryParse(parts[1],CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) {return false;}
                if (!double.TryParse(parts[2],NumberStyles.Any, CultureInfo.InvariantCulture, out temp)) {return false;}
                if (!double.TryParse(parts[3],NumberStyles.Any, CultureInfo.InvariantCulture, out waterlevel)) {return false;}
                return true;
            }    
            catch
            {
                return false; 
            }
        }   
        static async Task BackgroundProcessor()
     {
        //
        int logCount = 0;
        double totalTemp = 0.0;
        double maxTemp = double.MinValue;
        int maxTempIndex = 0 ;
        //
        while (true)
        {
            await Task.Delay(3000);
            List<string> itemsToProcess = new List<string>();
                lock (DATAlock)
                {
                    while (DATAqueue.Count > 0)
                    { 
                        itemsToProcess.Add(DATAqueue.Dequeue());
                    }
                }
                    if (itemsToProcess.Count > 0)
                {
                   Console.ForegroundColor = ConsoleColor.DarkRed;
                   Console.WriteLine($"\n[Background] xử lý {itemsToProcess.Count} gói tin từ queue:");
                   foreach (var item in itemsToProcess)
                       {
                            Console.WriteLine($"   => {item}");
                       }
                            Console.ResetColor();
                }
       
                    foreach (var item in itemsToProcess)
                    { 
                        if(item.StartsWith("$LOG") && item.EndsWith("#"))
                        {
                            if(ArduinoData(item, out DateTime date,out double temp ,out double waterlevel))
                                {
                                    //
                                    logCount++;
                                    totalTemp += temp;

                                    if(temp > maxTemp)
                                        {
                                            maxTemp = temp;
                                            maxTempIndex = logCount;
                                        }
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine($"log ngay:{date:yyyy-MM-dd} , nhiet do:{temp:F1} do , muc nuoc:{waterlevel}cm ");
                                    Console.ResetColor();

                                    CSVhisstory.Write(date, temp, waterlevel);

                                    if(logCount % 10 == 0)
                                        {
                                            double avgTemp = totalTemp / 10;
                                            Console.ForegroundColor = ConsoleColor.Magenta;
                                            Console.WriteLine("\n======================================================");
                                            Console.WriteLine($"Nhiệt độ trung bình : {avgTemp:F2} °C");
                                            Console.WriteLine($"Nhiệt độ cao nhất   : {maxTemp:F1} °C (tại lần đo thứ {maxTempIndex})");
                                            Console.WriteLine("======================================================\n");
                                            totalTemp = 0.0;
                                            maxTemp = double.MinValue;
                                            maxTempIndex = 0;
                                            //
                            
                                        }
                                }
                                else
                                {
                                    Console.WriteLine("loi cu phap yeu cau kiem tra lai");
                                }
                        }
                        else
                        {
                            string[] parts = item.Split(':');
                            if (parts.Length < 2) continue; 

                            string prefix = parts[0];
                            if (prefix == "DEV") 
                                { 
                                    if (parts.Length >= 3)
                                        {
                                            string id = parts[1];
                                            string newStatus = parts[2];

                                            lock (dictDATA)
                                                {
                                                    if (dictDATABASE.TryGetValue(id, out thietbido? thietbi) && thietbi != null)
                                                        {
                                                            thietbi.trangthai = newStatus;
                                                           Console.WriteLine($"[Update] {thietbi.tenthietbido} ({id}) chuyen sang : {thietbi.trangthai}");
                                                        }
                                                    else
                                                        {
                                                            Console.WriteLine($"[Cảnh báo] ID thiet bi khong hop le : {id}");
                                                        }
                                                }
                                        }
                                } 
                            else if (prefix == "TEMP") 
                                {
                                    string tempValue = parts[1];
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine($"[TEMP] nhiet do do duoc : {tempValue}°C");
                                    Console.ResetColor();
                                }
                        }
                    }
            
        }

     }
        static async Task SensorSimulator()
        {
            string[] ids = { "CAM--001", "TEMP--001" };
            Random rand = new Random();

            while (true)
            {
                await Task.Delay(3000); 

                string data = $"AUTO_SENSOR | Temp: {rand.Next(20, 35)}°C | Time: {DateTime.Now:HH:mm:ss}";
                lock (DATAlock) { DATAqueue.Enqueue(data); }

                string randomTemp = (rand.NextDouble() * 10 + 20).ToString("F1", CultureInfo.InvariantCulture);
                double currentWater = BinhNuoc01 != null ? BinhNuoc01._mucnuochientai : 0;
            
                string logData = $"$LOG,{DateTime.Now:yyyy-MM-dd},{randomTemp},{currentWater}#";
                lock (DATAlock) { DATAqueue.Enqueue(logData); }  

                string randomId = ids[rand.Next(ids.Length)];
                string status = rand.Next(0, 2) == 0 ? "ON" : "OFF";
                lock (DATAlock) { DATAqueue.Enqueue($"DEV:{randomId}:{status}"); }    
            }
        }
    }
