namespace FoundryRag.Api.Infrastructure;

public class ApiException : Exception
{
    public ApiException(string code, string userMessage, int statusCode, Exception? innerException = null)
        : base(userMessage, innerException)
    {
        Code = code;
        UserMessage = userMessage;
        StatusCode = statusCode;
    }

    public string Code { get; }
    public string UserMessage { get; }
    public int StatusCode { get; }
}

public sealed class RequestValidationException : ApiException
{
    public RequestValidationException(string message)
        : base("InvalidRequest", message, StatusCodes.Status400BadRequest)
    {
    }
}

public sealed class ConfigurationMissingException : ApiException
{
    public ConfigurationMissingException(string message)
        : base("ConfigurationMissing", message, StatusCodes.Status500InternalServerError)
    {
    }
}

public sealed class ExternalServiceException : ApiException
{
    public ExternalServiceException(string message, Exception? innerException = null)
        : base("ExternalServiceFailure", message, StatusCodes.Status502BadGateway, innerException)
    {
    }
}

public sealed class SeedDataException : ApiException
{
    public SeedDataException(string message, Exception? innerException = null)
        : base("SeedDataFailure", message, StatusCodes.Status500InternalServerError, innerException)
    {
    }
}
