using System;
using System.Linq;
using System.Collections.Generic; // list
using System.Threading.Tasks;
using System.Globalization;
public class ThietBiDo
{
    public string TenThietBi { get; set; }
    public string TrangThai { get; set; }
    public virtual void MaThietBi() 
    {
         Console.WriteLine($"ma cua thiet bi:");
    }
    public virtual void TrangThaiHoatDong()
    {
     Console.WriteLine($"trang thai hoat dong:"); 
    }  
}
