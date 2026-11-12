import { useLocalSearchParams } from "expo-router";
import { StyleSheet, View } from "react-native";
import { useCallback, useEffect, useState } from "react";
import { useSettings } from "../../settings/store";
import { ApiError, getContainer, runAction } from "../../api/client";
import { describe, type Problem } from "../../api/messages";
import { ActionButton } from "../../ui/ActionButton";
import { ErrorView, LoadingView } from "../../ui/StatusView";
import { ThemedText } from "../../ui/ThemedText";

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
  const [problem, setProblem] = useState<Problem | null>(null);
  const [busy, setBusy] = useState<ContainerAction | null>(null);

  const load = useCallback(async () => {
    try {
      setProblem(null);
      setContainer(await getContainer({ apiKey, baseUrl }, id));
    } catch (err) {
      setProblem(describe(err));
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
        setProblem(describe(err));
      }
    } finally {
      setBusy(null);
    }
  };

  useEffect(() => {
    load();
  }, [load]);

  if (problem) {
    return <ErrorView problem={problem} onRetry={load} />;
  }

  if (!container) {
    return <LoadingView />;
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
      <ThemedText style={styles.name}>{container.name}</ThemedText>
      <ThemedText muted style={styles.mono} numberOfLines={1}>
        {container.image}
      </ThemedText>
      <ThemedText muted>{container.statusText}</ThemedText>
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
    fontSize: 24,
    fontWeight: "600",
  },
  mono: {
    fontFamily: "monospace",
    fontSize: 13,
  },
  actions: {
    gap: 10,
    marginTop: 16,
  },
});
