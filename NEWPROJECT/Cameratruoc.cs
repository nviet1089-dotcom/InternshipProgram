using System;
using System.IO;
using System.Collections.Generic; 
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
public class CameraTruoc : thietbido
{
    public string mathietbicamera {get; set; } = string.Empty;
    public override void mathietbido()
    {
        base.mathietbido();
        Console.WriteLine("mã của thiết bị camera:");
    }
    public override void trangthaicuathietbi()
    {
        base.trangthaicuathietbi();
        Console.WriteLine($"trạng thái hoạt động của thiết bị:");
    }
}