

using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace webapi_full.Attributes;

public class Logged : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        string method = context.HttpContext.Request.Method;
        string version = context.RouteData.Values["version"]?.ToString() ?? "Unknown";
        string controller = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
        string action = context.RouteData.Values["action"]?.ToString() ?? "Unknown";

        Log.Information($"{method}: api/v{version}/{controller}/{action}");

        base.OnActionExecuting(context);
    }
}