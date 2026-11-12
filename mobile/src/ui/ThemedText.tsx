import { StyleSheet, Text, type TextProps } from "react-native";
import { colors } from "./theme";

export function ThemedText({
  muted,
  style,
  ...props
}: TextProps & { muted?: boolean }) {
  return (
    <Text style={[muted ? styles.muted : styles.text, style]} {...props} />
  );
}

const styles = StyleSheet.create({
  text: { color: colors.text },
  muted: { color: colors.textMuted },
});
