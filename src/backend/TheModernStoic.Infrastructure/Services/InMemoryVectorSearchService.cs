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
            // For E2E tests, we don't need real search results.
            // Returning an empty list is sufficient to prevent crashes.
            return Task.FromResult(Enumerable.Empty<SearchResult>());
        }
    }
}
