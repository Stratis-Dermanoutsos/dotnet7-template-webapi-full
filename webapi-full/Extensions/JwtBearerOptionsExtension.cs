using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json;
using webapi_full.Entities.Response;

namespace webapi_full.Extensions;

public static class JwtBearerOptionsExtension
{
    public static void SetupJwtBearerEvents(this JwtBearerOptions options)
    {
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();

                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                string errorId = Guid.NewGuid().ToString();
                string exception = "You have to be logged in to access this resource.";
                ErrorResult errorResult = new ErrorResult
                {
                    Source = context.Request.Path,
                    Exception = exception,
                    ErrorId = errorId,
                    SupportMessage = $"Provide the Error Id: {errorId} to the support team for further analysis.",
                    StatusCode = context.Response.StatusCode
                };
                errorResult.Messages.Add(exception);

                Log.Error($"{errorResult.Exception} Request failed with Status Code {context.Response.StatusCode} and Error Id {errorId}.");

                context.Response.WriteAsync(JsonConvert.SerializeObject(errorResult));
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";

                string errorId = Guid.NewGuid().ToString();
                string exception = "You do not have the required permissions to access this resource.";
                ErrorResult errorResult = new ErrorResult
                {
                    Source = context.Request.Path,
                    Exception = exception,
                    ErrorId = errorId,
                    SupportMessage = $"Provide the Error Id: {errorId} to the support team for further analysis.",
                    StatusCode = context.Response.StatusCode
                };
                errorResult.Messages.Add(exception);

                Log.Error($"{errorResult.Exception} Request failed with Status Code {context.Response.StatusCode} and Error Id {errorId}.");

                context.Response.WriteAsync(JsonConvert.SerializeObject(errorResult));
                return Task.CompletedTask;
            }
        };
    }
}