using System;
using System.Collections.Generic;
using System.Text;
using TheModernStoic.Domain.Entities;

namespace TheModernStoic.Domain.Interfaces
{
    public interface IKnowledgeRepository
    {
        // Initialize DB/Container with Vector Policies
        Task InitializeAsync();

        // Bulk or Single Insert
        Task UpsertBatchAsync(IEnumerable<KnowledgeChunk> chunks);
    }
}
