using Microsoft.Azure.Cosmos;
using TheModernStoic.Domain.Interfaces;
using TheModernStoic.Infrastructure.Services;

var seederPath = Path.Combine(AppContext.BaseDirectory, "SeederFiles");
var modelPath = Path.Combine(seederPath, "model.onnx");
var vocabPath = Path.Combine(seederPath, "vocab.txt");

// Verify existence (Good practice for debugging deployment issues)
if (!File.Exists(modelPath))
{
    throw new FileNotFoundException($"ONNX Model not found at {modelPath}. Ensure build properties copy it.");
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var connString = builder.Configuration["CosmosDb:ConnectionString"];
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

// Register the Search Service
builder.Services.AddScoped<IVectorSearchService, CosmosVectorSearchService>();

//Add controllers
builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/", () => "The Modern Stoic API is running!");

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
