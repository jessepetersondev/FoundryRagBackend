using System.Text.Json;
using FluentAssertions;
using FoundryRag.Api.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FoundryRag.Tests;

public sealed class ErrorHandlingTests
{
    [Fact]
    public async Task InvokeAsync_MapsValidationExceptionToErrorResponse()
    {
        var middleware = new ErrorHandlingMiddleware(
            _ => throw new RequestValidationException("Question is required."),
            NullLogger<ErrorHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var body = await ReadBodyAsync(context);
        body.RootElement.GetProperty("error").GetProperty("code").GetString().Should().Be("InvalidRequest");
        body.RootElement.GetProperty("error").GetProperty("message").GetString().Should().Be("Question is required.");
    }

    [Fact]
    public async Task InvokeAsync_MapsUnexpectedExceptionToSafeError()
    {
        var middleware = new ErrorHandlingMiddleware(
            _ => throw new InvalidOperationException("sensitive internal detail"),
            NullLogger<ErrorHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var body = await ReadBodyAsync(context);
        body.RootElement.GetProperty("error").GetProperty("code").GetString().Should().Be("ServerError");
        body.RootElement.GetProperty("error").GetProperty("message").GetString().Should().NotContain("sensitive");
    }

    [Theory]
    [InlineData("AzureOpenAi")]
    [InlineData("AzureSearch")]
    public async Task InvokeAsync_MapsMissingRequiredConfigurationToSafeError(string optionsName)
    {
        var middleware = new ErrorHandlingMiddleware(
            _ => throw new OptionsValidationException(optionsName, typeof(object), ["missing setting"]),
            NullLogger<ErrorHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var body = await ReadBodyAsync(context);
        body.RootElement.GetProperty("error").GetProperty("code").GetString().Should().Be("ConfigurationMissing");
        body.RootElement.GetProperty("error").GetProperty("message").GetString().Should().Contain("Required local configuration");
        body.RootElement.GetProperty("error").GetProperty("message").GetString().Should().NotContain("missing setting");
    }

    [Fact]
    public async Task InvokeAsync_MapsSeedDataExceptionWithoutExposingLocalPath()
    {
        var middleware = new ErrorHandlingMiddleware(
            _ => throw new SeedDataException("Seed data could not be loaded."),
            NullLogger<ErrorHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var body = await ReadBodyAsync(context);
        var message = body.RootElement.GetProperty("error").GetProperty("message").GetString();
        message.Should().Be("Seed data could not be loaded.");
        message.Should().NotContain("/");
        message.Should().NotContain("\\");
    }

    private static async Task<JsonDocument> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }
}
