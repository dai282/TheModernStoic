using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheModernStoic.Domain.Entities;
using TheModernStoic.Domain.Interfaces;
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

// Register the ONNX Generator using the Extension Method
builder.Services.AddBertOnnxEmbeddingGenerator(modelPath, vocabPath);

// Register Services
// Register a "Runner" service that contains our logic (keeps Program.cs clean)
//builder.Services.AddSingleton<SeederRunner>();

// Cleaner and chunker
builder.Services.AddSingleton<GutenbergCleaner>();
builder.Services.AddSingleton<IContentProcessor, StoicTextChunker>();

using var host = builder.Build();


// Execution scope
using var scope = host.Services.CreateScope();
    //txt processor
var processor = scope.ServiceProvider.GetRequiredService<IContentProcessor>();
    //embedding generator
var generator = scope.ServiceProvider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

Console.WriteLine("Reading Meditations.txt...");

var rawText = await File.ReadAllTextAsync(textPath);

Console.WriteLine("Cleaning and chunking...");

//chunks is IEnumerable<string>
var chunks = processor.Process(rawText).ToList();

Console.WriteLine($"Generated {chunks.Count} chunks. Beginning Embedding Generation...");

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