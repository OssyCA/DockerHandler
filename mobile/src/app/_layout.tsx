import { Stack } from "expo-router";
import { SettingsProvider } from "../settings/store";

export default function RootLayout() {
  return (
    <SettingsProvider>
      <Stack>
        <Stack.Screen name="index" options={{ title: "DockerHandler" }} />
      </Stack>
    </SettingsProvider>
  );
}
