using System.Net;

namespace HerpControllerService.Exceptions;

public class HerpControllerException(HttpStatusCode statusCode, string? message = null) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}