namespace HerpControllerService.Models;

public class RealTimeSensorDataModel
{
    public long DeviceId { get; set; }
    public List<RealTimeSensorReadingModel> SensorData { get; set; } = [];
}

public class RealTimeSensorReadingModel
{
    public long SensorId { get; set; }
    public double Temperature { get; set; }
    public double? Humidity { get; set; }
}