using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheModernStoic.Domain.Interfaces;
using TheModernStoic.Domain.ValueObjects;

namespace TheModernStoic.Infrastructure.Services
{
    public class InMemoryVectorSearchService : IVectorSearchService
    {
        public Task<IEnumerable<SearchResult>> SearchAsync(string query, int limit = 3)
        {
            // For E2E tests, return a single, predictable mock result 
            // based on the query to make the service's behavior more realistic.
            var mockResults = new List<SearchResult>
            {
                new SearchResult
                {
                    Source = "In-Memory Test",
                    Content = $"A mock stoic quote related to: '{query}'",
                    Score = 1
                }
            };
            return Task.FromResult(mockResults.AsEnumerable());
        }
    }
}
