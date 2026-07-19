using System;
public class cumcamera : thietbido
{
    public string mathietbicambien {get; set; } = string.Empty;
    public override void tenthietbido()
    {
        base.tenthietbid();
        Console.WriteLine($"mã của thiết bị cảm biến :");
    }
    public override void trangthaicuathietbi()
    {
        base.trangthaicuathietbi();
        Console.WriteLine($"trạng thái hoạt động của thiết bị:");
    }


}