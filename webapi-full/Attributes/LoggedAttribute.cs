

using Microsoft.AspNetCore.Mvc.Filters;

namespace webapi_full.Attributes;

public class Logged : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        string method = context.HttpContext.Request.Method;
        string path = context.HttpContext.Request.Path;

        Log.Information($"{method}: {path}");

        base.OnActionExecuting(context);
    }
}