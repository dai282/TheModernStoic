//combines Semantic Kernel (for Embeddings) and Cosmos SDK (for Querying).

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.AI;
using TheModernStoic.Domain.Interfaces;
using TheModernStoic.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;

namespace TheModernStoic.Infrastructure.Services;

public class CosmosVectorSearchService : IVectorSearchService
{
    private readonly Container _container;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

    public CosmosVectorSearchService(
        CosmosClient cosmosClient, 
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, 
        IConfiguration configuration)
    {
        // Ideally, fetch DB/Container names from config/options pattern
        var databaseName = configuration["CosmosDb:DatabaseName"] ?? "ModernStoicDb";
        var containerName = "Knowledge";
        _container = cosmosClient.GetContainer(databaseName, containerName);
        _embeddingGenerator = embeddingGenerator;
    }

    public async Task<IEnumerable<SearchResult>> SearchAsync(string query, int limit = 3)
    {
        // 1. Convert User Input -> Vector (using Local ONNX)
        // GenerateEmbeddingAsync returns a ReadOnlyMemory<float>
        var embedding = await _embeddingGenerator.GenerateAsync(query);

        // Convert to array for Cosmos SDK
        float[] vector = embedding.Vector.ToArray();

        // 2. Define the Vector Search Query
        // syntax: VectorDistance(c.vector, @embedding)
        var sqlQueryText = @"
            SELECT TOP @limit
                c.content,
                c.source as source,
                VectorDistance(c.vector, @vector) as score
            FROM c
            ORDER BY VectorDistance(c.vector, @vector)";

        var queryDefinition = new QueryDefinition(sqlQueryText)
            .WithParameter("@limit", limit)
            .WithParameter("@vector", vector);

        // 3. Execute Query
        var iterator = _container.GetItemQueryIterator<CosmosSearchResultDto>(queryDefinition);
        var results = new List<SearchResult>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            foreach (var item in response)
            {
                results.Add(new SearchResult(item.Content, item.Source, item.Score));
            }
        }

        return results;
    }

        // Private DTO to map the exact JSON return from Cosmos
    private class CosmosSearchResultDto
    {
        public string Content { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}