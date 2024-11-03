using HerpControllerService.Models.Device;
using Microsoft.AspNetCore.SignalR;

namespace HerpControllerService.SignalR;

public class DeviceCommandResponseSender(IHubContext<DeviceCommandHub> hub)
{
    public async Task SendPresenceResponse(long deviceId, string requesterId) =>
        await hub.Clients.User(requesterId).SendAsync("AnnouncePresenceResponse", deviceId);

    public async Task SendCurrentDeviceConfig(long deviceId, string requesterId, DeviceConfigModel config) =>
        await hub.Clients.User(requesterId).SendAsync("DeviceConfigResponse", deviceId, config);
    
    public async Task SendTimerPinStates(long deviceId, string requesterId, DeviceTimerPinStatesResponseModel model) =>
        await hub.Clients.User(requesterId).SendAsync("DeviceTimerPinStatesResponse", deviceId, model);
}