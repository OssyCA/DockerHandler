import { useLocalSearchParams } from "expo-router";
import { StyleSheet, Text, View } from "react-native";
import { colors } from "../../ui/theme";
import { useCallback, useEffect, useState } from "react";
import { useSettings } from "../../settings/store";
import { ApiError, getContainer, runAction } from "../../api/client";
import { ActionButton } from "../../ui/ActionButton";

import type { ContainerDetails } from "../../api/types";
import type { ContainerAction } from "../../api/client";

const labels: Record<ContainerAction, { idle: string; busy: string }> = {
  start: { idle: "Start", busy: "Starting…" },
  stop: { idle: "Stop", busy: "Stopping…" },
  restart: { idle: "Restart", busy: "Restarting…" },
};

export default function Container() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const { apiKey, baseUrl } = useSettings();
  const [container, setContainer] = useState<ContainerDetails | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState<ContainerAction | null>(null);

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

  const act = (action: ContainerAction) => async () => {
    setBusy(action);
    try {
      await runAction({ apiKey, baseUrl }, id, action);
      await load();
    } catch (err) {
      if (err instanceof ApiError && err.code === "conflict") {
        await load();
      } else {
        setError(
          err instanceof Error ? err.message : "An unknown error occurred",
        );
      }
    } finally {
      setBusy(null);
    }
  };

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

  const idle = busy === null;
  const running = container.state === "Running";
  const canStart = idle && !running && container.state !== "Restarting";
  const canStop = idle && running;
  const canRestart = idle && running;

  const title = (action: ContainerAction) =>
    busy === action ? labels[action].busy : labels[action].idle;

  return (
    <View style={styles.sheet}>
      <Text style={styles.name}>{container.name}</Text>
      <Text style={styles.mono} numberOfLines={1}>
        {container.image}
      </Text>
      <Text style={styles.muted}>{container.statusText}</Text>
      <View style={styles.actions}>
        <ActionButton
          title={title("restart")}
          onPress={act("restart")}
          disabled={!canRestart}
        />
        <ActionButton
          title={title("stop")}
          onPress={act("stop")}
          disabled={!canStop}
          danger
        />
        <ActionButton
          title={title("start")}
          onPress={act("start")}
          disabled={!canStart}
        />
      </View>
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
  actions: {
    gap: 10,
    marginTop: 16,
  },
  text: { color: colors.text },
  muted: { color: colors.textMuted },
});
