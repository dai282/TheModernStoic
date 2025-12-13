using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheModernStoic.Domain.Entities;
using TheModernStoic.Domain.Interfaces;
using TheModernStoic.Infrastructure.Data;
using TheModernStoic.Infrastructure.FileProcessing;

// 1. Setup Paths
var currentDir = Directory.GetCurrentDirectory();
var resourcesDir = Path.Combine(currentDir, "Resources");
var modelPath = Path.Combine(resourcesDir, "model.onnx");
var vocabPath = Path.Combine(resourcesDir, "vocab.txt");
var textPath = Path.Combine(resourcesDir, "Meditations.txt");

// Validation
if (!File.Exists(modelPath) || !File.Exists(vocabPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: Models not found at {resourcesDir}");
    Console.ResetColor();
    return;
}

// 2. Configure Dependency Injection (The "Host")
var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

// Register the ONNX Generator using the Extension Method
builder.Services.AddBertOnnxEmbeddingGenerator(modelPath, vocabPath);

// Register Services
// Register a "Runner" service that contains our logic (keeps Program.cs clean)
//builder.Services.AddSingleton<SeederRunner>();

// Cleaner and chunker
builder.Services.AddSingleton<GutenbergCleaner>();
builder.Services.AddSingleton<IContentProcessor, StoicTextChunker>();

// Cosmos Registration
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
builder.Services.AddSingleton<IKnowledgeRepository, CosmosKnowledgeRepository>();


using var host = builder.Build();


// Execution scope
using var scope = host.Services.CreateScope();
    //txt processor
var processor = scope.ServiceProvider.GetRequiredService<IContentProcessor>();
    //embedding generator
var generator = scope.ServiceProvider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
    // container knowledge repository
var repository = scope.ServiceProvider.GetRequiredService<IKnowledgeRepository>();


// 4. Initialize DB
Console.WriteLine("Initializing Cosmos DB 'Knowledge' container...");
await repository.InitializeAsync(); // This creates the 384-dim container


Console.WriteLine("Reading Meditations.txt...");
var rawText = await File.ReadAllTextAsync(textPath);

Console.WriteLine("Cleaning and chunking...");
//chunks is IEnumerable<string>
var allChunks = processor.Process(rawText).ToArray();
Console.WriteLine($"Generated {allChunks.Length} chunks.");

// Reduce batch size to prevent hitting RU limits
int batchSize = 5;
int totalProcessed = 0;

foreach (var batch in allChunks.Chunk(batchSize))
{
    //Generate embeddings
    var embeddings = await generator.GenerateAsync(batch);

    var chunksToSave = new List<KnowledgeChunk>();
    for (int i = 0; i < batch.Length; i++)
    {
        chunksToSave.Add(new KnowledgeChunk
        {
            Id = Guid.NewGuid(),
            Content = batch[i],
            Source = "Meditations - Marcus Aurelius",
            Vector = embeddings[i].Vector.ToArray()
        });
    }

    // 2. Save to Azure
    try
    {
        await repository.UpsertBatchAsync(chunksToSave);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nError saving batch: {ex.Message}");
        // Optional: break or continue depending on how strict you want to be
    }

    totalProcessed += batch.Length;
    Console.Write($"\rSeeded {totalProcessed}/{allChunks.Length}...");

    // 3. Throttle yourself manually to respect Free Tier limits
    // Wait 200ms between batches
    await Task.Delay(200);
}

Console.WriteLine("\nDone! Knowledge Base is live.");

/*
// 3. Embedding Loop
var knowledgeBase = new List<KnowledgeChunk>();

// NOTE: In production, batch this. Don't do 1 by 1.
// Since we are running local ONNX, we can batch reasonably well (e.g., 10 at a time).
int batchSize = 10;

for (int i = 0; i < chunks.Count; i+= batchSize)
{
    //Skip(), Take() and ToList() is LINQ
    //each iteration, you start at i, grab the next 10 items and turn them into a list 
    var batch = chunks.Skip(i).Take(batchSize).ToList();

    // Generate Embeddings for the batch
    var embeddings = await generator.GenerateAsync(batch);

    for (int j = 0; j < batch.Count; j++)
    {
        knowledgeBase.Add(new KnowledgeChunk
        {
            Id = Guid.NewGuid(),
            Content = batch[j],
            Source = "Meditations - Marcus Aurelius",
            Vector = embeddings[j].Vector.ToArray() // Convert ReadOnlyMemory to float[]
        });
    }

    Console.Write($"\rProcessed {knowledgeBase.Count}/{chunks.Count} chunks...");
}


Console.WriteLine("\nDone! Embeddings stored in memory.");
Console.WriteLine($"Sample Vector[0]: {knowledgeBase.First().Vector[0]}");


//Alternative: using Chunk() for larger datasetes
//var batches = chunks.Chunk(batchSize);

//foreach (var batch in batches)
//{
//    var embeddings = await generator.GenerateAsync(batch);

//    for (int j = 0; j < batch.Length; j++)
//    {
//        knowledgeBase.Add(new KnowledgeChunk
//        {
//            Id = Guid.NewGuid(),
//            Content = batch[j],
//            Source = "Meditations - Marcus Aurelius",
//            Vector = embeddings[j].Vector.ToArray() // Convert ReadOnlyMemory to float[]
//        });
//    }

//    Console.Write($"\rProcessed {knowledgeBase.Count}/{chunks.Count} chunks...");
//}

*/