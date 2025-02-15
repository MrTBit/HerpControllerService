using HerpControllerService.Models.API;
using HerpControllerService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerpControllerService.Controllers;

[Route("devices")]
public class DeviceController(ILogger<DeviceController> logger, DeviceService deviceService) : BaseController(logger)
{
    [AllowAnonymous]
    [HttpGet("all")]
    public async Task<ActionResult<List<DeviceModel>>> GetAllDevices() => await this.BuildResponseAsync<List<DeviceModel>>(
        async () =>
        {
            return Ok((await deviceService.GetDevices(null)).Select(d => (DeviceModel)d).ToList());
        });

    [Authorize]
    [HttpGet("by-id")]
    public async Task<ActionResult<List<DeviceModel>>> GetByIds([FromQuery] List<long> ids) =>
        await this.BuildResponseAsync<List<DeviceModel>>(
            async () =>
            {
                if (ids.Count == 0)
                {
                    return BadRequest();
                }

                return Ok((await deviceService.GetDevices(ids)).Select(d => (DeviceModel)d).ToList());
            });

    [Authorize]
    [HttpPut("{id:long}")]
    public async Task<ActionResult<DeviceModel>> Update(long id, [FromBody]DeviceModel deviceModel) =>
        await this.BuildResponseAsync<DeviceModel>(
            async () =>
            {
                if (deviceModel == null || deviceModel.Id == null || deviceModel.Name == null || id != deviceModel.Id)
                {
                    return BadRequest();
                }

                return Ok(await deviceService.UpdateDevice(deviceModel));
            });
}