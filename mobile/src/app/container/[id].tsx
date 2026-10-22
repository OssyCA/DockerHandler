import { useLocalSearchParams } from "expo-router";
import { Text, View } from "react-native";
import { colors } from "../../ui/theme";

export default function Container() {
  const { id } = useLocalSearchParams<{ id: string }>();

  return (
    <View style={{ padding: 20 }}>
      <Text style={{ color: colors.text, fontSize: 16 }}>
        Container ID: {id}
      </Text>
    </View>
  );
}
