import { SafeAreaView, ScrollView, Text, View } from 'react-native';

const sections = ['Home', 'Trips', 'Documents', 'Notifications', 'Profile'];

export default function App() {
  return (
    <SafeAreaView style={{ flex: 1, backgroundColor: '#eff6ff' }}>
      <ScrollView contentContainerStyle={{ padding: 24, gap: 16 }}>
        <Text style={{ fontSize: 28, fontWeight: '700', color: '#0f172a' }}>Voyara Mobile</Text>
        <Text style={{ fontSize: 16, color: '#334155' }}>
          Starter Expo scaffold for the traveler companion app.
        </Text>

        {sections.map((section) => (
          <View
            key={section}
            style={{
              backgroundColor: '#ffffff',
              borderRadius: 16,
              padding: 16,
              borderWidth: 1,
              borderColor: '#bfdbfe',
            }}
          >
            <Text style={{ fontSize: 18, fontWeight: '600', color: '#0f172a' }}>{section}</Text>
            <Text style={{ marginTop: 6, color: '#475569' }}>
              Screen group placeholder ready for traveler-facing implementation.
            </Text>
          </View>
        ))}
      </ScrollView>
    </SafeAreaView>
  );
}
