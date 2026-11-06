import { useRouter } from "expo-router";
import { StyleSheet, View } from "react-native";
import type { Problem } from "../api/messages";
import { ActionButton } from "./ActionButton";
import { ThemedText } from "./ThemedText";

export function LoadingView() {
  return (
    <View style={styles.view}>
      <ThemedText muted>Loading...</ThemedText>
    </View>
  );
}

export function ErrorView({
  problem,
  onRetry,
}: {
  problem: Problem;
  onRetry: () => void;
}) {
  const router = useRouter();

  return (
    <View style={styles.view}>
      <ThemedText>{problem.message}</ThemedText>
      {problem.remedy === "retry" && (
        <ActionButton title="Försök igen" onPress={onRetry} />
      )}
      {problem.remedy === "settings" && (
        <ActionButton
          title="Till inställningar"
          onPress={() => router.navigate("/settings")}
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  view: {
    padding: 24,
    gap: 16,
  },
});
