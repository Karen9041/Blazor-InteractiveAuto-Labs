namespace TestPrototype.SharedUI.Models;

public class ActivityRecordDto
{
    public string Type { get; set; } = "戶外自行車"; // 運動種類
    public double Distance { get; set; } // 距離 (KM)
    public TimeSpan Duration { get; set; } // 花費時間
    public int HeartRate { get; set; } // 平均心率
}