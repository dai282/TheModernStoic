import { useState, useEffect, useCallback } from "react";
import type { JournalEntry, JournalResponse } from "../types/journal";
import { useStoicApi } from "./useStoicApi";
import { useAuth0 } from "@auth0/auth0-react";

export const useJournal = () =>{
    const [entries, setEntries] = useState<JournalEntry[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const { isAuthenticated } = useAuth0();

    const {submitUserEntry, getHistory} = useStoicApi();

    /* useCallback accepts as a first parameter a function and returns a memoized version of it 
    (in terms of its memory location, not the computation done inside). 
    Meaning that the returned function doesn't get recreated on a new memory reference every time the component re-renders, 
    while a normal function inside a component does.
    The returned function gets recreated on a new memory reference if one of the variables inside 
    useCallback's dependency array (its second parameter) changes.*/

    //TLDR: fetches history from backend and sets entries state
    const fetchHistory = useCallback(async () =>{
        try { 
            const data = await getHistory();
            setEntries(data);
        }
        catch(err){
            console.error(err);
            setError("Could not load history");
        }
    }, []);

    //fetches history on mount
    useEffect(() => {
        if (isAuthenticated){
            fetchHistory();
        }
    }, [fetchHistory, isAuthenticated]);

    //submit new entry
    const submitEntry = async (text: string): Promise<JournalResponse | null> =>{
        setLoading(true);
        setError(null);

        try{
            const response = await submitUserEntry(text);
            // Optimistic update or refetch? Let's refetch to be safe for now.
            await fetchHistory(); 
            return response;
        } catch(err){
            setError("Marcus Aurelius is silent (API Error)");
            return null;
        } finally{
            setLoading(false);
        }
    }

    //don't need to fetch history as it's done on mount
    return {entries, loading, error, submitEntry};
}