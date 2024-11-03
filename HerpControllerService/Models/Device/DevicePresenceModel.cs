namespace HerpControllerService.Models.Device;

public class DevicePresenceModel
{
    public string? RequesterId { get; set; }
    public string DeviceName { get; set; } = null!;
}