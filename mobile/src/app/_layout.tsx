import { Stack } from "expo-router";
import { SettingsProvider } from "../settings/store";

export default function RootLayout() {
  return (
    <SettingsProvider>
      <Stack>
        <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
      </Stack>
    </SettingsProvider>
  );
}
