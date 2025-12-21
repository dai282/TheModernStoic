using TheModernStoic.Application.DTOs;

namespace TheModernStoic.Domain.Interfaces;

public interface IJournalService
{
    Task<JournalResponseDto> ProcessJournalEntryAsync(string userText);

    Task<IEnumerable<JournalEntryDto>> GetHistoryAsync();
}