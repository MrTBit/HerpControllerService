namespace HerpControllerService.Models.Device;

public class DeviceSensorDataModel
{
    public List<DeviceDhtSensorModel>? DhtSensors { get; set; }
    public List<DeviceDs18bSensorModel>? Ds18BSensors { get; set; }
    public List<DeviceShtSensorModel>? ShtSensors { get; set; }
}