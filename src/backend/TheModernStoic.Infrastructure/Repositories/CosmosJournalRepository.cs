using Microsoft.Azure.Cosmos;
using TheModernStoic.Domain.Entities;
using TheModernStoic.Domain.Interfaces;

namespace TheModernStoic.Infrastructure.Repositories;

public class CosmosJournalRepository : IJournalRepository
{
    private readonly Container _container;

    public CosmosJournalRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task AddEntryAsync (JournalEntry entry)
    {
        await _container.CreateItemAsync(entry, new PartitionKey(entry.UserId));
    }

    public async Task<IEnumerable<JournalEntry>> GetEntriesAsync(string userId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId ORDER BY c.CreatedAt DESC")
            .WithParameter("@userId", userId);

        var iterator = _container.GetItemQueryIterator<JournalEntry>(query);
        var results = new List<JournalEntry>();

        // Iterate through the results and add all entries to the list
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task DeleteEntryAsync(string userId, string entryId)
    {
        await _container.DeleteItemAsync<JournalEntry>(entryId, new PartitionKey(userId));
    }

}