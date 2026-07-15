using System;
using System.Collections.Generic; // list
using System.Linq;
using System.Globalization;
//ngay 1 2 bai tap tao class (thuoc tinh va phuong thuc) co su dung dong goi
class CanNhua
{
    //tinh dong goi 
    private double DungTichToiDa;
    private double ChieuCaoCan;
    private double MucNuocHienTai;
// them thuc tinh chp dung tic toi da va chieu cao
    public double DungTichToiDaCuaCaNhuaThu1
    {
        get { return DungTichToiDa;}
        set
        {
            if ( value >= 5 && value <= 15 )
                DungTichToiDa = value;
            else
            {
                Console.WriteLine($"loi: dung tich toi thieu la 5 va toi da la 15!!!");
            }
        }
    }
    public double ChieuCaoCanCuaCaNhuaThu1
    {
        get { return ChieuCaoCan;}
        set
        {
            if ( value >= 20 && value <= 50 )
                ChieuCaoCan = value;
            else
            {           
               Console.WriteLine($"loi: chieu cao can toi thieu la 20va toi da la!!!!");
            }
        }
    }
    public double MucNuocHienTaiCuaCaNhuaThu1
    {
        get { return MucNuocHienTai;}
        set
        {
            if ( value >= 0 && value <= DungTichToiDa )
                MucNuocHienTai = value;
            else 
            {
                Console.WriteLine($"loi: Muc Nuoc hien tai sai");
            }
        }
    }
    public CanNhua(double SucChua, double ChieuCao)
    {
        this.DungTichToiDaCuaCaNhuaThu1 = SucChua;
        this.ChieuCaoCanCuaCaNhuaThu1 = ChieuCao;
        this.MucNuocHienTai = 0;
    }
        public void DoNuocVao(double lit)
    {
        if (this.MucNuocHienTai + lit > this.DungTichToiDa )
        {
            this.MucNuocHienTai = this.DungTichToiDa;
            Console.WriteLine($"Nuoc tran !!!!!");
        }
        else
        {
            this.MucNuocHienTai += lit; 
            Console.WriteLine($"Muc nuoc hien tai la: {this.MucNuocHienTai}.");
        }
    }
}