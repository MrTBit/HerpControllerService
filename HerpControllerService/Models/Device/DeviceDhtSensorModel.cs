namespace HerpControllerService.Models.Device;

public class DeviceDhtSensorModel
{
    public int Pin { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
}