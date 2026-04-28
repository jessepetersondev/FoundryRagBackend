using FluentAssertions;
using FoundryRag.Api.Options;
using FoundryRag.Api.Services;
using OpenAI.Chat;
using System.Reflection;

namespace FoundryRag.Tests;

public sealed class AzureOpenAiChatCompletionServiceTests
{
    [Fact]
    public void CreateChatCompletionOptions_AppliesConfiguredMaxOutputTokens()
    {
        var options = AzureOpenAiChatCompletionService.CreateChatCompletionOptions(
            new RagOptions { MaxOutputTokens = 321, Temperature = 0.2f });

        options.MaxOutputTokenCount.Should().Be(321);
    }

    [Fact]
    public void CreateChatCompletionOptions_AppliesTemperatureWhenSdkExposesWritableOption()
    {
        var options = AzureOpenAiChatCompletionService.CreateChatCompletionOptions(
            new RagOptions { MaxOutputTokens = 321, Temperature = 0.2f });
        var temperatureProperty = typeof(ChatCompletionOptions).GetProperty(
            "Temperature",
            BindingFlags.Instance | BindingFlags.Public);

        if (temperatureProperty?.CanWrite == true)
        {
            temperatureProperty.GetValue(options).Should().NotBeNull();
        }
        else
        {
            AzureOpenAiChatCompletionService.SupportsTemperatureOption.Should().BeFalse();
        }
    }
}
