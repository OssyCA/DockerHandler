import { Text, View } from "react-native";
import { Link, Redirect } from "expo-router";
import { useSettings } from "../settings/store";
import { useState, useEffect, useCallback } from "react";
import type { ContainerSummary, ContainerState } from "../api/types";
import { listContainers } from "../api/client";
import { FlatList } from "react-native";

const stateColors: Record<ContainerState, string> = {
  Running: "turquoise",
  Restarting: "orange",
  Exited: "red",
  Paused: "yellow",
  Created: "gray",
  Removing: "lightgray",
  Dead: "darkgray",
  Unknown: "black",
};

export default function Index() {
  const { apiKey, baseUrl } = useSettings();
  const [containers, setContainers] = useState<ContainerSummary[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [refreshing, setRefreshing] = useState(false);

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
      <View>
        <Text>Error: {error}</Text>
        <Link href="/settings">Settings</Link>
      </View>
    );
  }

  if (containers === null) {
    return <Text>Loading...</Text>;
  }

  return (
    <View style={{ flex: 1 }}>
      <Text>Docker manager</Text>
      <Link href="/settings">Settings</Link>

      <FlatList
        refreshing={refreshing}
        onRefresh={onRefresh}
        data={containers}
        keyExtractor={(container) => container.id}
        renderItem={({ item }) => (
          <View style={{ flexDirection: "row", alignItems: "center" }}>
            <View
              style={{
                backgroundColor: stateColors[item.state],
                width: 10,
                height: 10,
                borderRadius: 5,
                margin: 5,
              }}
            />
            <View>
              <Text>{item.name}</Text>
              <Text>{item.statusText}</Text>
              <Text>{item.image}</Text>
            </View>
          </View>
        )}
      />
    </View>
  );
}
