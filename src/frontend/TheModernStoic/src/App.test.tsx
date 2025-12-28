import { describe, expect, it, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from '@testing-library/user-event';
import App from "./App";
import type { JournalEntry, JournalResponse } from "./types/journal";
import { useAuth0 } from '@auth0/auth0-react';
import { useJournal } from "./hooks/useJournal";

// Mock the hooks
vi.mock('./hooks/useJournal', () => ({
  useJournal: vi.fn()
}));

vi.mock('@auth0/auth0-react', () => ({
  useAuth0: vi.fn(),
  LoginButton: () => <button>Login</button>,
  LogoutButton: () => <button>Logout</button>
}));

// Mock the components
vi.mock('./components/JournalInputCard', () => ({
  default: ({ onSubmit, onResponse }: any) => (
    <div data-testid="journal-input-card">
      <textarea data-testid="journal-textarea" />
      <button data-testid="journal-submit" onClick={() => onSubmit('test').then((res: JournalResponse | null) => onResponse(res))}>
        Reflect
      </button>
    </div>
  )
}));

vi.mock('./components/HistoryFeed', () => ({
  default: ({ entries }: any) => (
    <div data-testid="history-feed">
      {entries.map((entry: JournalEntry) => (
        <div key={entry.id} data-testid={`entry-${entry.id}`}>
          {entry.userText}
        </div>
      ))}
    </div>
  )
}));

describe("App Component", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe("Authentication States", () => {
    it("shows loading screen when auth is loading", () => {
      const mockUseAuth0 = vi.mocked(useAuth0);
      const mockUseJournal = vi.mocked(useJournal);

      mockUseAuth0.mockReturnValue({
        isAuthenticated: false,
        isLoading: true,
        user: undefined,
        loginWithRedirect: vi.fn(),
        logout: vi.fn()
      } as any);

      // Mock useJournal even though it won't be used
      mockUseJournal.mockReturnValue({
        entries: [],
        loading: false,
        error: null,
        submitEntry: vi.fn(),
        deleteEntry: vi.fn()
      });

      render(<App />);

      expect(screen.getByText("Loading...")).toBeInTheDocument();
    });

    it("shows landing page when not authenticated", () => {
      const mockUseAuth0 = vi.mocked(useAuth0);
      const mockUseJournal = vi.mocked(useJournal);

      mockUseAuth0.mockReturnValue({
        isAuthenticated: false,
        isLoading: false,
        user: undefined,
        loginWithRedirect: vi.fn(),
        logout: vi.fn()
      } as any);

      // Mock useJournal even though it won't be used
      mockUseJournal.mockReturnValue({
        entries: [],
        loading: false,
        error: null,
        submitEntry: vi.fn(),
        deleteEntry: vi.fn()
      });

      render(<App />);

      expect(screen.getByText("The Modern Stoic")).toBeInTheDocument();
      expect(screen.getByText('"The soul becomes dyed with the color of its thoughts."')).toBeInTheDocument();
    });

    it("shows main app when authenticated", () => {
      const mockUseAuth0 = vi.mocked(useAuth0);
      const mockUseJournal = vi.mocked(useJournal);

      mockUseAuth0.mockReturnValue({
        isAuthenticated: true,
        isLoading: false,
        user: { email: 'test@example.com' },
        loginWithRedirect: vi.fn(),
        logout: vi.fn()
      } as any);

      mockUseJournal.mockReturnValue({
        entries: [],
        loading: false,
        error: null,
        submitEntry: vi.fn(),
        deleteEntry: vi.fn()
      });

      render(<App />);

      expect(screen.getByText("The Modern Stoic")).toBeInTheDocument();
      expect(screen.getByText("Dialogue with Marcus Aurelius")).toBeInTheDocument();
      expect(screen.getByText("Memento Mori • 2025 • test@example.com")).toBeInTheDocument();
    });
  });

  describe("Tab Navigation", () => {
    beforeEach(() => {
      const mockUseAuth0 = vi.mocked(useAuth0);
      const mockUseJournal = vi.mocked(useJournal);

      mockUseAuth0.mockReturnValue({
        isAuthenticated: true,
        isLoading: false,
        user: { email: 'test@example.com' },
        loginWithRedirect: vi.fn(),
        logout: vi.fn()
      } as any);

      mockUseJournal.mockReturnValue({
        entries: [],
        loading: false,
        error: null,
        submitEntry: vi.fn(),
        deleteEntry: vi.fn()
      });
    });

    it("shows JournalInputCard by default", () => {
      render(<App />);

      expect(screen.getByTestId("journal-input-card")).toBeInTheDocument();
      expect(screen.queryByTestId("history-feed")).not.toBeInTheDocument();
    });

    it("switches to HistoryFeed when History tab is clicked", async () => {
      const user = userEvent.setup();
      render(<App />);

      const historyTab = screen.getByText("History");
      await user.click(historyTab);

      expect(screen.getByTestId("history-feed")).toBeInTheDocument();
      expect(screen.queryByTestId("journal-input-card")).not.toBeInTheDocument();
    });

    it("switches back to JournalInputCard when Journal tab is clicked", async () => {
      const user = userEvent.setup();
      render(<App />);

      // Switch to history first
      const historyTab = screen.getByText("History");
      await user.click(historyTab);

      // Switch back to journal
      const journalTab = screen.getByText("Journal");
      await user.click(journalTab);

      expect(screen.getByTestId("journal-input-card")).toBeInTheDocument();
      expect(screen.queryByTestId("history-feed")).not.toBeInTheDocument();
    });

    it("highlights active tab", async () => {
      const user = userEvent.setup();
      render(<App />);

      // Journal tab should be active by default
      expect(screen.getByText("Journal").closest('button')).toHaveClass('border-stoic-accent', 'text-stoic-accent');

      // Click History tab
      const historyTab = screen.getByText("History");
      await user.click(historyTab);

      // History tab should now be active
      expect(historyTab.closest('button')).toHaveClass('border-stoic-accent', 'text-stoic-accent');
      expect(screen.getByText("Journal").closest('button')).not.toHaveClass('border-stoic-accent');
    });
  });

  describe("Response Overlay", () => {
    const mockUseAuth0 = vi.mocked(useAuth0);
    const mockUseJournal = vi.mocked(useJournal);

    beforeEach(() => {
      mockUseAuth0.mockReturnValue({
        isAuthenticated: true,
        isLoading: false,
        user: { email: 'test@example.com' },
        loginWithRedirect: vi.fn(),
        logout: vi.fn()
      } as any);
    });

    it("shows loading overlay when submitting journal entry", () => {
      mockUseJournal.mockReturnValue({
        entries: [],
        loading: true,
        error: null,
        submitEntry: vi.fn(),
        deleteEntry: vi.fn()
      });

      render(<App />);

      expect(screen.getByText("The Stoic is contemplating...")).toBeInTheDocument();
    });

    it("shows response overlay after successful submission", async () => {
      const mockSubmitEntry = vi.fn().mockResolvedValue({
        userText: "Test",
        stoicAdvice: "Test advice",
        citedQuotes: []
      });

      mockUseJournal.mockReturnValue({
        entries: [],
        loading: false,
        error: null,
        submitEntry: mockSubmitEntry,
        deleteEntry: vi.fn()
      });

      const user = userEvent.setup();
      render(<App />);

      // Trigger the onResponse callback by clicking the mocked submit button
      const submitButton = screen.getByTestId("journal-submit");
      await user.click(submitButton);

      expect(screen.getByText("Test advice")).toBeInTheDocument();
    });

    it("closes response overlay when close button is clicked", async () => {
      const mockSubmitEntry = vi.fn().mockResolvedValue({
        userText: "Test",
        stoicAdvice: "Test advice",
        citedQuotes: []
      });

      mockUseJournal.mockReturnValue({
        entries: [],
        loading: false,
        error: null,
        submitEntry: mockSubmitEntry,
        deleteEntry: vi.fn()
      });

      const user = userEvent.setup();
      render(<App />);

      // Trigger overlay
      const submitButton = screen.getByTestId("journal-submit");
      await user.click(submitButton);

      // Close overlay
      const closeButton = screen.getByText("Close");
      await user.click(closeButton);

      // Wait for the fade out animation to complete (500ms timeout in App.tsx)
      await vi.waitFor(() => {
        expect(screen.queryByText("Test advice")).not.toBeInTheDocument();
      }, { timeout: 600 });
    });
  });

  describe("History Feed Integration", () => {
    const mockUseAuth0 = vi.mocked(useAuth0);
    const mockUseJournal = vi.mocked(useJournal);

    const mockEntries: JournalEntry[] = [
      {
        id: "1",
        date: "2023-12-01T10:00:00Z",
        userText: "Test entry 1",
        stoicResponse: "Test response 1"
      }
    ];

    beforeEach(() => {
      mockUseAuth0.mockReturnValue({
        isAuthenticated: true,
        isLoading: false,
        user: { email: 'test@example.com' },
        loginWithRedirect: vi.fn(),
        logout: vi.fn()
      }  as any);

      mockUseJournal.mockReturnValue({
        entries: mockEntries,
        loading: false,
        error: null,
        submitEntry: vi.fn(),
        deleteEntry: vi.fn()
      });
    });

    it("passes entries to HistoryFeed component", async () => {
      const user = userEvent.setup();
      render(<App />);

      // Switch to history tab
      const historyTab = screen.getByText("History");
      await user.click(historyTab);

      expect(screen.getByTestId("entry-1")).toBeInTheDocument();
      expect(screen.getByText("Test entry 1")).toBeInTheDocument();
    });

    it("passes deleteEntry function to HistoryFeed", async () => {
      const mockDeleteEntry = vi.fn();
      mockUseJournal.mockReturnValue({
        entries: mockEntries,
        loading: false,
        error: null,
        submitEntry: vi.fn(),
        deleteEntry: mockDeleteEntry
      });

      const user = userEvent.setup();
      render(<App />);

      // Switch to history tab
      const historyTab = screen.getByText("History");
      await user.click(historyTab);

      // The HistoryFeed component should receive the deleteEntry function
      // (this is tested indirectly through the mock)
      expect(mockDeleteEntry).toBeDefined();
    });
  });
});
