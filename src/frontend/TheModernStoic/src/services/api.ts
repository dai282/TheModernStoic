import type { JournalEntry, JournalResponse } from "../types/journal";
import axios from "axios";

// TODO: In production, this comes from import.meta.env.VITE_API_URL
// For now, copy the HTTPS URL from your Aspire Dashboard (e.g., https://localhost:7231)
const API_BASE = "http://localhost:5289/api";

export const journalService = {
  async submitUserEntry(userText: string): Promise<JournalResponse> {
    try {
      const response = await axios.post<JournalResponse>(
        `${API_BASE}/journal`,
        { text: userText } // This is the request body (will be JSON.stringified)
      );
      return response.data;
    } catch (error) {
      console.error("Failed to submit journal entry:", error);
      throw new Error("Failed to submit journal entry.");
    }
  },

  async getHistory(): Promise<JournalEntry[]> {
    try {
      const response = await axios.get<JournalEntry[]>(`${API_BASE}/journal`);
      return response.data;
    } catch (error) {
      console.error("Failed to load history:", error);
      throw new Error("Failed to load history.");
    }
  },
};
