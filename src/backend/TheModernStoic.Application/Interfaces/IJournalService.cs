namespace TheModernStoic.Domain.Interfaces;

public interface IJournalService
{
    Task<JournalResponseDto> ProcessJournalEntryAsync(string userText);
}