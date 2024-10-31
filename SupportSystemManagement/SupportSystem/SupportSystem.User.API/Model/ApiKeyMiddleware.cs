namespace SupportSystem.User.API.Model;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-Api-Key";

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        StringValues extractedApiKey = StringValues.Empty;
        bool isKeyAvaiable = context.Request.Headers.TryGetValue(ApiKeyHeaderName, out extractedApiKey);

        if (context.Request.Path.StartsWithSegments("/api/user/validate") )
        {
            if (!isKeyAvaiable)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key not provided.");
                return;
            }

            var configuredApiKey = "1234567890ABCDEF"; // Store securely in configuration

            if (!configuredApiKey.Equals(extractedApiKey.ToString()))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized client.");
                return;
            }
        }
        await _next(context);
    }
}
