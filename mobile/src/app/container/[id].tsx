import { useLocalSearchParams } from "expo-router";
import { StyleSheet, Text, View } from "react-native";
import { colors } from "../../ui/theme";
import { useCallback, useEffect, useState } from "react";
import { useSettings } from "../../settings/store";
import type { ContainerDetails } from "../../api/types";
import { getContainer } from "../../api/client";

export default function Container() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const { apiKey, baseUrl } = useSettings();
  const [container, setContainer] = useState<ContainerDetails | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setError(null);
      setContainer(await getContainer({ apiKey, baseUrl }, id));
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "An unknown error occurred",
      );
    }
  }, [apiKey, baseUrl, id]);

  useEffect(() => {
    load();
  }, [load]);

  if (error) {
    return (
      <View style={styles.sheet}>
        <Text style={styles.text}>Error: {error}</Text>
      </View>
    );
  }

  if (!container) {
    return (
      <View style={styles.sheet}>
        <Text style={styles.muted}>Loading...</Text>
      </View>
    );
  }

  return (
    <View style={styles.sheet}>
      <Text style={styles.name}>{container.name}</Text>
      <Text style={styles.mono} numberOfLines={1}>
        {container.image}
      </Text>
      <Text style={styles.muted}>{container.statusText}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  sheet: {
    padding: 24,
    gap: 8,
  },
  name: {
    color: colors.text,
    fontSize: 24,
    fontWeight: "600",
  },
  mono: {
    color: colors.textMuted,
    fontFamily: "monospace",
    fontSize: 13,
  },
  text: { color: colors.text },
  muted: { color: colors.textMuted },
});
