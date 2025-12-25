
//matches journal entry DTO from backend
export interface JournalEntry{
    id: string;
    date: string; //ISO string
    userText: string;
    stoicResponse: string;
}

//matches journal response DTO from backend
export interface JournalResponse{
    userText: string;
    stoicAdvice: string;
    citedQuotes: string[];
}