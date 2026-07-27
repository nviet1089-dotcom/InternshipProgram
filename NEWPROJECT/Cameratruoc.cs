using System;
using System.IO;
using System.Collections.Generic; 
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
public class CameraTruoc : thietbido
{
    public string mathietbicamera {get; set; } = string.Empty;
    public override void mathietbido()
    {
        base.mathietbido();
        Console.WriteLine("mã của thiết bị camera:CAM--001");
    }
    public override void trangthaicuathietbi()
    {
        base.trangthaicuathietbi();
        Console.WriteLine($"trạng thái hoạt động của thiết bị:TRUE");
    }
    public void ConnectSerial()
    {
        Console.WriteLine($"Camera: Đang mo luong video tu cong serial...");
        try
        {
            Mat testImage = new Mat(100, 100, DepthType.Cv8U, 3);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[EmguCV OK] Khởi tạo Mat thành công! Kích thước: {testImage.Width}x{testImage.Height}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[EmguCV LỖI]: {ex.Message}");
            Console.ResetColor();
        }
    }
}