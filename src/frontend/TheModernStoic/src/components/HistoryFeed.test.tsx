import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from '@testing-library/user-event';
import HistoryFeed from "./HistoryFeed";
import type { JournalEntry } from "../types/journal";

const mockEntries: JournalEntry[] = [
  {
    id: "1",
    date: "2023-12-01T10:00:00Z",
    userText: "I'm feeling anxious about work",
    stoicResponse: "Remember that we control our judgments, not external events."
  },
  {
    id: "2",
    date: "2023-12-02T15:30:00Z",
    userText: "Had a disagreement with a friend",
    stoicResponse: "Focus on what you can control - your own responses and actions."
  }
];

describe("HistoryFeed Component", () => {
  it("renders empty state when no entries", () => {
    render(<HistoryFeed entries={[]} deleteEntry={vi.fn()} />);

    expect(screen.getByText("No entries yet. Begin your journey.")).toBeInTheDocument();
  });

  it("renders list of entries with correct content", () => {
    render(<HistoryFeed entries={mockEntries} deleteEntry={vi.fn()} />);

    // Check user texts are displayed
    expect(screen.getByText("I'm feeling anxious about work")).toBeInTheDocument();
    expect(screen.getByText("Had a disagreement with a friend")).toBeInTheDocument();

    // Check stoic responses are displayed
    expect(screen.getByText("Remember that we control our judgments, not external events.")).toBeInTheDocument();
    expect(screen.getByText("Focus on what you can control - your own responses and actions.")).toBeInTheDocument();

    // Check dates are formatted
    expect(screen.getByText("12/1/2023")).toBeInTheDocument();
    expect(screen.getByText("12/2/2023")).toBeInTheDocument();
  });

  it("renders delete buttons for each entry", () => {
    render(<HistoryFeed entries={mockEntries} deleteEntry={vi.fn()} />);

    const deleteButtons = screen.getAllByRole('button', { name: /delete/i });
    expect(deleteButtons).toHaveLength(2);
  });

  it("calls deleteEntry when delete button is clicked", async () => {
    const mockDeleteEntry = vi.fn().mockResolvedValue(undefined);
    const user = userEvent.setup();

    render(<HistoryFeed entries={mockEntries} deleteEntry={mockDeleteEntry} />);

    const deleteButtons = screen.getAllByRole('button', { name: /delete/i });
    await user.click(deleteButtons[0]);

    expect(mockDeleteEntry).toHaveBeenCalledWith("1");
    expect(mockDeleteEntry).toHaveBeenCalledTimes(1);
  });

  it("shows loading state on delete button while deleting", async () => {
    const mockDeleteEntry = vi.fn().mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)));
    const user = userEvent.setup();

    render(<HistoryFeed entries={mockEntries} deleteEntry={mockDeleteEntry} />);

    const deleteButtons = screen.getAllByRole('button', { name: /delete/i });
    await user.click(deleteButtons[0]);

    // Button should show "Deleting..." while loading
    expect(screen.getByRole('button', { name: /deleting/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /deleting/i })).toBeDisabled();
  });

  it("removes loading state after delete completes", async () => {
    const mockDeleteEntry = vi.fn().mockResolvedValue(undefined);
    const user = userEvent.setup();

    render(<HistoryFeed entries={mockEntries} deleteEntry={mockDeleteEntry} />);

    const deleteButtons = screen.getAllByRole('button', { name: /delete/i });
    await user.click(deleteButtons[0]);

    // Wait for the async operation to complete
    await vi.waitFor(() => {
      expect(screen.getAllByRole('button', { name: /delete/i })).toHaveLength(2);
    });
  });

  it("handles multiple simultaneous deletes", async () => {
    const mockDeleteEntry = vi.fn().mockResolvedValue(undefined);
    const user = userEvent.setup();

    render(<HistoryFeed entries={mockEntries} deleteEntry={mockDeleteEntry} />);

    const deleteButtons = screen.getAllByRole('button', { name: /delete/i });

    // Click both delete buttons quickly
    await user.click(deleteButtons[0]);
    await user.click(deleteButtons[1]);

    expect(mockDeleteEntry).toHaveBeenCalledWith("1");
    expect(mockDeleteEntry).toHaveBeenCalledWith("2");
  });
});
