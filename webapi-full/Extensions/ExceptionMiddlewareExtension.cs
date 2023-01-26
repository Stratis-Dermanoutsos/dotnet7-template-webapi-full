using webapi_full.Middleware;

namespace webapi_full.Extensions;

/// <summary>
/// <paramref name="builder" />
/// <param name="builder">: The <paramref name="IApplicationBuilder" /> to use.</param>
/// <br />
/// <returns>Returns the <paramref name="IApplicationBuilder" /> with the <paramref name="ExceptionMiddleware" /> added.</returns>
/// </summary>
public static class ExceptionMiddlewareExtension
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}