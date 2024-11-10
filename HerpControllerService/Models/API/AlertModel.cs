using HerpControllerService.Enums;

namespace HerpControllerService.Models.API;

public class AlertModel : BaseApiModel
{
    public long? DeviceId { get; set; }
    public DeviceModel? Device { get; set; }
    
    public long? SensorId { get; set; }
    public SensorModel? Sensor { get; set; }
    
    public AlertType Type { get; set; }
    
    public string Message { get; set; }
    
    public AlertStatus Status { get; set; }
}