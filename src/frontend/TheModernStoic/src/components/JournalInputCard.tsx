import { useState } from "react";
import type { JournalResponse } from "../types/journal";

interface JournalInputCardProps {
  onSubmit: (text: string) => Promise<JournalResponse | null>;
  loading: boolean;
  error: string | null;
  onResponse: (response: JournalResponse | null) => void;
}

function JournalInputCard({ onSubmit, loading, error, onResponse }: JournalInputCardProps) {
  const [input, setInput] = useState("");

  const handleReflect = async () => {
    if (!input) return;
    const journalResponse = await onSubmit(input);
    //callbackfunction, effectively "passing" JournalResponse to App.tsx
    onResponse(journalResponse);
    setInput("");
  };

  return (
    <div className="bg-white p-6 sm:p-8 rounded-lg shadow-sm border border-stoic-subtle text-left animate-fade-in">
      <h2 className="text-xl text-stoic-ink mb-4 font-serif">
        What troubles you, friend?
      </h2>
      <textarea
        className="w-full bg-stoic-paper border border-stoic-subtle rounded p-4 text-stoic-ink 
        focus:outline-none focus:border-stoic-sand focus:ring-1 focus:ring-stoic-sand 
        transition-all font-serif resize-none"
        rows={6}
        value={input}
        onChange={(e) => setInput(e.target.value)}
        disabled={loading}
        placeholder="Write your thoughts here..."
      />

      <div className="mt-4 flex items-center justify-between">
        <span className="text-xs text-stoic-charcoal italic">
          {loading ? "The Stoic is contemplating..." : "Space to reflect."}
        </span>
        <button
          onClick={handleReflect}
          disabled={loading}
          className="bg-stoic-ink text-stoic-paper px-6 py-2 rounded font-sans text-sm tracking-wide 
          hover:bg-stoic-charcoal transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {loading ? "Consulting..." : "Reflect"}
        </button>
      </div>

      {error && (
        <div className="mt-4 p-3 bg-red-50 text-red-800 text-sm border border-red-100 rounded">
          {error}
        </div>
      )}
    </div>
  );
}

export default JournalInputCard;
