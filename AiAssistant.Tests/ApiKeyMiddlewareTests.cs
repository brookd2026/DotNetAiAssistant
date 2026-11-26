using AiAssistant.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace AiAssistant.Tests;

[TestFixture]
public class ApiKeyMiddlewareTests
{
    private static IConfiguration BuildConfig(string? key) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AI:AppApiKey"] = key })
            .Build();

    [Test]
    public async Task InvokeAsync_MissingHeader_Returns401()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var nextCalled = false;
        var middleware = new ApiKeyMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, BuildConfig("secret-123"));

        await middleware.InvokeAsync(context);

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
        Assert.That(nextCalled, Is.False);
    }

    [Test]
    public async Task InvokeAsync_WrongKey_Returns401()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Api-Key"] = "wrong-key";
        context.Response.Body = new MemoryStream();
        var middleware = new ApiKeyMiddleware(_ => Task.CompletedTask, BuildConfig("secret-123"));

        await middleware.InvokeAsync(context);

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
    }

    [Test]
    public async Task InvokeAsync_ValidKey_CallsNext()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Api-Key"] = "secret-123";
        var nextCalled = false;
        var middleware = new ApiKeyMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, BuildConfig("secret-123"));

        await middleware.InvokeAsync(context);

        Assert.That(nextCalled, Is.True);
    }
}
