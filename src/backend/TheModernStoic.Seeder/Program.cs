using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel.Connectors.Onnx; // For AddBertOnnx... extension
using TheModernStoic.Domain.Entities;

// 1. Setup Paths
var currentDir = Directory.GetCurrentDirectory();
var resourcesDir = Path.Combine(currentDir, "Resources");
var modelPath = Path.Combine(resourcesDir, "model.onnx");
var vocabPath = Path.Combine(resourcesDir, "vocab.txt");

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

// Register a "Runner" service that contains our logic (keeps Program.cs clean)
builder.Services.AddSingleton<SeederRunner>();

using var host = builder.Build();

// 3. Execute
var runner = host.Services.GetRequiredService<SeederRunner>();
await runner.RunAsync();


// --- Internal Runner Class (The "Brain" of the console app) ---
class SeederRunner
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;

    public SeederRunner(IEmbeddingGenerator<string, Embedding<float>> generator)
    {
        _generator = generator;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("Modern Stoic Seeder Initialized...");

        var text = "The obstacle is the way.";
        Console.WriteLine($"Generating vector for: '{text}'");

        // Generate
        var result = await _generator.GenerateAsync([text]);
        var embedding = result[0];

        // Output Verification
        Console.WriteLine($"Success! Vector Length: {embedding.Vector.Length}");

        // Peek at data
        var values = embedding.Vector.ToArray();
        Console.WriteLine($"First 3 values: [{values[0]:F4}, {values[1]:F4}, {values[2]:F4}]");

        // Architecture check: Map to Domain Entity
        var chunk = new KnowledgeChunk
        {
            Content = text,
            // Note: Cosmos DB Vector Search usually expects float[]
            Vector = values
        };

        Console.WriteLine("Mapped to Domain Entity successfully.");
    }
}