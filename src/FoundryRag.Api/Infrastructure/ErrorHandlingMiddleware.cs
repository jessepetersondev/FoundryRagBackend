using Azure;
using FoundryRag.Api.Contracts;
using Microsoft.Extensions.Options;
using System.ClientModel;
using System.Text.Json;

namespace FoundryRag.Api.Infrastructure;

public sealed class ErrorHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            _logger.LogWarning(ex, "Handled API error {Code}", ex.Code);
            await WriteErrorAsync(context, ex.StatusCode, ex.Code, ex.UserMessage);
        }
        catch (OptionsValidationException ex)
        {
            _logger.LogError(ex, "Configuration validation failed");
            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "ConfigurationMissing",
                "Required local configuration is missing or invalid. Check AzureOpenAi, AzureSearch, and Rag settings.");
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure service request failed with status {Status}", ex.Status);
            await WriteErrorAsync(
                context,
                StatusCodes.Status502BadGateway,
                "ExternalServiceFailure",
                "An Azure service request failed. Check server logs and local configuration.");
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(ex, "Azure OpenAI client request failed with status {Status}", ex.Status);
            await WriteErrorAsync(
                context,
                StatusCodes.Status502BadGateway,
                "ExternalServiceFailure",
                "An Azure OpenAI request failed. Check server logs and local configuration.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled server error");
            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "ServerError",
                "An unexpected server error occurred.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string code, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var response = ErrorResponse.Create(code, message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
