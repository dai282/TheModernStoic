import { useAuth0 } from "@auth0/auth0-react";
import axios from "axios";
import { useMemo } from "react";
import type { JournalEntry, JournalResponse } from "../types/journal";

const API_BASE = import.meta.env.VITE_API_BASE_URL;

export const useStoicApi = () => {
  const { getAccessTokenSilently } = useAuth0();

  //create an axios instance that automatically adds the token
  //using useMemo so we don't recreate the client on every render
  const client = useMemo(() => {
    const instance = axios.create({
      baseURL: API_BASE,
    });

    instance.interceptors.request.use(async (config) => {
      try {
        const token = await getAccessTokenSilently();
        config.headers.Authorization = `Bearer ${token}`;
      } catch (error) {
        console.error("Error getting auth token:", error);
      }
      return config;
    });

    return instance;
  }, [getAccessTokenSilently]);

  //define methods using the authenticated client
  const submitUserEntry = async (
    userText: string
  ): Promise<JournalResponse> => {
    const response = await client.post<JournalResponse>(
      `/journal`,
      { text: userText } // This is the request body (will be JSON.stringified)
    );
    return response.data;
  };

  const getHistory = async (): Promise<JournalEntry[]> => {
    const response = await client.get<JournalEntry[]>("/journal");
    return response.data;
  };

  const deleteUserEntry = async (entryId: string): Promise<void> =>{
    await client.delete<void>(`/journal/${entryId}`);
  }

  return {submitUserEntry, getHistory, deleteUserEntry};
};
