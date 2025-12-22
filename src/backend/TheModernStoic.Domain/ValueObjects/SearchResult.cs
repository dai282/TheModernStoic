namespace TheModernStoic.Domain.ValueObjects;

public record SearchResult
(
    string Content,
    string Source,
    double Score // 0 to 1. Lower is more relevant.
);