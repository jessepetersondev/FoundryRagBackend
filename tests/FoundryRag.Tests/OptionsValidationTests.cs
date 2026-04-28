using FluentAssertions;
using FoundryRag.Api.Options;

namespace FoundryRag.Tests;

public sealed class OptionsValidationTests
{
    [Fact]
    public void AzureOpenAiOptions_MissingRequiredConfiguration_IsInvalid()
    {
        AzureOpenAiOptions.IsValid(new AzureOpenAiOptions()).Should().BeFalse();
    }

    [Fact]
    public void AzureSearchOptions_MissingRequiredConfiguration_IsInvalid()
    {
        AzureSearchOptions.IsValid(new AzureSearchOptions()).Should().BeFalse();
    }
}
