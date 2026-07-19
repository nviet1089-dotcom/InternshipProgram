using System;
class program
//dotnet run --project NEWPROJECT
{
    static void Main(string[] args)
    {
        Console.WriteLine("======KHỞI TẠO BÌNH NƯỚC======");

        double succhua = 0;
        while (true)
        {
            Console.WriteLine("nhập dung tích của bình nước:");
            string input = Console.ReadLine() ?? string.Empty;
            if (double.TryParse(input, out succhua) && succhua > 0)
            {
                break;
            }
            Console.WriteLine("thông số sai, vui lòng nhập lại:");
        }
        
        double  chieucao = 0;
        while (true)
        {
            Console.WriteLine("nhập chiều cao của bình nước:");
            string input = Console.ReadLine() ?? string.Empty;
            if (double.TryParse(input, out chieucao) && chieucao > 0)
            {
                break;
            }
            Console.WriteLine("thông số sai, vui lòng nhập lại:");
        }
        BinhNuoc BinhNuoc01 = new BinhNuoc(succhua, chieucao);
        while(true)
        {
            Console.WriteLine($"nhập mức nước đã đổ vào (0 - {BinhNuoc01._dungtichtoida}):");
            string inputmucnuoc = Console.ReadLine() ?? string.Empty;
            if(double.TryParse(inputmucnuoc , out double mucnuoc ))
            {
                if(mucnuoc < 0)
                {
                    Console.WriteLine("thông tin nhập không đúng, vui lòng nhập lại:");
                }
                else  if(mucnuoc > BinhNuoc01._dungtichtoida)
                {
                    BinhNuoc01._mucnuochientai = mucnuoc;
                    Console.WriteLine("Cảnh báo bình đã đầy nước có thể tràn ra bên ngoài.!");
                    break;
                }
                else
                {
                    BinhNuoc01._mucnuochientai = mucnuoc;
                    Console.WriteLine("vui lòng đợi xử lí thông tin.");
                    break;
                }
            }
            else
            {
                Console.WriteLine("thông tin sai vui lòng nhập lại!");
            }
        }

        Console.WriteLine("vui lòng đợi giây lát");
        Console.WriteLine($"dung tích tối đa của bình nước là: {BinhNuoc01._dungtichtoida}.lit");
        Console.WriteLine($"mực nước hiện tại trong bình nước là: {BinhNuoc01._mucnuochientai}.lit");


        Console.WriteLine("===============KIỂM TRA THIẾT BỊ===============");
        List<thietbido> danhsachthietbi = new List<thietbido>();
        danhsachthietbi.Add(new CamBiennhietdo {mathietbicambien = "TEMP--001"});
        danhsachthietbi.Add(new Cameratruoc {mathietbicamera = "CAMERA--001"});
        if (danhsachthietbi != null && danhsachthietbi.Count > 0)
        {
            foreach(var thietbi in danhsachthietbi)
            {
                Console.WriteLine("===============KHỚI TẠO THIẾT BỊ===============");
                thietbi.mathietbido;
                thietbi.trangthaicuathietbi;
                if(thietbi is IConnectable thietbiketnoi)
                {
                    thietbiketnoi.ConnectSerial();
                }
            }
        }

    }
}
