namespace TheModernStoic.Domain.Interfaces;

public interface IContentProcessor
{
    //orchestrator for cleaning and chunking the text
    IEnumerable<string> Process(string rawText);
}