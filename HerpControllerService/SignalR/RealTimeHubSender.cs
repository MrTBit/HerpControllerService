using HerpControllerService.Models;
using Microsoft.AspNetCore.SignalR;

namespace HerpControllerService.SignalR;

public class RealTimeHubSender(IHubContext<RealTimeHub> hub)
{
    public async Task BroadcastRealTimeSensorData(RealTimeSensorDataModel model) => await hub.Clients.All.SendAsync("ReceivedRealTimeSensorData", model);
}