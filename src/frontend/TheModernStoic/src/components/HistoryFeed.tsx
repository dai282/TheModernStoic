import type { JournalEntry } from "../types/journal";

interface HistoryFeedProps {
  entries: JournalEntry[];
  deleteEntry: (entryId: string) => Promise<void>;
  loading: boolean;
}

function HistoryFeed({ entries, deleteEntry, loading }: HistoryFeedProps) {
  if (entries.length === 0) {
    return (
      <div className="text-center text-stoic-charcoal italic mt-10">
        No entries yet. Begin your journey.
      </div>
    );
  }
  return (
    <div className="space-y-4">
      {entries.map((entry, idx) => (
        <div
          key={idx}
          className="bg-white p-6 rounded-lg shadow-sm border border-stoic-subtle text-left animate-fade-in"
        >
          {/* User's thought */}
          <p className="font-semibold text-gray-700">{entry.userText}</p>
          {/* Stoic Response - Designed to look like a quote */}
          <div className="pl-4 border-l-2 border-stoic-sand">
            <p className="text-stoic-ink font-serif leading-relaxed italic ">
              {entry.stoicResponse}
            </p>
          </div>

          <div className="mt-4 flex justify-between items-center">
            <button
              disabled={loading}
              onClick={() => deleteEntry(entry.id)}
              className="bg-stoic-accent text-stoic-paper px-3 py-1 rounded text-sm font-medium 
                      hover:bg-stoic-accent/90 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? "Deleting..." : "Delete"}
            </button>
            {/* Meta */}
            <span className="text-xs text-stoic-sand uppercase tracking-wider font-sans">
              {new Date(entry.date).toLocaleDateString()}
            </span>
          </div>
        </div>
      ))}
    </div>
  );
}

export default HistoryFeed;
