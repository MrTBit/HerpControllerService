using HerpControllerService.Enums;

namespace HerpControllerService.Models.API;

public class SensorModel : BaseApiModel
{
    public string HardwareId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public SensorType Type { get; set; }
    public double? MinimumTemperature { get; set; }
    public double? MaximumTemperature { get; set; }
    public double? MinimumHumidity { get; set; }
    public double? MaximumHumidity { get; set; }
}