namespace FoundryRag.Api.Infrastructure;

public sealed class RetryPolicy
{
    private static readonly TimeSpan[] Delays =
    [
        TimeSpan.FromMilliseconds(200),
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromSeconds(1)
    ];

    private readonly ILogger<RetryPolicy> _logger;

    public RetryPolicy(ILogger<RetryPolicy> logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception ex) when (attempt < Delays.Length && IsTransient(ex))
            {
                _logger.LogWarning(ex, "Transient failure during {OperationName}; retry attempt {Attempt}", operationName, attempt + 1);
                await Task.Delay(Delays[attempt], cancellationToken);
            }
        }
    }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        string operationName,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            async ct =>
            {
                await operation(ct);
                return true;
            },
            operationName,
            cancellationToken);
    }

    private static bool IsTransient(Exception ex) =>
        ex is Azure.RequestFailedException { Status: 408 or 429 or >= 500 } ||
        ex is System.ClientModel.ClientResultException { Status: 408 or 429 or >= 500 } ||
        ex is TimeoutException ||
        ex is HttpRequestException;
}
