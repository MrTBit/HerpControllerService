using HerpControllerService.Enums;
using Newtonsoft.Json;

namespace HerpControllerService.Models.Device;

public class DeviceConfigModel
{
    [JsonProperty("dhtSensors")]
    public List<DeviceConfigDhtSensorModel>? DhtSensors { get; set; }
    
    [JsonProperty("shtSensors")]
    public List<DeviceConfigShtSensorModel>? ShtSensors { get; set; }
    
    [JsonProperty("timers")]
    public List<DeviceConfigTimerModel>? Timers { get; set; }
    
    [JsonProperty("oneWirePin")]
    public int? OneWirePin { get; set; }
}

public class DeviceConfigDhtSensorModel()
{
    [JsonProperty("pin")]
    public int Pin { get; set; }
    
    [JsonProperty("type")]
    public DhtSensorType Type { get; set; }
}

public class DeviceConfigShtSensorModel()
{
    [JsonProperty("bus")]
    public int Bus { get; set; }
}

public class DeviceConfigTimerModel()
{
    [JsonProperty("pin")]
    public int Pin { get; set; }
    
    [JsonProperty("start")]
    public int StartTime { get; set; }
    
    [JsonProperty("end")]
    public int EndTime { get; set; }
    
    [JsonProperty("type")]
    public DeviceTimerType Type { get; set; }
}