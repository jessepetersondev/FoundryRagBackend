using FoundryRag.Api.Services;

namespace FoundryRag.Tests;

internal sealed class SeedWorkspace : IDisposable
{
    private SeedWorkspace(string root)
    {
        Root = root;
        Environment = TestWebHostEnvironment.Create("Development", root);
    }

    public string Root { get; }
    public TestWebHostEnvironment Environment { get; }

    public static SeedWorkspace Create(string json)
    {
        var workspace = CreateWithoutFile();
        var dataDir = Path.Combine(workspace.Root, "Data");
        Directory.CreateDirectory(dataDir);
        File.WriteAllText(Path.Combine(dataDir, "seed-markets.json"), json);
        return workspace;
    }

    public static SeedWorkspace CreateWithoutFile()
    {
        var root = Path.Combine(Path.GetTempPath(), "foundry-rag-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return new SeedWorkspace(root);
    }

    public void Dispose()
    {
        if (Directory.Exists(Root))
        {
            Directory.Delete(Root, recursive: true);
        }
    }
}
