namespace FoundryRag.Api.Contracts;

public sealed record ErrorResponse(ErrorDetail Error)
{
    public static ErrorResponse Create(string code, string message) => new(new ErrorDetail(code, message));
}

public sealed record ErrorDetail(string Code, string Message);
