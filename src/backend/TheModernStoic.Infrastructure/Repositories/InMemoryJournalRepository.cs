using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheModernStoic.Domain.Entities;
using TheModernStoic.Domain.Interfaces;

namespace TheModernStoic.Infrastructure.Repositories
{
    public class InMemoryJournalRepository : IJournalRepository
    {
        // Store data as: ConcurrentDictionary<userId, ConcurrentDictionary<entryId, JournalEntry>>
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalEntry>> _entries = new();

        public Task AddEntryAsync(JournalEntry entry)
        {
            var userEntries = _entries.GetOrAdd(entry.UserId, new ConcurrentDictionary<string, JournalEntry>());
            userEntries[entry.Id] = entry;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<JournalEntry>> GetEntriesAsync(string userId)
        {
            if (_entries.TryGetValue(userId, out var userEntries))
            {
                return Task.FromResult(userEntries.Values.AsEnumerable());
            }
            return Task.FromResult(Enumerable.Empty<JournalEntry>());
        }

        public Task DeleteEntryAsync(string userId, string entryId)
        {
            if (_entries.TryGetValue(userId, out var userEntries))
            {
                userEntries.TryRemove(entryId, out _);
            }
            return Task.CompletedTask;
        }
    }
}
