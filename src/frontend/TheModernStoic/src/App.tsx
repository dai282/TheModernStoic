import { useState } from "react";
import reactLogo from "./assets/react.svg";
import viteLogo from "/vite.svg";
import "./App.css";
import { useJournal } from "./hooks/useJournal";

function App() {
  const { entries, loading, error, submitEntry } = useJournal();

  const [input, setInput] = useState("");

  const handleReflect = async () => {
    if (!input) return;
    await submitEntry(input);
    setInput("");
  };

  return (
    <div className="p-10 max-w-2xl mx-auto font-sans">
      <h1 className="text-3xl font-bold mb-6">The Modern Stoic (Dev Mode)</h1>

      {/* Input Section */}
      <div className="mb-8 border p-4 rounded">
        <textarea
          className="w-full border p-2"
          rows={3}
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="What troubles you?"
        />
        <button
          onClick={handleReflect}
          disabled={loading}
          className="bg-black text-white px-4 py-2 mt-2 rounded disabled:opacity-50"
        >
          {loading ? "Consulting..." : "Reflect"}
        </button>
        {error && <p className="text-red-500 mt-2">{error}</p>}
      </div>

      {/* History Feed */}
      <h2 className="text-xl font-bold mb-4">History</h2>
      <div className="space-y-4">
        {entries.map((entry, idx) => (
          <div key={idx} className="bg-gray-100 p-4 rounded">
            <p className="font-semibold text-gray-700">"{entry.userText}"</p>
            <p className="mt-2 text-gray-900 border-l-4 border-black pl-3 italic">
              {entry.stoicResponse}
            </p>
            <span className="text-xs text-gray-500">
              {new Date(entry.date).toLocaleDateString()}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}

export default App;
