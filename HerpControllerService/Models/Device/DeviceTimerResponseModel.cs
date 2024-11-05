using Newtonsoft.Json;

namespace HerpControllerService.Models.Device;

public class DeviceTimerResponseModel
{
    [JsonProperty("timer")] 
    public DeviceTimerEventModel Timer { get; set; } = null!;
}