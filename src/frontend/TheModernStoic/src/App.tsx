import { useState } from "react";
import "./App.css";
import { useJournal } from "./hooks/useJournal";
import JournalInputCard from "./components/JournalInputCard";
import HistoryFeed from "./components/HistoryFeed";
import { useAuth0 } from "@auth0/auth0-react";
import { LoginButton, LogoutButton } from "./components/Auth";

function App() {
  const { isAuthenticated, isLoading: isAuthLoading } = useAuth0();
  const { entries, loading, error, submitEntry } = useJournal();

  const [activeTab, setActiveTab] = useState<"journal" | "history">("journal");

  if (isAuthLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-stoic-paper text-stoic-ink">
        Loading...
      </div>
    );
  }

  // View 1: Not Authenticated (Landing Page)
  if (!isAuthenticated) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center bg-stoic-paper text-stoic-ink p-4 animate-fade-in">
        <h1 className="text-5xl font-serif italic mb-4">The Modern Stoic</h1>
        <p className="mb-8 text-stoic-charcoal">
          "The soul becomes dyed with the color of its thoughts."
        </p>
        <LoginButton />
      </div>
    );
  }

  // View 2: Authenticated App
  return (
    <div className="min-h-screen flex flex-col items-center py-12 px-4 sm:px-6 bg-stoic-paper text-stoic-ink font-sans antialiased">
      {/* Header */}
      <header className="mb-12 text-center space-y-2 animate-fade-in">
        <h1 className="text-4xl sm:text-5xl italic tracking-tight font-serif text-stoic-ink">
          The Modern Stoic
        </h1>
        <p className="text-stoic-charcoal font-light text-lg">
          Dialogue with Marcus Aurelius
        </p>
        <div className="absolute right-0 top-0">
          <LogoutButton />
        </div>
      </header>

      {/*Main Container*/}
      <main className="w-full max-w-2xl space-y-8">
        {/*Navigation Tabs*/}
        <button
          onClick={() => setActiveTab("journal")}
          className={`pb-2 px-6 text-sm font-medium transition-colors ${
            activeTab === "journal"
              ? "border-b-2 border-stoic-accent text-stoic-accent"
              : "text-stoic-charcoal hover:text-stoic-ink"
          }`}
        >
          Journal
        </button>

        <button
          onClick={() => setActiveTab("history")}
          className={`pb-2 px-6 text-sm font-medium transition-colors ${
            activeTab === "history"
              ? "border-b-2 border-stoic-accent text-stoic-accent"
              : "text-stoic-charcoal hover:text-stoic-ink"
          }`}
        >
          History
        </button>

        {/* Content Area */}
        <div className="transition-all duration-300">
          {activeTab === "journal" ? (
            <JournalInputCard
              onSubmit={submitEntry}
              loading={loading}
              error={error}
            />
          ) : (
            <HistoryFeed entries={entries} />
          )}
        </div>
      </main>

      <footer className="mt-20 text-stoic-sand text-xs font-sans text-center">
        Memento Mori • {new Date().getFullYear()}
      </footer>
    </div>
  );
}

export default App;
