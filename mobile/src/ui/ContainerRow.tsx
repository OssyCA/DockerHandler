import { Pressable, StyleSheet, Text, View } from "react-native";
import type { ContainerState, ContainerSummary } from "../api/types";
import { colors } from "./theme";

const stateColors: Record<ContainerState, string> = {
  Running: "#4FD1C5",
  Restarting: colors.accent,
  Paused: colors.accent,
  Dead: "#F0846A",
  Exited: colors.textMuted,
  Created: colors.textMuted,
  Removing: colors.textMuted,
  Unknown: colors.textMuted,
};

export function ContainerRow({
  container,
  onPress,
}: {
  container: ContainerSummary;
  onPress: () => void;
}) {
  const color = stateColors[container.state];
  const running = container.state === "Running";

  return (
    <Pressable style={styles.card} onPress={onPress}>
      <View
        style={[
          styles.dot,
          { borderColor: color, backgroundColor: running ? color : undefined },
        ]}
      />
      <View style={styles.body}>
        <Text style={[styles.name, !running && styles.stopped]}>
          {container.name}
        </Text>
        <Text style={styles.meta} numberOfLines={1}>
          {container.image} · {container.statusText}
        </Text>
      </View>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    flexDirection: "row",
    alignItems: "center",
    gap: 14,
    padding: 16,
    borderRadius: 14,
    borderWidth: 1,
    borderColor: colors.border,
    backgroundColor: colors.card,
  },
  dot: {
    width: 10,
    height: 10,
    borderRadius: 5,
    borderWidth: 1.5,
  },
  body: {
    flex: 1,
    gap: 4,
  },
  name: {
    color: colors.text,
    fontSize: 17,
    fontWeight: "600",
  },
  stopped: {
    color: colors.textMuted,
  },
  meta: {
    color: colors.textMuted,
    fontFamily: "monospace",
    fontSize: 13,
  },
});
