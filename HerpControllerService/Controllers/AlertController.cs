using HerpControllerService.Models.API;
using HerpControllerService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerpControllerService.Controllers;

[Authorize]
[Route("alerts")]
public class AlertController(ILogger<AlertController> logger, AlertService alertService, TelegramService telegramService) : BaseController(logger)
{
    [HttpGet("active")]
    public async Task<ActionResult<List<AlertModel>>> GetActiveAlerts() =>
        await this.BuildResponseAsync<List<AlertModel>>(
            async () => Ok((await alertService.GetActiveAlerts()).Select(a => (AlertModel)a).ToList()));
    
    [HttpGet("{alertId:long}")]
    public async Task<ActionResult<AlertModel>> GetAlertById(long alertId) =>
        await this.BuildResponseAsync<AlertModel>(
            async () => Ok(await alertService.GetAlertById(alertId)));

    [HttpPut]
    public async Task<ActionResult<AlertModel>> UpdateAlert([FromBody] AlertModel model) =>
        await this.BuildResponseAsync<AlertModel>(
            async () =>
            {
                if (model.Id == null || model.Id < 0 || model.Status == null)
                {
                    return BadRequest();
                }

                return Ok(await alertService.Update(model));
            });

    [HttpPost("test")]
    public async Task SendTestAlert([FromQuery] string message) =>
        await this.BuildResponseAsync(
            async () =>
            {
                await telegramService.SendAlertAsync(-1, message);
                return Ok();
            });
}