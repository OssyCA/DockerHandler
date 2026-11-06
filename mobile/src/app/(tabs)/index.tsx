import { FlatList, StyleSheet } from "react-native";
import { Redirect, useFocusEffect, useRouter } from "expo-router";
import { useSettings } from "../../settings/store";
import { useState, useCallback } from "react";
import type { ContainerSummary } from "../../api/types";
import { listContainers } from "../../api/client";
import { describe, type Problem } from "../../api/messages";
import { ContainerRow } from "../../ui/ContainerRow";
import { ErrorView, LoadingView } from "../../ui/StatusView";
import { ThemedText } from "../../ui/ThemedText";

export default function Index() {
  const { apiKey, baseUrl } = useSettings();
  const [containers, setContainers] = useState<ContainerSummary[] | null>(null);
  const [problem, setProblem] = useState<Problem | null>(null);
  const [refreshing, setRefreshing] = useState(false);
  const router = useRouter();

  const load = useCallback(async () => {
    try {
      setProblem(null);
      setContainers(await listContainers({ apiKey, baseUrl }));
    } catch (err) {
      setProblem(describe(err));
    }
  }, [apiKey, baseUrl]);

  async function onRefresh() {
    setRefreshing(true);
    await load();
    setRefreshing(false);
  }

  useFocusEffect(
    useCallback(() => {
      if (!apiKey || !baseUrl) return;
      load();
    }, [load, apiKey, baseUrl]),
  );

  if (!apiKey || !baseUrl) {
    return <Redirect href="/settings" />;
  }

  if (problem) {
    return <ErrorView problem={problem} onRetry={load} />;
  }

  if (containers === null) {
    return <LoadingView />;
  }

  const running = containers.filter((c) => c.state === "Running").length;

  return (
    <FlatList
      contentContainerStyle={styles.list}
      refreshing={refreshing}
      onRefresh={onRefresh}
      data={containers}
      ListHeaderComponent={
        <ThemedText muted>
          {running} running · {containers.length - running} stopped
        </ThemedText>
      }
      ListEmptyComponent={
        <ThemedText muted>
          Inga hanterade containers. Lägg till etiketten
          managed-by=docker-controller.
        </ThemedText>
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
});
