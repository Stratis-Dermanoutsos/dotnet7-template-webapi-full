using System.Net;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;
using webapi_full.Entities.Response;
using webapi_full.Exceptions;

namespace webapi_full.Middleware;

public class ExceptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try {
            await next(context);
        } catch (Exception exception) {
            //* Create a new error id using a Guid.
            string errorId = Guid.NewGuid().ToString();

            //* Push the error id and exception stack trace onto our logger.
            LogContext.PushProperty("ErrorId", errorId);
            LogContext.PushProperty("StackTrace", exception.StackTrace);

            //* Create a new error result using the exception's.
            ErrorResult errorResult = new ErrorResult
            {
                Source = exception.TargetSite?.DeclaringType?.FullName,
                Exception = exception.Message.Trim(),
                ErrorId = errorId,
                SupportMessage = $"Provide the Error Id: {errorId} to the support team for further analysis."
            };
            errorResult.Messages.Add(exception.Message);

            //* If the exception is not a custom exception, set the exception variable to the InnerException to be switched on in a moment.
            if (exception is not CustomException && exception.InnerException != null)
                while (exception.InnerException != null)
                    exception = exception.InnerException;

            //* Switch on the exception to set the status code and error messages.
            switch (exception)
            {
                case CustomException e:
                    errorResult.StatusCode = (int)e.StatusCode;
                    if (e.ErrorMessages is not null)
                        errorResult.Messages = e.ErrorMessages;

                    break;

                case KeyNotFoundException:
                    errorResult.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                default:
                    errorResult.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            Log.Error($"{errorResult.Exception} Request failed with Status Code {context.Response.StatusCode} and Error Id {errorId}.");

            //* Write the error result to the response.
            HttpResponse response = context.Response;
            if (!response.HasStarted) {
                response.ContentType = "application/json";
                response.StatusCode = errorResult.StatusCode;
                await response.WriteAsync(JsonConvert.SerializeObject(errorResult));
            }
            else
                Log.Warning("Can't write error response. Response has already started.");
        }
    }
}