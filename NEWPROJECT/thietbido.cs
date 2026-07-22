using System;
using System.IO;
using System.Collections.Generic; 
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

public class thietbido
{
    public string tenthietbido {get; set; } = string.Empty;
    public string trangthai {get; set; } = string.Empty;
    public virtual void mathietbido()
    {
        Console.WriteLine("mã của thiết bị:");
    }
    public virtual void trangthaicuathietbi()
    {
        Console.WriteLine("trạng thái hoạt động của thiết bị:");
    }
}
