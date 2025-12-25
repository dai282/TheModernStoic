import type { JournalEntry } from "../types/journal";

interface HistoryFeedProps {
  entries: JournalEntry[];
}

function HistoryFeed({ entries }: HistoryFeedProps) {
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
          <p className="font-semibold text-gray-700">"{entry.userText}"</p>
          {/* Stoic Response - Designed to look like a quote */}
          <div className="pl-4 border-l-2 border-stoic-sand">
            <p className="text-stoic-ink font-serif leading-relaxed">
              {entry.stoicResponse}
            </p>
          </div>

          {/* Meta */}
          <div className="mt-4 flex justify-end">
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
