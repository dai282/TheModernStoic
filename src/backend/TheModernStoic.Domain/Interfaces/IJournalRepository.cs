using TheModernStoic.Domain.Entities;

namespace TheModernStoic.Domain.Interfaces;

public interface IJournalRepository
{
    Task AddEntryAsync(JournalEntry entry);
    Task<IEnumerable<JournalEntry>> GetEntriesAsync(string userId);
}