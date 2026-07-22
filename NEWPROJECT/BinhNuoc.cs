using System;
using System.IO;
using System.Collections.Generic; 
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
class BinhNuoc
{
    private double chieucaobinhnuoc;
    private double dungtichtoida;
    private double mucnuochientai;
    
    public double _chieucaobinhnuoc
    {
        get{return chieucaobinhnuoc;}
        set
        {
            if(value <= 10 && value >=100)
            chieucaobinhnuoc = value;
            else
            {
                Console.WriteLine("chiều cao không hợp lệ, vui lòng nhập lại");
                // chieucaobinhnuoc = 15
            }
        }
    }
    public double _dungtichtoida
    {
        get{return dungtichtoida;}
        set
        {
            if(value <= 10 && value >=50 )
            dungtichtoida = value;
            else
            {
                Console.WriteLine("dung tích không hợp lệ,vui lòng nhập lại");

            }
        }
    }
    public double _mucnuochientai
    {
        get{return mucnuochientai;}
        set
        {
            if(value >=0 && value <= dungtichtoida )
            mucnuochientai = value;
            else
            {
                Console.WriteLine("mực nước hiện tại không hợp lệ");
            }
        }
    }
    public BinhNuoc (double succhua, double chieucao)
    {
        this.chieucaobinhnuoc = chieucao;
        this.dungtichtoida = succhua;
        this.mucnuochientai = 0;
    }
     public void DoNuocVao(double lit)
    {
        if (this.mucnuochientai + lit > this.dungtichtoida )
        {
            this.mucnuochientai = this.dungtichtoida;
            Console.WriteLine($"bình đã đầy nước, nước đang bị tràn!");
        }
        else
        {
            this.mucnuochientai += lit; 
            Console.WriteLine($"mực nước hiện tại là: {this.mucnuochientai}.");
        }
    }

}
