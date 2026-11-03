import { Pressable, StyleSheet, Text } from "react-native";
import { colors } from "./theme";

export function ActionButton({
  onPress,
  disabled,
  title,
  danger,
}: {
  onPress: () => void;
  disabled?: boolean;
  title: string;
  danger?: boolean;
}) {
  return (
    <Pressable
      onPress={onPress}
      disabled={disabled}
      style={[
        styles.button,
        danger && styles.dangerButton,
        disabled && styles.disabled,
      ]}
    >
      <Text style={[styles.title, danger && styles.dangerTitle]}>{title}</Text>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  button: {
    paddingVertical: 16,
    paddingHorizontal: 20,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: colors.border,
    backgroundColor: colors.card,
  },
  dangerButton: { backgroundColor: colors.dangerSurface },
  disabled: { opacity: 0.4 },
  title: {
    color: colors.text,
    fontSize: 16,
    fontWeight: "600",
  },
  dangerTitle: { color: colors.danger },
});
