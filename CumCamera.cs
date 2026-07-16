using System;
using System.IO;
using System.Collections.Generic; // list
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
public class CumCamera : ThietBiDo , IConnectable
// tinh da hinh va ke thua
{
    public string MaThietBiCamera {get; set; } = string.Empty;
    public override void MaThietBi()
    {
        base.MaThietBi();
        Console.WriteLine(MaThietBiCamera);
    }
    public override void TrangThaiHoatDong()
    {
        Console.WriteLine($"trang thai hoat dong:true");
    }
    public void ConnectSerial()
    {
        Console.WriteLine($"Camera: Đang mo luong video tu cong serial...");
    }
}