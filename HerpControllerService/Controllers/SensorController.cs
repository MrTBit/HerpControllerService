using HerpControllerService.Models.API;
using HerpControllerService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerpControllerService.Controllers;

[Authorize]
[Route("sensors")]
public class SensorController(ILogger<SensorController> logger, SensorService sensorService) : BaseController(logger)
{
    [HttpPut]
    public async Task<ActionResult<SensorModel>> Update([FromBody]SensorModel model) => await this.BuildResponseAsync<SensorModel>(
        async () =>
        {
            if (model == null || model.Id == null || model.Name == null)
            {
                return BadRequest();
            }

            return Ok(await sensorService.UpdateSensor(model));
        });
}