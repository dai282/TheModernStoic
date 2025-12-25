using System.ClientModel;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.AI;
using OpenAI;
using TheModernStoic.Domain.Interfaces;
using TheModernStoic.Infrastructure.Repositories;
using TheModernStoic.Infrastructure.Services;

// var seederPath = Path.Combine(AppContext.BaseDirectory, "SeederFiles");
// var modelPath = Path.Combine(seederPath, "model.onnx");
// var vocabPath = Path.Combine(seederPath, "vocab.txt");
var resourcesPath = Path.Combine(AppContext.BaseDirectory, "Resources");
var modelPath = Path.Combine(resourcesPath, "model.onnx");
var vocabPath = Path.Combine(resourcesPath, "vocab.txt");

// Verify existence (Good practice for debugging deployment issues)
if (!File.Exists(modelPath))
{
    throw new FileNotFoundException($"ONNX Model not found at {modelPath}. Ensure build properties copy it.");
}

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<CosmosClient>(sp =>
{
    //var connString = builder.Configuration["CosmosDb:ConnectionString"];
    var connString = builder.Configuration.GetConnectionString("CosmosDb");
    if (string.IsNullOrEmpty(connString))
        throw new InvalidOperationException("CosmosDb:ConnectionString is missing in User Secrets.");

    return new CosmosClient(connString, new CosmosClientOptions
    {
        // 1. Switch to Gateway mode to bypass Firewall/Port issues
        ConnectionMode = ConnectionMode.Gateway,
        // 2. Enable Bulk Execution for performance
        AllowBulkExecution = true, // Speeds up seeding significantly
        // 3. Increase Timeout (Default is 60s, bump to 2 mins for cross-region heavy loads)
        RequestTimeout = TimeSpan.FromMinutes(2),

        // --- NEW: Force camelCase Serialization ---
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    });
});

// Register the ONNX Generator using the Extension Method
builder.Services.AddBertOnnxEmbeddingGenerator(modelPath, vocabPath);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add CORS for React
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173",
         "https://blue-ocean-065454300.6.azurestaticapps.net") // The Vite default port
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var hfApiKey = builder.Configuration["AI:HuggingFaceApiKey"];
var hfModelId = builder.Configuration["HuggingFace:ModelId"];
//var hfModelId = "meta-llama/Llama-3.1-8B-Instruct";

if (string.IsNullOrEmpty(hfApiKey))
    throw new InvalidOperationException("HugginFaceApiKey is missing in User Secrets.");

builder.Services.AddChatClient(new OpenAIClient(
    new ApiKeyCredential(hfApiKey),
    new OpenAIClientOptions
    {
        Endpoint = new Uri($"https://router.huggingface.co/v1/")
    }).GetChatClient(model: hfModelId).AsIChatClient());

// Register the Search Service
builder.Services.AddScoped<IVectorSearchService, CosmosVectorSearchService>();
builder.Services.AddScoped<IJournalService, JournalService>();

// Register the new Repository
builder.Services.AddSingleton<IJournalRepository>(sp =>
{
    var client = sp.GetRequiredService<CosmosClient>();
    return new CosmosJournalRepository(client, "ModernStoicDb", "Entries");
});

//Add controllers
builder.Services.AddControllers();

var app = builder.Build();

app.MapDefaultEndpoints();

// Enable Swagger Middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // This creates the UI at /swagger/
}

app.MapGet("/", () => "The Modern Stoic API is running!");

app.UseCors("ReactPolicy");

app.MapControllers();

app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
{
    var endpoints = endpointSources.SelectMany(es => es.Endpoints);
    return endpoints.Select(e =>
    {
        var routeEndpoint = e as RouteEndpoint;
        return new
        {
            Route = routeEndpoint?.RoutePattern.RawText,
            DisplayName = e.DisplayName
        };
    });
});

app.Run();
