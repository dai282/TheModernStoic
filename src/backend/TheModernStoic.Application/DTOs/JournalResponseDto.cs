public class JournalResponseDto
{
    public string UserText {get; set;} = string.Empty;
    public string StoicAdvice { get; set; } = string.Empty;
    public List<string> CitedQuotes { get; set; } = new();
}