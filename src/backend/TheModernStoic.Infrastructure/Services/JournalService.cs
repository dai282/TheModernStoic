using System.Text;
using Microsoft.Extensions.AI;
using TheModernStoic.Application.DTOs;
using TheModernStoic.Domain.Entities;
using TheModernStoic.Domain.Interfaces;

namespace TheModernStoic.Infrastructure.Services;

//Orchestrates the journal processing workflow
public class JournalService : IJournalService
{
    private readonly IChatClient _chatClient;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly IJournalRepository _journalRepository;

    public JournalService(
        IChatClient chatClient,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IVectorSearchService vectorSearchService,
        IJournalRepository journalRepository
    )
    {
        _chatClient = chatClient;
        _embeddingGenerator = embeddingGenerator;
        _vectorSearchService = vectorSearchService;
        _journalRepository = journalRepository;
    }

    public async Task<JournalResponseDto> ProcessJournalEntryAsync(string userText)
    {
        //1. Return search results (Cosmos Vector Search Service already does the embedding for us)
        var searchResults = await _vectorSearchService.SearchAsync(userText);

        //Format the context for the LLM, combine them into a single paragraph/string
        var contextBuilder = new StringBuilder();
        foreach (var result in searchResults)
        {
            contextBuilder.AppendLine($"- \"{result.Content}\" (Source: {result.Source})");
        }

        string retrievedContext = contextBuilder.ToString();

        //2. Contruct the prompt
        var messages = new List<ChatMessage>
        {
            new (ChatRole.System,
                "You are Marcus Aurelius, Roman Emperor and Stoic philosopher. " +
                    "Analyze the user's journal entry. " +
                    "Use the provided context from your own writings (Meditations) to offer specific, stoic advice. " +
                    "Do not be preachy; be empathetic but firm. " +
                    "If the context matches well, quote it directly in your response."
            ),

            new (ChatRole.User, 
                $"Here is the context from your writings:\n{retrievedContext}\n\n" +
                    $"User Journal Entry: \"{userText}\""
            )
        };

        // 3. Generate Advice (Remote API - HuggingFace)
        var chatResponse = await _chatClient.GetResponseAsync(messages);

        //4. Create Journal Entry Entity (with user text, AI advice, date, etc.)
        var entry = new JournalEntry
        {
            UserText = userText,
            StoicResponse = chatResponse.Text,
            CreatedAt = DateTime.UtcNow,
            UserId = "guest-user" // Hardcoded for now, extracted from Context later
        };

        //5. Persist to Cosmos DB
        await _journalRepository.AddEntryAsync(entry);

        //6. Return Result
        return new JournalResponseDto
        {
            UserText = userText,
            StoicAdvice = chatResponse.Text ?? "I am silent at the moment.",
            CitedQuotes = searchResults.Select(x => x.Content).ToList()
        };
    }

        // Add the new method for History
    public async Task<IEnumerable<JournalEntryDto>> GetHistoryAsync()
    {
        var entries = await _journalRepository.GetEntriesAsync("guest-user");
        // Map to DTO
        return entries.Select(e => new JournalEntryDto 
        { 
            Id = e.Id,
            Date = e.CreatedAt,
            UserText = e.UserText,         
            StoicResponse = e.StoicResponse
        });
    }

}