using HerpControllerService.Models.Device;
using HerpControllerService.Services;
using HerpControllerService.SignalR;
using Newtonsoft.Json;

namespace HerpControllerService.mqtt.Processors;

public class ReceivedPresenceDataProcessor(ILogger<ReceivedPresenceDataProcessor> logger, DeviceService deviceService, DeviceCommandResponseSender deviceCommandResponseSender)
{
    public async Task Process(MqttService mqttService, string message)
    {
        var presenceData = JsonConvert.DeserializeObject<DevicePresenceModel>(message);

        if (presenceData == null)
        {
            logger.LogWarning("Received malformed presence data.");
            return;
        }

        var device = await deviceService.GetDeviceByHardwareId(presenceData.DeviceName);

        if (device == null)
        {
            device = await deviceService.CreateDevice(presenceData.DeviceName);
            await mqttService.SubscribeToNewDeviceTopics(device.HardwareId);
        }

        if (!string.IsNullOrWhiteSpace(presenceData.RequesterId))
        {
            await deviceCommandResponseSender.SendPresenceResponse(device.Id, presenceData.RequesterId);
        }
    }
}