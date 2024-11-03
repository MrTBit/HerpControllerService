using System.Net;
using HerpControllerService.Exceptions;
using HerpControllerService.Models.Device;
using HerpControllerService.mqtt;
using HerpControllerService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerpControllerService.Controllers;

[Authorize]
[Route("device-commands")]
public class DeviceCommandController(ILogger<DeviceCommandController> logger, DeviceService deviceService, MqttService mqttService) : BaseController(logger)
{
    [HttpPost("{deviceId:long}/0")]
    public async Task<ActionResult> SendSendSensorDataCommand(long deviceId) => await this.BuildResponseAsync(
        async () =>
        {
            var device = (await deviceService.GetDevices([deviceId])).FirstOrDefault();

            if (device == null)
            {
                throw new HerpControllerException(HttpStatusCode.BadRequest, "Device not found.");
            }

            await mqttService.SendRequestSensorDataCommand(device.HardwareId);

            return Ok();
        });

    [HttpPost("{deviceId:long}/1")]
    public async Task<ActionResult> SendNewConfigToDevice(long deviceId, [FromBody] DeviceConfigModel newConfig) =>
        await this.BuildResponseAsync(
            async () =>
            {
                var device = (await deviceService.GetDevices([deviceId])).FirstOrDefault();

                if (device == null)
                {
                    throw new HerpControllerException(HttpStatusCode.BadRequest, "Device not found.");
                }

                if (newConfig == null)
                {
                    throw new HerpControllerException(HttpStatusCode.BadRequest, "New device config is required.");
                }
                
                await mqttService.SendNewDeviceConfig(device.HardwareId, newConfig);

                return Ok();
            });

    [HttpPost("{deviceId:long}/2")]
    public async Task<ActionResult> SendNewSensorPollingInterval(long deviceId, [FromQuery] long interval) =>
        await this.BuildResponseAsync(
            async () =>
            {
                var device = (await deviceService.GetDevices([deviceId])).FirstOrDefault();

                if (device == null)
                {
                    throw new HerpControllerException(HttpStatusCode.BadRequest, "Device not found.");
                }

                if (interval < 1000)
                {
                    throw new HerpControllerException(HttpStatusCode.BadRequest, "Interval must be > 1000");
                }
                
                await mqttService.SendDeviceChangeSensorPollingIntervalCommand(device.HardwareId, interval);

                return Ok();
            });

    [HttpPost("{deviceId:long}/3")]
    public async Task<ActionResult> SendAnnouncePresenceCommand(long deviceId) =>
        await this.BuildResponseAsync(async () =>
        {
            var device = (await deviceService.GetDevices([deviceId])).FirstOrDefault();

            if (device == null)
            {
                throw new HerpControllerException(HttpStatusCode.BadRequest, "Device not found.");
            }

            // Should never happen
            if (User.Identity?.Name == null)
            {
                throw new HerpControllerException(HttpStatusCode.InternalServerError, "Unauthenticated user somehow.");
            }

            await mqttService.SendAnnouncePresenceCommand(device.HardwareId, User.Identity.Name);

            return Ok();
        });

    [HttpPost("all/3")]
    public async Task<ActionResult> SendAnnouncePresenceCommandToAll() => await this.BuildResponseAsync(async () =>
    {
        // Should never happen
        if (User.Identity?.Name == null)
        {
            throw new HerpControllerException(HttpStatusCode.InternalServerError, "Unauthenticated user somehow.");
        }

        await mqttService.SendAnnouncePresenceCommandToAll(User.Identity.Name);

        return Ok();
    });

    [HttpPost("{deviceId:long}/4")]
    public async Task<ActionResult> SendSendCurrentConfigCommand(long deviceId) => await this.BuildResponseAsync(
        async () =>
        {
            var device = (await deviceService.GetDevices([deviceId])).FirstOrDefault();

            if (device == null)
            {
                throw new HerpControllerException(HttpStatusCode.BadRequest, "Device not found.");
            }

            // Should never happen
            if (User.Identity?.Name == null)
            {
                throw new HerpControllerException(HttpStatusCode.InternalServerError, "Unauthenticated user somehow.");
            }

            await mqttService.SendSendCurrentConfigCommand(device.HardwareId, User.Identity.Name);

            return Ok();
        });

    [HttpPost("{deviceId:long}/5")]
    public async Task<ActionResult> SendSendCurrentTimerPinStates(long deviceId) => await this.BuildResponseAsync(
        async () =>
        {
            var device = (await deviceService.GetDevices([deviceId])).FirstOrDefault();

            if (device == null)
            {
                throw new HerpControllerException(HttpStatusCode.BadRequest, "Device not found.");
            }

            // Should never happen
            if (User.Identity?.Name == null)
            {
                throw new HerpControllerException(HttpStatusCode.InternalServerError, "Unauthenticated user somehow.");
            }
            
            await mqttService.SendSendCurrentTimerPinStatesCommand(device.HardwareId, User.Identity.Name);

            return Ok();
        });
}