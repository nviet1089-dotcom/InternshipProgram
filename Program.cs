using System;
using System.Collections.Generic; // list
using System.Linq;
// --- NƠI CHẠY CHƯƠNG TRÌNH ---
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("--- PHẦN 1: CAN NHUA ---");
        CanNhua CaNhuaThu1 = new CanNhua(5.5, 30.0);
        while (true)
        {
            Console.WriteLine("Nhap muc nuoc hien tai (0 - 5.5):");
            string inputMucNuoc = Console.ReadLine() ?? string.Empty; // Sửa lỗi tiềm ẩn null
            if (double.TryParse(inputMucNuoc, out double MucNuoc) && MucNuoc >= 0 && MucNuoc <= 5.5)
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
        Console.WriteLine($"Muc Nuoc Hien Tai Trong Ca: {CaNhuaThu1.MucNuocHienTaiCuaCaNhuaThu1}");

        Console.WriteLine("--- THIeT Bi ĐO ---");
        
        List<ThietBiDo> danhSachThietBi = new List<ThietBiDo>();
        
        danhSachThietBi.Add(new CumCamera { MaThietBiCamera = "CAM-001" });
        danhSachThietBi.Add(new CamBienNhietDo { MaThietBiCamBien = "SEN-001" });
        
        if (danhSachThietBi != null && danhSachThietBi.Count > 0)
        {
           foreach (var thietBi in danhSachThietBi)
           {
                Console.WriteLine("------------LOADING-----------");
                thietBi.MaThietBi();
                thietBi.TrangThai();
            
                if (thietBi is IConnectable thietBiKetNoi)
                {
                    thietBiKetNoi.ConnectSerial();
                }
           }
        }

        List<double> LichSuDo = new List<double>();
        
        string input; 
        
        Console.WriteLine("\n--- NHAP NHIET DO ---");
        while(true)
        {
            Console.Write("nhap nhiet do do (nhap 'ok' de dung): ");
            
            // Gán giá trị vào biến 'input' đã khai báo ở trên
            input = Console.ReadLine() ?? string.Empty; 
            
            if(input.ToLower() == "ok") break;

            if(double.TryParse(input, out double nhietDo))
            {
               LichSuDo.Add(nhietDo);
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

        Console.ReadLine();
    }
}