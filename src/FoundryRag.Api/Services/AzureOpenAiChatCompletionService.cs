using FoundryRag.Api.Infrastructure;
using FoundryRag.Api.Models;
using FoundryRag.Api.Options;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Reflection;

namespace FoundryRag.Api.Services;

public sealed class AzureOpenAiChatCompletionService : IChatCompletionService
{
    private readonly AzureOpenAiClientFactory _clientFactory;
    private readonly RetryPolicy _retryPolicy;
    private readonly RagOptions _ragOptions;
    private readonly ILogger<AzureOpenAiChatCompletionService> _logger;

    public AzureOpenAiChatCompletionService(
        AzureOpenAiClientFactory clientFactory,
        RetryPolicy retryPolicy,
        IOptions<RagOptions> ragOptions,
        ILogger<AzureOpenAiChatCompletionService> logger)
    {
        _clientFactory = clientFactory;
        _retryPolicy = retryPolicy;
        _ragOptions = ragOptions.Value;
        _logger = logger;
    }

    public async Task<string> GenerateAnswerAsync(RagPrompt prompt, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(prompt);

        return await _retryPolicy.ExecuteAsync(async ct =>
        {
            var client = _clientFactory.GetChatClient();
            var messages = new ChatMessage[]
            {
                new SystemChatMessage(prompt.SystemMessage),
                new UserChatMessage(prompt.UserMessage)
            };

            var options = CreateChatCompletionOptions(_ragOptions, _logger);

            var completion = await client.CompleteChatAsync(messages, options, ct);
            var answer = string.Concat(completion.Value.Content.Select(part => part.Text)).Trim();

            _logger.LogInformation("Azure OpenAI chat completion succeeded with answer length {AnswerLength}", answer.Length);
            return string.IsNullOrWhiteSpace(answer)
                ? "I do not have enough information in the indexed data to answer that."
                : answer;
        }, "Azure OpenAI chat completion", cancellationToken);
    }

    internal static ChatCompletionOptions CreateChatCompletionOptions(
        RagOptions ragOptions,
        ILogger? logger = null)
    {
        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = ragOptions.MaxOutputTokens
        };

        if (!TryApplyTemperature(options, ragOptions.Temperature))
        {
            // Azure.AI.OpenAI 2.1.0 depends on OpenAI.Chat options that expose max
            // output tokens but not temperature. Future compatible SDK versions may
            // expose a writable Temperature property; when they do, this helper will
            // set it without changing the service boundary.
            logger?.LogDebug("Configured chat temperature was not applied because this SDK does not expose a writable Temperature option.");
        }

        return options;
    }

    internal static bool SupportsTemperatureOption =>
        typeof(ChatCompletionOptions).GetProperty("Temperature", BindingFlags.Instance | BindingFlags.Public)?.CanWrite == true;

    private static bool TryApplyTemperature(ChatCompletionOptions options, float temperature)
    {
        var property = typeof(ChatCompletionOptions).GetProperty("Temperature", BindingFlags.Instance | BindingFlags.Public);
        if (property?.CanWrite != true)
        {
            return false;
        }

        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        object value;

        if (targetType == typeof(float))
        {
            value = temperature;
        }
        else if (targetType == typeof(double))
        {
            value = (double)temperature;
        }
        else if (targetType == typeof(decimal))
        {
            value = (decimal)temperature;
        }
        else
        {
            return false;
        }

        property.SetValue(options, value);
        return true;
    }
}
