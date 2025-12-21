
using System.Text.Json.Serialization;
namespace TheModernStoic.Domain.Entities;
public class JournalEntry
{
    [JsonPropertyName("id")] // Cosmos requires lower case 'id'
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("userId")] // Matches Partition Key path in Bicep
    public string UserId { get; set; } = "guest-user"; 

    public string UserText { get; set; }
    public string StoicResponse { get; set; }

    // We store the vector too, in case we want to search history later!
    // Matches path: '/embedding' in Bicep
    [JsonPropertyName("embedding")] 
    public float[] Embedding { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Optional: Store the quotes used for reference?
    // public List<Quote> ReferenceQuotes { get; set; } 
}