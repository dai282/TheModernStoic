import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from '@testing-library/user-event';
import JournalInputCard from "./JournalInputCard";
import type { JournalResponse } from "../types/journal";

const mockJournalResponse: JournalResponse = {
  userText: "Test input",
  stoicAdvice: "Test stoic advice",
  citedQuotes: ["Quote 1", "Quote 2"]
};

describe("JournalInputCard Component", () => {
  const defaultProps = {
    onSubmit: vi.fn(),
    loading: false,
    error: null as string | null,
    onResponse: vi.fn()
  };

  it("renders textarea and button", () => {
    render(<JournalInputCard {...defaultProps} />);

    expect(screen.getByPlaceholderText("Write your thoughts here...")).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /reflect/i })).toBeInTheDocument();
    expect(screen.getByText("What troubles you, friend?")).toBeInTheDocument();
  });

  it("updates textarea value when user types", async () => {
    const user = userEvent.setup();
    render(<JournalInputCard {...defaultProps} />);

    const textarea = screen.getByPlaceholderText("Write your thoughts here...");
    await user.type(textarea, "I'm feeling stressed");

    expect(textarea).toHaveValue("I'm feeling stressed");
  });

  it("calls onSubmit with input text when Reflect button is clicked", async () => {
    const mockOnSubmit = vi.fn().mockResolvedValue(mockJournalResponse);
    const user = userEvent.setup();

    render(<JournalInputCard {...defaultProps} onSubmit={mockOnSubmit} />);

    const textarea = screen.getByPlaceholderText("Write your thoughts here...");
    const button = screen.getByRole('button', { name: /reflect/i });

    await user.type(textarea, "Test input");
    await user.click(button);

    expect(mockOnSubmit).toHaveBeenCalledWith("Test input");
    expect(mockOnSubmit).toHaveBeenCalledTimes(1);
  });

  it("clears input after successful submit", async () => {
    const mockOnSubmit = vi.fn().mockResolvedValue(mockJournalResponse);
    const user = userEvent.setup();

    render(<JournalInputCard {...defaultProps} onSubmit={mockOnSubmit} />);

    const textarea = screen.getByPlaceholderText("Write your thoughts here...");
    const button = screen.getByRole('button', { name: /reflect/i });

    await user.type(textarea, "Test input");
    await user.click(button);

    expect(textarea).toHaveValue("");
  });

  it("calls onResponse with the journal response after submit", async () => {
    const mockOnSubmit = vi.fn().mockResolvedValue(mockJournalResponse);
    const mockOnResponse = vi.fn();
    const user = userEvent.setup();

    render(<JournalInputCard {...defaultProps} onSubmit={mockOnSubmit} onResponse={mockOnResponse} />);

    const textarea = screen.getByPlaceholderText("Write your thoughts here...");
    const button = screen.getByRole('button', { name: /reflect/i });

    await user.type(textarea, "Test input");
    await user.click(button);

    expect(mockOnResponse).toHaveBeenCalledWith(mockJournalResponse);
  });

  it("does not call onSubmit when button clicked with empty input", async () => {
    const mockOnSubmit = vi.fn();
    const user = userEvent.setup();

    render(<JournalInputCard {...defaultProps} onSubmit={mockOnSubmit} />);

    const button = screen.getByRole('button', { name: /reflect/i });
    await user.click(button);

    expect(mockOnSubmit).not.toHaveBeenCalled();
  });

  it("shows loading state and disables controls", () => {
    render(<JournalInputCard {...defaultProps} loading={true} />);

    const textarea = screen.getByPlaceholderText("Write your thoughts here...");
    const button = screen.getByRole('button', { name: /consulting/i });

    expect(textarea).toBeDisabled();
    expect(button).toBeDisabled();
    expect(button).toHaveTextContent("Consulting...");
    expect(screen.getByText("The Stoic is contemplating...")).toBeInTheDocument();
  });

  it("shows error message when error prop is provided", () => {
    const errorMessage = "Network error occurred";
    render(<JournalInputCard {...defaultProps} error={errorMessage} />);

    expect(screen.getByText(errorMessage)).toBeInTheDocument();
  });

  it("does not show error message when error is null", () => {
    render(<JournalInputCard {...defaultProps} error={null} />);

    // Should not find any error text
    expect(screen.queryByText(/error/i)).not.toBeInTheDocument();
  });

  it("shows default status text when not loading", () => {
    render(<JournalInputCard {...defaultProps} />);

    expect(screen.getByText("Space to reflect.")).toBeInTheDocument();
  });

  it("handles onSubmit returning null response", async () => {
    const mockOnSubmit = vi.fn().mockResolvedValue(null);
    const mockOnResponse = vi.fn();
    const user = userEvent.setup();

    render(<JournalInputCard {...defaultProps} onSubmit={mockOnSubmit} onResponse={mockOnResponse} />);

    const textarea = screen.getByPlaceholderText("Write your thoughts here...");
    const button = screen.getByRole('button', { name: /reflect/i });

    await user.type(textarea, "Test input");
    await user.click(button);

    expect(mockOnResponse).toHaveBeenCalledWith(null);
  });
});
