using HerpControllerService.Models.Device;
using HerpControllerService.Services;
using Newtonsoft.Json;

namespace HerpControllerService.mqtt.Processors;

public class ReceivedTimerDataProcessor(ILogger<ReceivedTimerDataProcessor> logger, DeviceService deviceService, TimerPinStateService timerPinStateService)
{
    public async Task Process(string deviceHardwareName, string message)
    {
        var timerData = JsonConvert.DeserializeObject<DeviceTimerEventModel>(message);

        if (timerData == null)
        {
            logger.LogWarning("Received malformed sensor data from device {deviceId}.", deviceHardwareName);
            return;
        }
        
        var device = await deviceService.GetDeviceByHardwareId(deviceHardwareName);

        if (device == null)
        {
            logger.LogWarning("Received message from device {deviceId} not found in db.", deviceHardwareName);
            return;
        }

        await timerPinStateService.CreateTimerPinState(device, timerData.Pin, timerData.State);
    }
}