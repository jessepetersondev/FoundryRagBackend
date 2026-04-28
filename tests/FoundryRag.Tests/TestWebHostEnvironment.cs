using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace FoundryRag.Tests;

internal sealed class TestWebHostEnvironment : IWebHostEnvironment
{
    private TestWebHostEnvironment(string environmentName, string contentRootPath)
    {
        EnvironmentName = environmentName;
        ContentRootPath = contentRootPath;
        ContentRootFileProvider = new PhysicalFileProvider(contentRootPath);
        WebRootPath = contentRootPath;
        WebRootFileProvider = ContentRootFileProvider;
    }

    public string ApplicationName { get; set; } = "FoundryRag.Tests";
    public IFileProvider WebRootFileProvider { get; set; }
    public string WebRootPath { get; set; }
    public string EnvironmentName { get; set; }
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; }

    public static TestWebHostEnvironment Create(string environmentName, string? contentRootPath = null)
    {
        return new TestWebHostEnvironment(environmentName, contentRootPath ?? Directory.GetCurrentDirectory());
    }
}
