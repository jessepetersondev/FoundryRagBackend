using FoundryRag.Api.Infrastructure;
using FoundryRag.Api.Options;
using FoundryRag.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<AzureOpenAiOptions>()
    .Bind(builder.Configuration.GetSection(AzureOpenAiOptions.SectionName))
    .Validate(AzureOpenAiOptions.IsValid, "Azure OpenAI endpoint, API key, chat deployment, and embedding deployment are required.")
    .ValidateOnStart();

builder.Services.AddOptions<AzureSearchOptions>()
    .Bind(builder.Configuration.GetSection(AzureSearchOptions.SectionName))
    .Validate(AzureSearchOptions.IsValid, "Azure AI Search endpoint, API key, and index name are required.")
    .ValidateOnStart();

builder.Services.AddOptions<RagOptions>()
    .Bind(builder.Configuration.GetSection(RagOptions.SectionName))
    .Validate(RagOptions.IsValid, "RAG options are invalid. Check top-k, thresholds, token limits, question length, context length, and embedding dimensions.")
    .ValidateOnStart();

builder.Services.AddSingleton<AzureOpenAiClientFactory>();
builder.Services.AddSingleton<AzureSearchClientFactory>();
builder.Services.AddSingleton<RetryPolicy>();

builder.Services.AddScoped<IEmbeddingService, AzureOpenAiEmbeddingService>();
builder.Services.AddScoped<IChatCompletionService, AzureOpenAiChatCompletionService>();
builder.Services.AddScoped<IVectorSearchService, AzureAiSearchVectorService>();
builder.Services.AddScoped<IPromptBuilder, GroundedPromptBuilder>();
builder.Services.AddScoped<IAnswerGroundingValidator, AnswerGroundingValidator>();
builder.Services.AddScoped<IRagService, RagService>();
builder.Services.AddScoped<ISeedDataReader, SeedDataReader>();
builder.Services.AddScoped<IDocumentIngestionService, DocumentIngestionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();

app.Run();

public partial class Program;
