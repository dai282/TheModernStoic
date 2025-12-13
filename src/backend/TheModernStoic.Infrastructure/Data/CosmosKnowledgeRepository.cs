//This is where the Senior-level code lives. We must code the container creation to
//automatically apply the Vector Policy if the container doesn't exist.

using TheModernStoic.Domain.Entities;
using TheModernStoic.Domain.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;

namespace TheModernStoic.Infrastructure.Data
{
    public class CosmosKnowledgeRepository : IKnowledgeRepository
    {
        private readonly CosmosClient _cosmosClient;
        private readonly IConfiguration _configuration;

        private Container? _container;

        // We default to "ModernStoicDb" if not in config, but we prefer config.
        // The "Knowledge" container is distinct from "Entries"
        private readonly string _dbName = "ModernStoicDb";
        private readonly string _containerName = "Knowledge";

        public CosmosKnowledgeRepository(CosmosClient cosmosClient, IConfiguration configuration)
        {
            _cosmosClient = cosmosClient;
            _configuration = configuration;

            var configuredDb = _configuration["CosmosDb:DatabaseName"];
            if (!string.IsNullOrEmpty(configuredDb)) _dbName = configuredDb;

        }

        public async Task InitializeAsync()
        {
            //Create database if no exists
            Database database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_dbName);

            // 2. Define Vector Embedding Policy (Must match ONNX model: 384 dims)
            var vectorEmbeddingPolicy = new VectorEmbeddingPolicy(new Collection<Embedding>
            {
                new Embedding
                {
                    Path = "/vector", // The property name in KnowledgeChunk
                    DataType = VectorDataType.Float32,
                    Dimensions = 384, // <--- CRITICAL: Matches all-MiniLM-L6-v2
                    DistanceFunction = DistanceFunction.Cosine
                }
            });

            // 3. Define Indexing Policy
            var indexingPolicy = new IndexingPolicy
            {
                Automatic = true
            };

            // This tells Cosmos: "Index everything in the document by default..."
            indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/*" });

            // "...EXCEPT the /Vector field (which we will handle with a Vector Index)"
            // Exclude /Vector/* from standard text indexing to save costs
            indexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = "/vector/*" });

            // Add Vector Index
            indexingPolicy.VectorIndexes.Add(new VectorIndexPath
            {
                Path = "/vector",
                Type = VectorIndexType.QuantizedFlat // Standard vector index
            });

            // 4. Create Container
            // Partition Key: /id (Random distribution)
            ContainerProperties properties = new ContainerProperties(id: _containerName, partitionKeyPath: "/id")
            {
                VectorEmbeddingPolicy = vectorEmbeddingPolicy,
                IndexingPolicy = indexingPolicy
            };

            _container = await database.CreateContainerIfNotExistsAsync(properties);
        }

        //insert chunks into the container
        public async Task UpsertBatchAsync(IEnumerable<KnowledgeChunk> chunks)
        {
            if (_container == null)
            {
                await InitializeAsync();
            }

            var tasks = new List<Task>();

            foreach (var chunk in chunks)
            {
                // KnowledgeChunk.Id is a Guid, so chunk.Id.ToString() is the Partition Key
                tasks.Add(_container!.UpsertItemAsync(chunk, new PartitionKey(chunk.Id.ToString())));
            }

            // Run all upserts in parallel (Bulk support must be enabled in Client)
            await Task.WhenAll(tasks);

        }
    }
}
