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
    // voi key la ma thiet bị string va value la doi tuong cua thietbido
    private static readonly object _dictLock = new object();
    //Xử lý bất đồng bộ, đa luồng an toàn khi thiết bị giả lập gửi dữ liệu về liên tục.

    static void Main(string[] args)
    {
        
        //tao thiet bi cho dictionary voi cac ma thiet bi sau
        _deviceRegistry.Add("CAM_01", new ThietBiDo { TenThietBi = "Camera Cong", TrangThai = "OFF" });
        //key la cam01 cong con value la camera cong co gia tri trang thai la off
        // tuong tu voi temp
        _deviceRegistry.Add("TEMP_01", new ThietBiDo { TenThietBi = "Cam bien Nhiet", TrangThai = "OFF" }); 
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
            {
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
            // tinh truu tuong
                if (thietBi is IConnectable thietBiKetNoi)
                // gọi xem co ket noi theo chuan truu tuong hay khong
                {
                
                    thietBiKetNoi.ConnectSerial();
                }
           }
        }

        Console.WriteLine("\n CHAY THU PHEP CHIA DU LIEU CUA CAM BIEN GUI VE");
        Console.WriteLine("NHAP CHUOI GIA LAP CAM BIEN CO DANG( 100,0 ) VUI LONG NHAP:" );
        Console.WriteLine("VUI LONG NHAP:");

        string chuoiThoInput = Console.ReadLine() ?? string.Empty;

        double ketQuaChia = SensorService.ChiaDuLieuCuaCamBien(chuoiThoInput);
        Console.WriteLine($"=> Kết quả xử lý cuối cùng: {ketQuaChia}");



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
        // thay chay gia lap o day de ctrinh chay muot ma hon
        Console.WriteLine("\n======================================================");
        Console.WriteLine("      ket thuc nhap du lieu    !!!!!!!");
        Console.WriteLine("\n   VUI LONG CHO TRONG GIAY LAT       ");
        Console.WriteLine("    BAT DAU CHAY GIA LAP VA BACKGROUND !!!");
        Console.WriteLine("\n======================================================");
        Console.ResetColor();
        // chuyen gia lap xuong day 
        Task.Run(() => BackgroundProcessor());   
        Task.Run(() => SensorSimulator());
        Console.ReadLine();

    }    
    // phuong thuc su li bai9 
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

        if (parts.Length != 4 || parts[0] != "LOG") 
        {
            return false;
        }

        if (!DateTime.TryParse(parts[1], CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) 
        {
            return false;
        }

        if (!double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out temp)) 
        {
            return false;
        }

        if (!double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out waterlevel)) 
        {
            return false;
        }

        return true; 
    }
    catch
    {
        return false; 
    }
}
    static async Task BackgroundProcessor()
     {
        while (true)
        {
            await Task.Delay(10000);
            
                // khởi tạo danh sách kiểu chuỗi để chứa các gói tin chuẩn bị được nôi ra khỏi hàm queue
                List<string> itemsToProcess = new List<string>();

                //khoa va lay du lieu tu queue ra de xu li 
                lock (_queueLock)
                {
                   while (_sensorQueue.Count > 0)
                {
                    // lay goi tin ra khoi queue hang doi va nap vao danh sach tam thoi 
                    itemsToProcess.Add(_sensorQueue.Dequeue());
                }
                }

                    // xu li goi tin da lay ra đảm bảo trong danh sách có ít nhất 1 phần tử 
                    if (itemsToProcess.Count > 0)
                {
                   Console.ForegroundColor = ConsoleColor.DarkRed;
                   Console.WriteLine($"\n[Background] Xu ly {itemsToProcess.Count} goi tin tu Queue:");
                   //duyệt gói tin trong danh sách tạm thời để in gói tin ra màn hình 
                   foreach (var item in itemsToProcess)
                       {
                         Console.WriteLine($"   => {item}");
                       }
                      Console.ResetColor();
                 }
       
                 foreach (var item in itemsToProcess)
                {
                    //kiểm tra định dạng của chuôi giả lập arduino gửi về 
                    if(item.StartsWith("$LOG") && item.EndsWith("#"))
                {
                    if(ArduinoData(item, out DateTime date,out double temp ,out double waterlevel))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"log ngay:{date:yyyy-MM-dd} , nhiet do:{temp:F1} do , muc nuoc:{waterlevel}cm ");
                        Console.ResetColor();

                        // gọi hàm kết nối file cua class logger  CSV bai 11 
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

                string prefix = parts[0];// lấy phần tử đầu tiên 

                // xu li thiet bi
                if (prefix == "DEV") 
                {
                    // dinh dang cho id cua day 8 
                    //ktra xem gói tin có đầy đủ 3 thành phần không 
                    if (parts.Length >= 3)
                    {
                        string id = parts[1];
                        string newStatus = parts[2];

                        lock (_dictLock)//su dung de dam bao an toan khi truy cap dictonary luc dang cap nhat
                        {
                            // dua key vao xu li nhan ra value
                            if (_deviceRegistry.TryGetValue(id, out ThietBiDo thietBi))
                            // khong dung for nen doi su dung TryGetValue de do phuc tap trong thoi gian luon la O(1)
                            {
                                // sau khi khoa lai ms chạy viec thay doi du ieu thiet bi
                                //cap nhat trang thai ruc tiep vao doi tuong tren Ram 
                                thietBi.TrangThai = newStatus;
                                Console.WriteLine($"[Update] {thietBi.TenThietBi} ({id}) chuyen sang : {thietBi.TrangThai}");
                            }
                            else
                            {
                                // xu li khi khong tim thay id thiet bi
                                Console.WriteLine($"[Cảnh báo] ID thiet bi khong hop le : {id}");
                            }
                        }// mo khoa de chay luong du lieu khac 
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
     static void Test()
{
    // Chuoi du lieu ban muon test // bai 9 
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
    string[] ids = { "CAM_01", "TEMP_01" };
    Random rand = new Random();

    while (true)
    {
        await Task.Delay(10000); 

        string data = $"AUTO_SENSOR | Temp: {rand.Next(20, 35)}°C | Time: {DateTime.Now:HH:mm:ss}";
        lock (_queueLock) { _sensorQueue.Enqueue(data); }

        string logData = $"$LOG,{DateTime.Now:yyyy-MM-dd},{(rand.NextDouble() * 10 + 20).ToString("F1", System.Globalization.CultureInfo.InvariantCulture)},{rand.Next(10, 50)}#";
        lock (_queueLock) { _sensorQueue.Enqueue(logData); }  

        string randomId = ids[rand.Next(ids.Length)];
        string status = rand.Next(0, 2) == 0 ? "ON" : "OFF";
        lock (_queueLock) { _sensorQueue.Enqueue($"DEV:{randomId}:{status}"); }    
    }
}

}
