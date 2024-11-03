using HerpControllerService.Enums;

namespace HerpControllerService.Models.API;

public class SensorModel : BaseApiModel
{
    public string HardwareId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public SensorType Type { get; set; }
}