//represents a piece of text + vector
namespace TheModernStoic.Domain.Entities;

public class KnowledgeChunk
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; } = string.Empty; // The actual quote
    public string Source { get; set; } = string.Empty;  // e.g., "Meditations - Book 1"
    
    // This is the vector representation. 
    // all-MiniLM-L6-v2 produces a vector of size 384.
    public float[] Vector { get; set; } = []; 
}