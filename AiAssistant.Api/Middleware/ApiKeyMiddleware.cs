using System.Security.Cryptography;
using System.Text;

namespace AiAssistant.Api.Middleware;

public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private const string HeaderName = "X-Api-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var expectedApiKey = configuration["AI:AppApiKey"];
        if (string.IsNullOrWhiteSpace(expectedApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Server API key is not configured." });
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Missing X-Api-Key header." });
            return;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expectedApiKey);
        var actualBytes = Encoding.UTF8.GetBytes(extractedApiKey.ToString());

        if (!CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key." });
            return;
        }

        await next(context);
    }
}
