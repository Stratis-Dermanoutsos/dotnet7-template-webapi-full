using System.Net;

namespace webapi_full.Exceptions;

public class InternalServerException : CustomException
{
    public InternalServerException(string message)
        : base(message, null, HttpStatusCode.InternalServerError)
    { }
}