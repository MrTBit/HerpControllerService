using Newtonsoft.Json;

namespace HerpControllerService.Models.Device;

public class DeviceTimerEventModel
{
    [JsonProperty("state")]
    public int State { get; set; }
    [JsonProperty("pin")]
    public int Pin { get; set; }
}