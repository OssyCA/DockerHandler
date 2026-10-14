import { useRouter } from "expo-router";
import { useState } from "react";
import { Button, Text, TextInput, View } from "react-native";

import { useSettings } from "../../settings/store";

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
    <View>
      <Text>API-nyckel</Text>
      <TextInput
        value={newApiKey}
        onChangeText={setNewApiKey}
        placeholder="64 tecken från serverns Auth__Keys"
        autoCapitalize="none"
        autoCorrect={false}
      />

      <Text>Serveradress</Text>
      <TextInput
        value={newBaseUrl}
        onChangeText={setNewBaseUrl}
        placeholder="https://example.com"
        autoCapitalize="none"
        autoCorrect={false}
      />

      <Button title="Spara" onPress={onSave} />
    </View>
  );
}
