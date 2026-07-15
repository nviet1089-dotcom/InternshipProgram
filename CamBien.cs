using System;
using System.Collections.Generic; // list
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
// cam bien nhiet do
public class CamBienNhietDo : ThietBiDo , IConnectable
{
    public string MaThietBiCamBien {get; set; } = string.Empty;
    public override void MaThietBi()
    {
        base.MaThietBi();
        Console.WriteLine(MaThietBiCamBien);
    }
    public List<double> LichSuDo { get; set; } = new List<double>();

    public void ThemDuLieu(double nhietDo)
    {
        LichSuDo.Add(nhietDo);
    }
    public override void TrangThaiHoatDong()
    {
        Console.WriteLine($"trang thai hoat dong:true");
    }
    public void ConnectSerial()
    {
        Console.WriteLine($"Cam bien: dang mo cong COM de nhan tin hieu so...");
    }
}