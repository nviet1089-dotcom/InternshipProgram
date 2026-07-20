using System;
using System.IO;
using System.Collections.Generic; // list
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
public class cumcamera : thietbido
{
    public string mathietbicamera {get; set; } = string.Empty;
    public override voi tenthietbido()
    {
        base.tenthietbido();
        Console.WriteLine("mã của thiết bị camera:");
    }
    public override void trangthaicuathietbi()
    {
        base.trangthaicuathietbi();
        Console.WriteLine($"trạng thái hoạt động của thiết bị:");
    }
}