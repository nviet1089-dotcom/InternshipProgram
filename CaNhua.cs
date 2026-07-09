using System;
using System.Collections.Generic; // list
using System.Linq;
//ngày 1 2 bai tap tạo class (thuộc tính và phương thức) có su dung đóng gói
class CanNhua
{
    private Double DungTichToiDa;
    private Double ChieuCaoCan;
    private Double MucNuocHienTai;

    public Double MucNuocHienTaiCuaCaNhuaThu1
    {
        get { return MucNuocHienTai;}
        set
        {
            if ( value >= 0 && value <= DungTichToiDa )
                MucNuocHienTai = value;
            else 
                Console.WriteLine($"loi: Muc Nuoc hien tai sai");
        }
    }
    public CanNhua(double SucChua, double ChieuCao)
    {
        this.DungTichToiDa = SucChua;
        this.ChieuCaoCan = ChieuCao;
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