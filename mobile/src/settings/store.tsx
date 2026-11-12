import * as SecureStore from "expo-secure-store";
import {
  createContext,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from "react";
import { Text } from "react-native";
import type { ApiConfig } from "../api/client";

const apiKeyEntry = "apiKey";
const baseUrlEntry = "baseUrl";
const defaultBaseUrl = process.env.EXPO_PUBLIC_API_URL ?? "";

type SettingsStore = ApiConfig & {
  save: (next: ApiConfig) => Promise<void>;
};

const SettingsContext = createContext<SettingsStore | null>(null);

export function SettingsProvider({ children }: { children: ReactNode }) {
  const [config, setConfig] = useState<ApiConfig | null>(null);

  useEffect(() => {
    async function loadConfig() {
      const apiKey = await SecureStore.getItemAsync(apiKeyEntry);
      const baseUrl = await SecureStore.getItemAsync(baseUrlEntry);

      setConfig({
        apiKey: apiKey ?? "",
        baseUrl: baseUrl ?? defaultBaseUrl,
      });
    }

    loadConfig();
  }, []);

  const save = async (next: ApiConfig) => {
    await SecureStore.setItemAsync(apiKeyEntry, next.apiKey);
    await SecureStore.setItemAsync(baseUrlEntry, next.baseUrl);
    setConfig(next);
  };

  if (config === null) {
    return <Text>Loading...</Text>;
  }

  return (
    <SettingsContext.Provider value={{ ...config, save }}>
      {children}
    </SettingsContext.Provider>
  );
}

export function useSettings(): SettingsStore {
  const store = useContext(SettingsContext);
  if (!store) {
    throw new Error("useSettings must be used within a SettingsProvider");
  }
  return store;
}
