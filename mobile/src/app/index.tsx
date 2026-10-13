import { Text, View } from "react-native";
import { Link } from "expo-router";
import { useSettings } from "../settings/store";
import { useState, useEffect } from "react";
import type { ContainerSummary } from "../api/types";
import { listContainers } from "../api/client";
import { FlatList } from "react-native";

export default function Index() {
  const { apiKey, baseUrl } = useSettings();
  const [containers, setContainers] = useState<ContainerSummary[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchContainers() {
      try {
        setContainers(await listContainers({ apiKey, baseUrl }));
        error && setError(null); // Clear error if fetch is successful
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "An unknown error occurred",
        );
      }
    }

    fetchContainers();
  }, [apiKey, baseUrl]);

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
    <View>
      <Text>Docker manager</Text>
      <Link href="/settings">Settings</Link>

      <FlatList
        data={containers}
        keyExtractor={(container) => container.id}
        renderItem={({ item }) => <Text>{item.name}</Text>}
      />
    </View>
  );
}
