import { useState } from "react";
import "./App.css";
import { useJournal } from "./hooks/useJournal";
import JournalInputCard from "./components/JournalInputCard";
import HistoryFeed from "./components/HistoryFeed";
import { useAuth0 } from "@auth0/auth0-react";
import { LoginButton, LogoutButton } from "./components/Auth";
import type { JournalResponse } from "./types/journal";

function App() {
  const { isAuthenticated, isLoading: isAuthLoading, user } = useAuth0();
  const { entries, loading, error, submitEntry, deleteEntry } = useJournal();

  const [activeTab, setActiveTab] = useState<"journal" | "history">("journal");
  const [responseOverlay, setResponseOverlay] = useState<{
    response: JournalResponse | null;
    visible: boolean;
    fading: boolean;
  }>({ response: null, visible: false, fading: false });

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
        <div>
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
              //callback function as prop
              //When this function is called with a parameter 'res', it checks if res exists, and then set the reponse overlay state
              onResponse={(res) => {
                if (res)
                  setResponseOverlay({
                    response: res,
                    visible: true,
                    fading: false,
                  });
              }}
            />
          ) : (
            <HistoryFeed
              entries={entries}
              deleteEntry={deleteEntry}
            />
          )}
        </div>
      </main>

      {/* Response Overlay */}
      {((loading && activeTab === "journal") || responseOverlay.visible) && (
        <div
          className={`fixed inset-0 bg-black/90 flex items-center justify-center z-50 transition-opacity duration-500 ${
            responseOverlay.fading ? "opacity-0" : "opacity-100"
          }`}
        >
          <div className=" w-[70%] ">
            {loading ? (
              <div className="text-center">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-stoic-paper mx-auto mb-4"></div>
                <p className="text-stoic-paper font-serif">
                  The Stoic is contemplating...
                </p>
              </div>
            ) : (
              <div className="text-center">
                <div className="mb-4 animate-fade-in">
                  <p className="text-stoic-paper font-serif text-lg leading-relaxed italic">
                    {responseOverlay.response?.stoicAdvice}
                  </p>
                </div>
                <button
                  onClick={() => {
                    //Fade out effect
                    setResponseOverlay((prev) => ({ ...prev, fading: true }));
                    setTimeout(
                      () =>
                        setResponseOverlay({
                          response: null,
                          visible: false,
                          fading: false,
                        }),
                      500
                    );
                  }}
                  className="mt-4 px-4 py-2 text-stoic-paper rounded hover:bg-stoic-charcoal"
                >
                  Close
                </button>
              </div>
            )}
          </div>
        </div>
      )}

      <footer className="mt-20 text-stoic-sand text-xs font-sans text-center">
        Memento Mori • {new Date().getFullYear()} • {user?.email}
      </footer>
    </div>
  );
}

export default App;
