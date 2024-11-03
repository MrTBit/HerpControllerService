using HerpControllerService.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace HerpControllerService.Controllers;

public abstract class BaseController(ILogger logger) : ControllerBase
{
    public readonly ILogger Logger = logger;

    public ObjectResult StatusCodeFromException(HerpControllerException exception) => StatusCode((int)exception.StatusCode, new {status = exception.StatusCode, message = exception.Message});
    public ObjectResult StatusCodeFromException(Exception exception) => StatusCode(StatusCodes.Status500InternalServerError, new {status = StatusCodes.Status500InternalServerError, message = exception.Message});
}

internal static class BaseControllerExtensions
{
    internal static async Task<ActionResult<T>> BuildResponseAsync<T>(
        this BaseController controller,
        Func<Task<ActionResult<T>>> processRequest,
        Func<HerpControllerException, ObjectResult>? processBadRequest = null)
    {
        try
        {
            return await processRequest();
        }
        catch (HerpControllerException serviceException)
        {
            return processBadRequest != null
                ? processBadRequest(serviceException)
                : controller.StatusCodeFromException(serviceException);
        }
        catch (Exception exception)
        {
            controller.Logger.LogError(exception, "An unexpected error occurred.");

            return controller.StatusCodeFromException(exception);
        }
    }
    
    internal static async Task<ActionResult> BuildResponseAsync(
        this BaseController controller,
        Func<Task<ActionResult>> processRequest,
        Func<HerpControllerException, ObjectResult>? processBadRequest = null)
    {
        try
        {
            return await processRequest();
        }
        catch (HerpControllerException serviceException)
        {
            return processBadRequest != null
                ? processBadRequest(serviceException)
                : controller.StatusCodeFromException(serviceException);
        }
        catch (Exception exception)
        {
            controller.Logger.LogError(exception, "An unexpected error occurred.");

            return controller.StatusCodeFromException(exception);
        }
    }
}