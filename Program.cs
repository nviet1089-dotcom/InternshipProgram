using System;
using System.IO;
using System.Collections.Generic; // list
using System.Linq;// thuc hien cac phep 
using System.Threading.Tasks;
using System.Globalization;// xu ly dau cham trong day 
// InternshipProgram
// --- NoI CHaY CHuoNG TRiNH ---
class Program
{
    // tao hang doi queue
    private static Queue<string> _sensorQueue = new Queue<string>();
    private static readonly object _queueLock = new object();
    // tạo dicrory
    private static Dictionary<string, ThietBiDo> _deviceRegistry = new Dictionary<string, ThietBiDo>();
    private static readonly object _dictLock = new object();
    static void Main(string[] args)
    {
        
        //tao thiet bi
        _deviceRegistry.Add("CAM_01", new ThietBiDo { TenThietBi = "Camera Cổng", TrangThai = "OFF" });
        _deviceRegistry.Add("TEMP_01", new ThietBiDo { TenThietBi = "Cảm biến Nhiệt", TrangThai = "OFF" }); 
        //them ham de khoi dong
        //Task.Run(() => BackgroundProcessor());   
        //Task.Run(() => SensorSimulator());
        
        Console.WriteLine("--- CAN NHUA ---");
        CanNhua CaNhuaThu1 = new CanNhua(5.5, 30.0);
        while (true)
        {
            Console.WriteLine("Nhap muc nuoc hien tai (0 - 5.5):");
            string inputMucNuoc = Console.ReadLine() ?? string.Empty; // tranh loi null
            if (double.TryParse(inputMucNuoc, out double MucNuoc))
            {// chinh sua lai logic de donh nhat voi file CaNhua.cs va file program 
                if (MucNuoc >= 0 && MucNuoc <= CaNhuaThu1.DungTichToiDaCuaCaNhuaThu1)
               {
                CaNhuaThu1.MucNuocHienTaiCuaCaNhuaThu1 = MucNuoc;
                Console.WriteLine("Thong tin da duoc tiep nhan");
                break;
            }
            else
            {
                Console.WriteLine("Thong tin sai, yeu cau nhap lai:");
            }
        }
        else
            {
                Console.WriteLine("dinh dang sai yeu cau chinh sua lai:");
            }
        }
        Console.WriteLine($"Muc Nuoc Hien Tai Trong Ca: {CaNhuaThu1.MucNuocHienTaiCuaCaNhuaThu1}");


        // bai 10 nhap va chay thu giai lap chia cac so voi cau truc try catch finally
        Console.WriteLine("\n CHAY THU PHEP CHIA DU LIEU CUA CAM BIEN GUI VE");
        Console.WriteLine("NHAP CHUOI GIA LAP CAM BIEN CO DANG( 100,0 ) VUI LONG NHAP:" );
        Console.WriteLine("VUI LONG NHAP:");

        string chuoiThoInput = Console.ReadLine() ?? string.Empty;

        double ketQuaChia = SensorService.ChiaDuLieuCuaCamBien(chuoiThoInput);
        Console.WriteLine($"=> Kết quả xử lý cuối cùng: {ketQuaChia}");



        // bai cua tuan truoc
        Console.WriteLine("--- THIeT Bi DO ---");
        
        List<ThietBiDo> danhSachThietBi = new List<ThietBiDo>();
        
        danhSachThietBi.Add(new CumCamera { MaThietBiCamera = "CAM-001" });
        danhSachThietBi.Add(new CamBienNhietDo { MaThietBiCamBien = "SEN-001" });
        
        if (danhSachThietBi != null && danhSachThietBi.Count > 0)
        {
           foreach (var thietBi in danhSachThietBi)
           {
                Console.WriteLine("------------LOADING-----------");
                thietBi.MaThietBi();
                thietBi.TrangThaiHoatDong();
            
                if (thietBi is IConnectable thietBiKetNoi)
                {
                    thietBiKetNoi.ConnectSerial();
                }
           }
        }

        List<double> LichSuDo = new List<double>();
        
        string input; 
        
        Console.WriteLine("\n--- NHIET DO tai cam bien ---");
        while(true)
        {
            Console.Write("nhap nhiet do do (nhap 'ok' de dung): ");
            input = Console.ReadLine() ?? string.Empty; 
            
            if(input.ToLower() == "ok") break;

            if(double.TryParse(input, out double nhietDo))
            {
               LichSuDo.Add(nhietDo);
               lock (_queueLock) { _sensorQueue.Enqueue($"NhietDo: {nhietDo} | Time: {DateTime.Now:HH:mm:ss}"); }
            }
            else
            {
               Console.WriteLine("vui long nhap lai du lieu"); 
            }
        }
        if(LichSuDo.Count > 0) 
        {
            double trungbinh = LichSuDo.Average();
            double nhietdocaonhat = LichSuDo.Max();
            Console.WriteLine($"trung binh nhiet do:{trungbinh:F2}");
           Console.WriteLine($"nhiet do lon nhat do duoc:{nhietdocaonhat}");
        }
        else
        {
            Console.Write($"khong the tinh duoc!!!");
        }
        Console.ForegroundColor = ConsoleColor.Yellow;
        // thay chay giai lap o day de ctrinh chay muot ma hon
        Console.WriteLine("\n======================================================");
        Console.WriteLine("      ket thuc nhap du lieu    !!!!!!!");
        Console.WriteLine("\n   VUI LONG CHO TRONG GIAY LAT       ");
        Console.WriteLine("    BAT DAU CHAY GIA LAP VA BACKGROUND !!!");
        Console.WriteLine("\n======================================================");
        Console.ResetColor();
        // chuyen giai lap xuong day 
        Task.Run(() => BackgroundProcessor());   
        Task.Run(() => SensorSimulator());
        Console.ReadLine();

    }    
    // phuong thuc su li bai9 
    public static bool ArduinoData(string rawData , out DateTime date , out double temp , out double waterlevel )
    {
        date = DateTime.MinValue ;
        temp = 0.0 ;
        waterlevel = 0.0 ;
        // xet tinh toan ven cua chuoi 
        if(string.IsNullOrEmpty(rawData) || !rawData.StartsWith("$LOG,") || !rawData.EndsWith("#"))
        {
            return false;
        }
        try
        {
           // lam sach chuoi  
           string cleanData = rawData.TrimEnd('#').Substring(1);
           // tach dau ","
           string[] parts = cleanData.Split(',');
           // xac dinh so luong phan tu co du khong
           if(parts.Length !=4) return false;
           // kiem tra dinh danh log 
           if(parts[0] != "LOG") return false;
           // gan ep du lieu
           if(!DateTime.TryParse(parts[1], out date )) return false;
           if (!double.TryParse(parts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out temp)) return false;
        // if(!double.TryParse(parts[2], out temp)) return;
        //CultureInfo.InvariantCulture ham nay ep may tinh su dung dau '.' la dau danh dau thap phan
           if (!double.TryParse(parts[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out waterlevel)) return false;
        // // if(!double.TryParse(parts[2], out waterlevel)) return;
        //System.Globalization ham nay giup may tinh fix loi khong co dau '.' vd 26.8
           return true;// phai dung true neu dung false khong tra ve du lieu
        }
        catch
        {
            return false;
        }
    }
    // tao background
    static async Task BackgroundProcessor()
     {
        while (true)
        {
            await Task.Delay(10000);
            
                List<string> itemsToProcess = new List<string>();

                lock (_queueLock)
                {
                   while (_sensorQueue.Count > 0)
                {
                    itemsToProcess.Add(_sensorQueue.Dequeue());
                }
                }

                    if (itemsToProcess.Count > 0)
                {
                   Console.ForegroundColor = ConsoleColor.DarkRed;
                   Console.WriteLine($"\n[Background] Xu ly {itemsToProcess.Count} goi tin tu Queue:");
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
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"log ngay:{date:yyyy-MM-dd} , nhiet do:{temp:F1} do , muc nuoc:{waterlevel}cm ");
                        Console.ResetColor();

                        // them de biet duoc day la cua file CSV
                        Logger.LogCSV(date, temp, waterlevel);
                    }
                    else
                    {
                        Console.WriteLine("loi cu phap yeu cau kiem tra lai");
                    }
                }
                else
                {
                string[] parts = item.Split(':');
                if (parts.Length < 2) continue; // neu loi ham nay giup bo qua

                string prefix = parts[0];

                // xu li thiet bi
                if (prefix == "DEV") 
                {
                    // dinh dang cho id
                    if (parts.Length >= 3)
                    {
                        string id = parts[1];
                        string newStatus = parts[2];

                        lock (_dictLock)//su dung tu khoa lock
                        {
                            if (_deviceRegistry.TryGetValue(id, out ThietBiDo thietBi))
                            {
                                thietBi.TrangThai = newStatus;
                                Console.WriteLine($"[Update] {thietBi.TenThietBi} ({id}) chuyen sang : {thietBi.TrangThai}");
                            }
                            else
                            {
                                Console.WriteLine($"[Cảnh báo] ID thiet bi khong hop le : {id}");
                            }
                        }
                    }
                }
                // phan in nhiet do 
                else if (prefix == "TEMP") 
                {
                    // dinnh dang: TEMP:Value
                    string tempValue = parts[1];
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"[TEMP] nhiet do do duoc : {tempValue}°C");
                    Console.ResetColor();
                }
            }
                }
            
        }
     }
     static void RunTest()
{
    // Chuoi du lieu ban muon test
    string testData = "$LOG,2026-07-03,28.5,45#";

    Console.WriteLine("--- CHe do TEST ---");
    Console.WriteLine($"dang day chuoi: {testData}");

    lock (_queueLock) 
    { 
        _sensorQueue.Enqueue(testData); 
    }
}
     // gia lap cu 1s gui du lieu
    static async Task SensorSimulator()
    {
        //string testData = "$LOG,2026-07-03,28.5,45#";
        string[] ids = { "CAM_01", "TEMP_01" };
        //random so lieu
        Random rand = new Random();
        //dat vong lap
        while (true)
        {
            await Task.Delay(10000); // thoi gian chay 1p

            string data = $"AUTO_SENSOR | Temp: {rand.Next(20, 35)}°C | Time: {DateTime.Now:HH:mm:ss}";
            lock (_queueLock) { _sensorQueue.Enqueue(data); }

            string logData = $"$LOG,{DateTime.Now:yyyy-MM-dd},{(rand.NextDouble() * 10 + 20).ToString("F1", System.Globalization.CultureInfo.InvariantCulture)},{rand.Next(10, 50)}#";
            lock (_queueLock) { _sensorQueue.Enqueue(logData); }                                     // ep từ 2,8 de no khong hien thi thanh 2,6 ma hien thi thanh 2.6

            string randomId = ids[rand.Next(ids.Length)];
            string status = rand.Next(0, 2) == 0 ? "ON" : "OFF";
            lock (_queueLock) { _sensorQueue.Enqueue($"DEV:{randomId}:{status}"); }
        }
    }    

}
