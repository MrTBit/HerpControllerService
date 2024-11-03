namespace HerpControllerService.Models.API;

public class DeviceModel : BaseApiModel
{
    public string HardwareId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<SensorModel>? Sensors { get; set; }
}