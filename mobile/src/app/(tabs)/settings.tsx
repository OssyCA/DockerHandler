import { useRouter } from "expo-router";
import { useState } from "react";
import { Pressable, StyleSheet, Text, TextInput, View } from "react-native";

import { useSettings } from "../../settings/store";
import { colors } from "../../ui/theme";

export default function Settings() {
  const { apiKey, baseUrl, save } = useSettings();
  const [newApiKey, setNewApiKey] = useState(apiKey);
  const [newBaseUrl, setNewBaseUrl] = useState(baseUrl);
  const router = useRouter();

  async function onSave() {
    await save({ apiKey: newApiKey, baseUrl: newBaseUrl });
    router.navigate("/");
  }

  return (
    <View style={styles.screen}>
      <View style={styles.field}>
        <Text style={styles.label}>SERVERADRESS</Text>
        <TextInput
          style={styles.input}
          placeholderTextColor={colors.textMuted}
          value={newBaseUrl}
          onChangeText={setNewBaseUrl}
          placeholder="https://example.com"
          autoCapitalize="none"
          autoCorrect={false}
        />
      </View>

      <View style={styles.field}>
        <Text style={styles.label}>API-NYCKEL</Text>
        <TextInput
          style={styles.input}
          placeholderTextColor={colors.textMuted}
          value={newApiKey}
          onChangeText={setNewApiKey}
          placeholder="64 tecken från serverns Auth__Keys"
          autoCapitalize="none"
          autoCorrect={false}
          secureTextEntry
        />
      </View>

      <Pressable
        style={({ pressed }) => [styles.button, pressed && styles.pressed]}
        onPress={onSave}
      >
        <Text style={styles.buttonText}>Spara</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: {
    padding: 24,
    gap: 20,
  },
  field: {
    gap: 8,
  },
  label: {
    color: colors.textMuted,
    fontSize: 12,
    fontWeight: "600",
    letterSpacing: 1.5,
  },
  input: {
    color: colors.text,
    backgroundColor: colors.card,
    borderColor: colors.border,
    borderWidth: 1,
    borderRadius: 12,
    paddingHorizontal: 16,
    paddingVertical: 14,
    fontFamily: "monospace",
    fontSize: 15,
  },
  button: {
    backgroundColor: colors.accent,
    borderRadius: 14,
    paddingVertical: 16,
    alignItems: "center",
    marginTop: 8,
  },
  pressed: {
    opacity: 0.8,
  },
  buttonText: {
    color: colors.background,
    fontSize: 17,
    fontWeight: "600",
  },
});
