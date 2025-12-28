using Microsoft.Extensions.AI;
using NSubstitute;
using OpenAI.Chat;
using TheModernStoic.Application.Interfaces;
using TheModernStoic.Domain.Entities;
using TheModernStoic.Domain.Interfaces;
using TheModernStoic.Domain.ValueObjects;
using TheModernStoic.Infrastructure.Services;

namespace TheModernStoic.Tests.Core
{
    //mock the database and AI client, test the logic
    public class JournalServiceTests
    {
        private readonly IJournalRepository _repoMock; //Mock Database
        private readonly IChatClient _chatMock; //Mock AI
        private readonly IVectorSearchService _vectorSearchMock;

        private readonly ICurrentUserService _currentUserServiceMock;

        private readonly JournalService _sut; //System Under Test

        public JournalServiceTests()
        {
            _repoMock = Substitute.For<IJournalRepository>();
            _chatMock = Substitute.For<IChatClient>();
            _vectorSearchMock = Substitute.For<IVectorSearchService>();
            _currentUserServiceMock = Substitute.For<ICurrentUserService>();

            //setup SUT with mocks
            _sut = new JournalService( _chatMock, _vectorSearchMock, _repoMock, _currentUserServiceMock);
        }

        [Fact]
        public async Task ProcessJournalEntryAsync_ShouldIncludeVectorContext_InChatPrompt()
        {
            //Arrange
            string userText = "I am anxious about the future.";
            string fakeQuote = "The future disturbs you? Leave it alone.";
            string fakeAdvice = "Focus on the present.";

            //1. Setup vector search to return a specific quote
            var searchResults = new List<SearchResult>
            {
                new SearchResult(fakeQuote, "Meditations Book 8", 0.95 )
            };
            _vectorSearchMock.SearchAsync(userText).Returns(searchResults);

            //2. Setup chat client to return a specific advice
            // Note: Microsoft.Extensions.AI uses ChatCompletion as the return type
            var chatResponse = new ChatResponse(new Microsoft.Extensions.AI.ChatMessage(
                ChatRole.Assistant,fakeAdvice));

            _chatMock.GetResponseAsync(
                Arg.Any<IEnumerable<Microsoft.Extensions.AI.ChatMessage>>(),
                Arg.Any<ChatOptions>(),
                Arg.Any<CancellationToken>()
                ).Returns(chatResponse);

            //ACT
            var result = await _sut.ProcessJournalEntryAsync(userText);

            //ASSERT

            //Check 1: Did we get the response?
            Assert.Equal(fakeAdvice, result.StoicAdvice);

            //Check 2: Did the logic construct the prompt correctly?
            // We inspect the arguments passed to GetResponseAsync

            await _chatMock.Received(1).GetResponseAsync(
                Arg.Is<IEnumerable<Microsoft.Extensions.AI.ChatMessage>>(messages =>
                    // Ensure System Prompt or User Prompt contains the retrieved quote
                    messages.Any(m => m.Text.Contains(fakeQuote)) &&
                    messages.Any(m => m.Text.Contains(userText))
                ),
                Arg.Any<ChatOptions>(),
                Arg.Any<CancellationToken>()
            );
        }

        [Fact]

        public async Task ProcessJournalEntryAsync_ShouldPersist_ToRepository()
        {
            // ARRANGE
            string userText = "Testing persistence.";
            var chatResponse = new ChatResponse(new Microsoft.Extensions.AI.ChatMessage(
                ChatRole.Assistant, "Saved."));

            _vectorSearchMock.SearchAsync(Arg.Any<string>()).Returns(new List<SearchResult>());
            _chatMock.GetResponseAsync(Arg.Any<IEnumerable<Microsoft.Extensions.AI.ChatMessage>>(), 
                                        null, default).Returns(chatResponse);

            // ACT
            await _sut.ProcessJournalEntryAsync(userText);

            // ASSERT
            // Verify AddEntryAsync was called exactly once with the correct data
            await _repoMock.Received(1).AddEntryAsync(
                Arg.Is<JournalEntry>(entry =>
                    entry.UserText == userText &&
                    entry.StoicResponse == "Saved." &&
                    entry.UserId == _currentUserServiceMock.UserId
                )
            );
        }
    }
}
