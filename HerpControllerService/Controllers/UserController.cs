using HerpControllerService.Models;
using HerpControllerService.Models.API;
using HerpControllerService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerpControllerService.Controllers;

[ApiController]
[Route("user")]
[Authorize]
public class UserController(ILogger<UserController> logger, AuthenticationService authenticationService) : BaseController(logger)
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<TokenModel>> Login() => await this.BuildResponseAsync<TokenModel>(
        async () =>
        {
            string? refreshToken = Request.Headers["RefreshToken"];

            TokenModel tokenModel;
                
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                tokenModel = await authenticationService.Login(Request.Headers.Authorization);
            }
            else
            {
                tokenModel = await authenticationService.RefreshLogin(refreshToken);
            }

            return Ok(tokenModel);
        });
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model) => await this.BuildResponseAsync(
        async () =>
        {
            if (model?.Username == null || model.Password == null)
            {
                return BadRequest();
            }
            
            await authenticationService.Register(model);
            return Ok();
        });
}