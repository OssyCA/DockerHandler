import { FlatList, StyleSheet, Text, View } from "react-native";
import { Link, Redirect, useRouter } from "expo-router";
import { useSettings } from "../../settings/store";
import { useState, useEffect, useCallback } from "react";
import type { ContainerSummary } from "../../api/types";
import { listContainers } from "../../api/client";
import { ContainerRow } from "../../ui/ContainerRow";
import { colors } from "../../ui/theme";

export default function Index() {
  const { apiKey, baseUrl } = useSettings();
  const [containers, setContainers] = useState<ContainerSummary[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [refreshing, setRefreshing] = useState(false);
  const router = useRouter();

  const load = useCallback(async () => {
    try {
      setError(null);
      setContainers(await listContainers({ apiKey, baseUrl }));
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "An unknown error occurred",
      );
    }
  }, [apiKey, baseUrl]);

  async function onRefresh() {
    setRefreshing(true);
    await load();
    setRefreshing(false);
  }

  useEffect(() => {
    if (!apiKey || !baseUrl) return;
    load();
  }, [load]);

  if (!apiKey || !baseUrl) {
    return <Redirect href="/settings" />;
  }

  if (error) {
    return (
      <View style={styles.message}>
        <Text style={styles.text}>Error: {error}</Text>
        <Link href="/settings" style={styles.link}>
          Settings
        </Link>
      </View>
    );
  }

  if (containers === null) {
    return (
      <View style={styles.message}>
        <Text style={styles.muted}>Loading...</Text>
      </View>
    );
  }

  const running = containers.filter((c) => c.state === "Running").length;

  return (
    <FlatList
      contentContainerStyle={styles.list}
      refreshing={refreshing}
      onRefresh={onRefresh}
      data={containers}
      ListHeaderComponent={
        <Text style={styles.muted}>
          {running} running · {containers.length - running} stopped
        </Text>
      }
      ListEmptyComponent={
        <Text style={styles.muted}>
          Inga hanterade containers. Lägg till etiketten
          managed-by=docker-controller.
        </Text>
      }
      keyExtractor={(container) => container.id}
      renderItem={({ item }) => (
        <ContainerRow
          container={item}
          onPress={() => router.push(`/container/${item.id}`)}
        />
      )}
    />
  );
}

const styles = StyleSheet.create({
  list: {
    padding: 16,
    gap: 10,
  },
  message: {
    padding: 16,
    gap: 12,
  },
  text: { color: colors.text },
  muted: { color: colors.textMuted },
  link: { color: colors.accent },
});
