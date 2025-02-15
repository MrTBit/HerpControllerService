using HerpControllerService.Models.API;
using HerpControllerService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerpControllerService.Controllers;

[Authorize]
[Route("sensors")]
public class SensorController(ILogger<SensorController> logger, SensorService sensorService) : BaseController(logger)
{
    [HttpPut("{id:long}")]
    public async Task<ActionResult<SensorModel>> Update(long id, [FromBody]SensorModel model) => await this.BuildResponseAsync<SensorModel>(
        async () =>
        {
            if (model == null || model.Id == null || model.Name == null || model.Id != id)
            {
                return BadRequest();
            }

            return Ok(await sensorService.UpdateSensor(model));
        });
}