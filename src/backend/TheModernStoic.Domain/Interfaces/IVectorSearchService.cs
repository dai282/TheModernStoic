using TheModernStoic.Domain.ValueObjects;

namespace TheModernStoic.Domain.Interfaces;

public interface IVectorSearchService
{
    // We ask for a query and how many results (k) we want
    Task<IEnumerable<SearchResult>> SearchAsync(string query, int limit = 3);
}