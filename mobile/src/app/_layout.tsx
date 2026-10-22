import { Stack } from "expo-router";
import { SettingsProvider } from "../settings/store";
import { colors } from "../ui/theme";

export default function RootLayout() {
  return (
    <SettingsProvider>
      <Stack>
        <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
        <Stack.Screen
          name="container/[id]"
          options={{
            presentation: "formSheet",
            sheetAllowedDetents: "fitToContents",
            sheetCornerRadius: 24,
            headerShown: false,
            contentStyle: { backgroundColor: colors.card },
          }}
        />
      </Stack>
    </SettingsProvider>
  );
}
