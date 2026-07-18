using System;
using System.IO;
using System.Collections.Generic; // list
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

// bai tap ngay 10 
public static class SensorService
{
    public static double ChiaDuLieuCuaCamBien(string rawSensorData)
    {
        // khoi tao trang thai cong 
        bool isPortOpen = false;
        // tao khoi bat dau ktra loi 
        try
        {
            isPortOpen = true;
            Console.WriteLine("cong da duoc ket noi...");
            //ktra du lieu dau vao co bị null (trong rong hay khong )
            if(string.IsNullOrWhiteSpace(rawSensorData))
            {
                // neu phat hien loi ngay lap tuc ep gui ve tinh hinh loi chuyen xuong catch ngay 
                throw new ArgumentException("Du lieu cam bien gui ve bi loi, yeu cau ktra lai!!!");
            }
            // ap dung cat chuoi 
            string[] parts = rawSensorData.Split(',');
            // ktra xem sau khi cat chuoi có du 2 phan tu khong
            if(parts.Length<2)
            {
                throw new ArgumentException("dinh dang du lieu sai , can ktra lai !!!!!");
            }
            if (!double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double tuSo) || !double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double mauSo))
               // double.TryParse(...) su dung de ep tu dang du lieu chuoi chu thanh chuoi so 
            {
                throw new FormatException("du lieu chua duoc chuyen sang dinh dang khac, vui long ktra lai!");
            }
            if (mauSo == 0)
            {
                // ngoai le phai chi cho 0 
                throw new DivideByZeroException("Canh Bao: mau so khong hop le vui long ktra lai (mau so phai khac 0)!!!!.");
            }
            double KetQua = tuSo / mauSo;
            Console.WriteLine($"ket qua phep toan la:{tuSo}/{mauSo}={KetQua}");
            return KetQua;
        }
        // neu loi la viec ngoai le chia cho 0 day code vè dong nay tiep nhan xu ly
        catch (DivideByZeroException ex)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            // xu dung ex.message lay y nguyen dong throw o tren gan xuong 
            Console.WriteLine($"[LOI MAU SO BANG 0:]: {ex.Message}");
            Console.ResetColor();
            // dat code tra ve dang 0.0 de code khong bị crack 
            return 0.0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[LOI DINH DANG VUI LONG KTRA LAI]: {ex.Message}");
            Console.ResetColor();
            return 0.0;
        }
        finally
        {
            if (isPortOpen)
            {
                isPortOpen = false;
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine($"DA DONG CONG KET NOI >>>>> GIAI PHONG DU LIEU !");
                Console.ResetColor();
            }
        }
    }
}