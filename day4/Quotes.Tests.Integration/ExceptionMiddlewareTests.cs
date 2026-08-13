using FluentAssertions;
using Microsoft.AspNetCore.Http;
using QuotesApi.Middleware;

namespace Quotes.Tests.Integration;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenNextThrows_ReturnsInternalServerErrorProblemDetails()
    {
        var middleware = new ExceptionMiddleware(_ =>
            throw new InvalidOperationException("Test exception"));

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode
            .Should().Be(StatusCodes.Status500InternalServerError);

        context.Response.ContentType
            .Should().StartWith("application/json");

        context.Response.Body.Position = 0;

        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();

        body.Should().Contain("An unexpected error occurred.");
    }
}