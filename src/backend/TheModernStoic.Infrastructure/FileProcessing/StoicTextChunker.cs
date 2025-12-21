using TheModernStoic.Domain.Interfaces;
using Microsoft.SemanticKernel.Text; 

namespace TheModernStoic.Infrastructure.FileProcessing;

                            //nice dependency injection
public class StoicTextChunker (GutenbergCleaner cleaner) : IContentProcessor
{
    public IEnumerable<string> Process(string rawText)
    {
        // 1. Clean the text
        var cleanText = cleaner.Clean(rawText);

        // 2. Chunking Strategy
        // Marcus Aurelius writes in "Books" and distinct paragraphs (aphorisms).
        // We want to capture whole paragraphs.

        // Using Semantic Kernel's built-in text chunker is efficient.
        // It splits by paragraph first, then falls back to sentences if lines are too long.
        #pragma warning disable SKEXP0050
        var lines = TextChunker.SplitPlainTextLines(
            cleanText,
            maxTokensPerLine: 100 //Perfer smaller lines for vector search
        );

        var paragraphs = TextChunker.SplitPlainTextParagraphs(
            lines, 
            maxTokensPerParagraph: 300, // Approx 1000-1200 characters
            overlapTokens: 30 // Vital: Overlap helps maintain context across boundaries
        );
        
        #pragma warning restore SKEXP0050

        return paragraphs;
    }
}