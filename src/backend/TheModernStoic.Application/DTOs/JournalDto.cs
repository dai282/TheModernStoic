namespace TheModernStoic.Application.DTOs;

// INPUT: User sending a new entry
public class CreateJournalDto
{
    public string Text { get; set; } = string.Empty;
}

// OUTPUT: The immediate response after chatting (already exists in your code)
public class JournalResponseDto
{
    public string UserText {get; set;} = string.Empty;
    public string StoicAdvice { get; set; } = string.Empty;
    public List<string> CitedQuotes { get; set; } = new();
}

// OUTPUT: The list for the "History" page
public class JournalEntryDto
{
    public string Id { get; set; }
    public DateTime Date { get; set; }
    public string UserText { get; set; }       // The Full User Input
    public string StoicResponse { get; set; }  // The Full AI Advice
}