using System.ClientModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.AI;
using OpenAI;
using TheModernStoic.API.Services;
using TheModernStoic.Application.Interfaces;
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

//AUTHENTICATION SETUP
//Configuration
var authDomain = builder.Configuration["Auth0:Domain"];
var authAudience = builder.Configuration["Auth0:Audience"];

//Add Authentication Services
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = authDomain;
    options.Audience = authAudience;
});

//Register CurrentUserService
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.AddServiceDefaults();

// ================== DATABASE & SEARCH SERVICES (Conditional) ==================
var useInMemoryDb = builder.Configuration.GetValue<bool>("UseInMemoryDb");

if (useInMemoryDb)
{
    // For CI/E2E tests, use lightweight in-memory stores
    builder.Services.AddSingleton<IJournalRepository, InMemoryJournalRepository>();
    builder.Services.AddScoped<IVectorSearchService, InMemoryVectorSearchService>();
}
else
{
    // For local dev and production, use the real Cosmos DB services
    builder.Services.AddSingleton<CosmosClient>(sp =>
    {
        var connString = builder.Configuration.GetConnectionString("CosmosDb");
        if (string.IsNullOrEmpty(connString))
            throw new InvalidOperationException("CosmosDb:ConnectionString is missing in User Secrets.");

        return new CosmosClient(connString, new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Gateway,
            AllowBulkExecution = true,
            RequestTimeout = TimeSpan.FromMinutes(2),
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        });
    });
    builder.Services.AddScoped<IVectorSearchService, CosmosVectorSearchService>();
    builder.Services.AddSingleton<IJournalRepository>(sp =>
    {
        var client = sp.GetRequiredService<CosmosClient>();
        return new CosmosJournalRepository(client, "ModernStoicDb", "Entries");
    });
}
// ==============================================================================

// Register the ONNX Generator using the Extension Method
builder.Services.AddBertOnnxEmbeddingGenerator(modelPath, vocabPath);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add CORS for React
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            // 1. Allow Localhost
            if (origin == "http://localhost:5173") return true;

            // 2. Allow Production (Exact Match)
            if (origin == "https://blue-ocean-065454300.6.azurestaticapps.net") return true;

            // 3. Allow Preview Environments (Dynamic Pattern)
            // Checks if it starts with your app name and ends with the azure domain
            // This covers -1, -2, -5, -6, etc.
            if (origin.StartsWith("https://blue-ocean-065454300-") &&
                origin.EndsWith(".azurestaticapps.net"))
            {
                return true;
            }

            return false;
        })
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

// Register the Journal Service (which depends on the repositories)
builder.Services.AddScoped<IJournalService, JournalService>();

// Register the new Repository - This is now handled in the conditional block above
// builder.Services.AddSingleton<IJournalRepository>(sp =>
// {
//     var client = sp.GetRequiredService<CosmosClient>();
//     return new CosmosJournalRepository(client, "ModernStoicDb", "Entries");
// });
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

//Middleware setup
app.UseCors("ReactPolicy");
app.UseAuthentication();
app.UseAuthorization();

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
