using System.Net;

namespace webapi_full.Exceptions;

public class UnauthorizedException : CustomException
{
    public UnauthorizedException(string message)
        : base(message, null, HttpStatusCode.Unauthorized)
    { }
}