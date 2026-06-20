/**
 * Authenticated home — placeholder for Phase 1.
 * Confirms the round-trip works: Firebase sign-in -> backend /auth/login ->
 * provisioned app user shown here. The "Create" flow lands in Phase 2.
 */

import { useRouter } from "expo-router";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { useAuthStore } from "@/store/useAuthStore";
import { colors, radius, spacing } from "@/theme";

export default function HomeScreen() {
  const router = useRouter();
  const user = useAuthStore((s) => s.user);
  const signOut = useAuthStore((s) => s.signOut);

  return (
    <SafeAreaView style={styles.root}>
      <View style={styles.body}>
        <Text style={styles.kicker}>◢ ◣  F R A M E</Text>
        <Text style={styles.hello}>
          Welcome{user?.display_name ? `,\n${user.display_name}` : ""}
        </Text>
        <Text style={styles.sub}>You're signed in. Let's frame your first scene.</Text>

        <View style={styles.card}>
          <View style={styles.row}>
            <Text style={styles.k}>Plan</Text>
            <Text style={styles.v}>{user?.plan ?? "—"}</Text>
          </View>
          <View style={styles.divider} />
          <View style={styles.row}>
            <Text style={styles.k}>Credits</Text>
            <Text style={styles.v}>{user?.credits ?? 0}</Text>
          </View>
        </View>

        <Pressable style={styles.cta} onPress={() => router.push("/(app)/describe")}>
          <Text style={styles.ctaText}>＋  New Scene</Text>
        </Pressable>
      </View>

      <Pressable style={styles.signout} onPress={signOut}>
        <Text style={styles.signoutText}>Sign out</Text>
      </Pressable>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  root: { flex: 1, backgroundColor: colors.bg },
  body: { flex: 1, paddingHorizontal: spacing.lg, paddingTop: spacing.xxl },
  kicker: {
    color: colors.gold,
    fontSize: 12,
    letterSpacing: 4,
    fontWeight: "600",
    marginBottom: spacing.lg,
  },
  hello: { color: colors.text, fontSize: 38, fontWeight: "600", letterSpacing: -1 },
  sub: { color: colors.muted, fontSize: 15, marginTop: spacing.sm, lineHeight: 22 },
  card: {
    marginTop: spacing.xl,
    backgroundColor: colors.surface,
    borderColor: colors.line,
    borderWidth: 1,
    borderRadius: radius.lg,
    padding: spacing.md,
  },
  row: { flexDirection: "row", justifyContent: "space-between", paddingVertical: 6 },
  divider: { height: 1, backgroundColor: colors.line, marginVertical: 4 },
  k: { color: colors.faint, fontSize: 13, textTransform: "uppercase", letterSpacing: 1 },
  v: { color: colors.text, fontSize: 15, fontWeight: "600", textTransform: "capitalize" },
  cta: {
    marginTop: spacing.xl,
    backgroundColor: colors.gold,
    borderRadius: radius.md,
    paddingVertical: 17,
    alignItems: "center",
  },
  ctaText: { color: colors.onGold, fontSize: 15, fontWeight: "700" },
  signout: { padding: spacing.lg, alignItems: "center" },
  signoutText: { color: colors.faint, fontSize: 14 },
});
