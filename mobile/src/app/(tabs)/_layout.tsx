import { Tabs } from "expo-router";

export default function LayoutTabs() {
  return (
    <Tabs>
      <Tabs.Screen name="index" options={{ title: "Containers" }} />
      <Tabs.Screen name="settings" options={{ title: "Settings" }} />
    </Tabs>
  );
}
