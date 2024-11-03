using HerpControllerService.Enums;
using HerpControllerService.Models.Device;
using HerpControllerService.Services;
using HerpControllerService.SignalR;
using Newtonsoft.Json;

namespace HerpControllerService.mqtt.Processors;

public class ReceivedDirectResponseDataProcessor(ILogger<ReceivedDirectResponseDataProcessor> logger, DeviceService deviceService, DeviceCommandResponseSender deviceCommandResponseSender)
{
    public async Task Process(string deviceHardwareName, string message)
    {
        var response = JsonConvert.DeserializeObject<DeviceNonSensorResponseModel>(message);

        if (response == null)
        {
            logger.LogWarning("Device {deviceName} sent malformed response.", deviceHardwareName);
            return;
        }

        var device = await deviceService.GetDeviceByHardwareId(deviceHardwareName);

        if (device == null)
        {
            logger.LogWarning("Device {deviceName} not found.", deviceHardwareName);
            return;
        }
        
        switch (response.Type)
        {
            case DeviceNonSensorResponseType.CONFIG:
            {
                var config = JsonConvert.DeserializeObject<DeviceConfigModel>(response.Data);

                if (config == null)
                {
                    logger.LogWarning("Device {deviceName} sent malformed config.", deviceHardwareName);
                    return;
                }
                
                await deviceCommandResponseSender.SendCurrentDeviceConfig(device.Id, response.RequesterId, config);
                break;
            }
            case DeviceNonSensorResponseType.TIMERS:
            {
                var timers = JsonConvert.DeserializeObject<DeviceTimerPinStatesResponseModel>(response.Data);

                if (timers == null)
                {
                    logger.LogWarning("Device {deviceName} sent malformed timer pin state data.", deviceHardwareName);
                    return;
                }
                
                await deviceCommandResponseSender.SendTimerPinStates(device.Id, response.RequesterId, timers);
                break;
            }
        }
    }
}