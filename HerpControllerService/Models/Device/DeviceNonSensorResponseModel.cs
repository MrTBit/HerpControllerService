using HerpControllerService.Enums;

namespace HerpControllerService.Models.Device;

public class DeviceNonSensorResponseModel
{
    public DeviceNonSensorResponseType Type { get; set; }
    public string Data { get; set; } = null!;
    public string RequesterId { get; set; } = null!;
}