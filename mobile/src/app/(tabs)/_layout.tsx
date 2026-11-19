import { Tabs } from "expo-router";
import { colors } from "../../ui/theme";
import { Settings, Container, MonitorActivity } from "lucide-react-native";

export default function LayoutTabs() {
  return (
    <Tabs
      screenOptions={{
        sceneStyle: { backgroundColor: colors.background },
        headerStyle: { backgroundColor: colors.background },
        headerTintColor: colors.text,
        headerShadowVisible: false,
        tabBarStyle: {
          backgroundColor: colors.card,
          borderTopColor: colors.border,
        },
        tabBarActiveTintColor: colors.accent,
        tabBarInactiveTintColor: colors.textMuted,
      }}
    >
      <Tabs.Screen
        name="index"
        options={{
          title: "Containers",
          tabBarIcon: ({ color, size }) => (
            <Container color={color} size={size} />
          ),
        }}
      />
      <Tabs.Screen
        name="settings"
        options={{
          title: "Settings",
          tabBarIcon: ({ color, size }) => (
            <Settings color={color} size={size} />
          ),
        }}
      />
      <Tabs.Screen
        name="metrics"
        options={{
          title: "Metrics",
          tabBarIcon: ({ color, size }) => (
            <MonitorActivity color={color} size={size} />
          ),
        }}
      />
    </Tabs>
  );
}
