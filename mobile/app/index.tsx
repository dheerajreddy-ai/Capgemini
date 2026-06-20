/**
 * Sign-In screen — the public entry point.
 * Visual language matches the approved prototype (screen 01): cinematic hero
 * gradient, letterbox bar, editorial headline, glass Google/Apple buttons.
 */

import { useState } from "react";
import {
  ActivityIndicator,
  Pressable,
  StyleSheet,
  Text,
  View,
} from "react-native";
import { LinearGradient } from "expo-linear-gradient";
import { SafeAreaView } from "react-native-safe-area-context";

import { useAuthStore } from "@/store/useAuthStore";
import { colors, radius, spacing } from "@/theme";

export default function SignInScreen() {
  const signIn = useAuthStore((s) => s.signIn);
  const error = useAuthStore((s) => s.error);
  const [busy, setBusy] = useState(false);

  const handleGoogle = async () => {
    setBusy(true);
    try {
      await signIn();
    } finally {
      setBusy(false);
    }
  };

  return (
    <View style={styles.root}>
      {/* Cinematic hero backdrop */}
      <LinearGradient
        colors={["#3a2f4d", "#7a4b54", "#c98a4e", "#e8b65a"]}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={StyleSheet.absoluteFill}
      />
      <LinearGradient
        colors={["transparent", "rgba(8,8,10,0.65)", colors.bg]}
        locations={[0.3, 0.62, 0.96]}
        style={StyleSheet.absoluteFill}
      />
      <View style={styles.letterbar} />

      <SafeAreaView style={styles.safe}>
        <View style={styles.content}>
          <Text style={styles.mark}>◢ ◣  F R A M E  ◢ ◣</Text>
          <Text style={styles.h1}>
            Direct your{"\n"}
            <Text style={styles.h1Italic}>first scene</Text>
          </Text>
          <Text style={styles.sub}>
            No film school. No jargon. Just your imagination — shot like a
            professional.
          </Text>

          <Pressable
            style={({ pressed }) => [styles.gbtn, pressed && styles.pressed]}
            onPress={handleGoogle}
            disabled={busy}
          >
            {busy ? (
              <ActivityIndicator color={colors.text} />
            ) : (
              <>
                <View style={styles.gIcon}>
                  <Text style={styles.gIconText}>G</Text>
                </View>
                <Text style={styles.gbtnText}>Continue with Google</Text>
              </>
            )}
          </Pressable>

          {error ? <Text style={styles.error}>{error}</Text> : null}

          <Text style={styles.terms}>
            By continuing you agree to our Terms &amp; Privacy Policy
          </Text>
        </View>
      </SafeAreaView>
    </View>
  );
}

const styles = StyleSheet.create({
  root: { flex: 1, backgroundColor: colors.bg },
  letterbar: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 0,
    height: 46,
    backgroundColor: "#000",
  },
  safe: { flex: 1, justifyContent: "flex-end" },
  content: { paddingHorizontal: spacing.xl, paddingBottom: spacing.xxl },
  mark: {
    color: colors.goldSoft,
    fontSize: 11,
    letterSpacing: 4,
    fontWeight: "600",
    textAlign: "center",
    marginBottom: spacing.md,
  },
  h1: {
    color: colors.text,
    fontSize: 52,
    lineHeight: 52,
    textAlign: "center",
    fontWeight: "600",
    letterSpacing: -1,
  },
  h1Italic: { color: colors.goldSoft, fontStyle: "italic", fontWeight: "400" },
  sub: {
    color: "rgba(244,243,241,0.72)",
    fontSize: 14.5,
    lineHeight: 23,
    textAlign: "center",
    marginTop: spacing.md,
    marginBottom: spacing.xl,
    maxWidth: 290,
    alignSelf: "center",
  },
  gbtn: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    gap: 11,
    paddingVertical: 16,
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.line2,
    backgroundColor: "rgba(255,255,255,0.06)",
  },
  pressed: { opacity: 0.7 },
  gIcon: {
    width: 19,
    height: 19,
    borderRadius: 10,
    backgroundColor: "#fff",
    alignItems: "center",
    justifyContent: "center",
  },
  gIconText: { color: "#4285F4", fontWeight: "800", fontSize: 12 },
  gbtnText: { color: colors.text, fontWeight: "600", fontSize: 15 },
  error: {
    color: "#E89B9B",
    fontSize: 13,
    textAlign: "center",
    marginTop: spacing.md,
  },
  terms: {
    color: colors.faint,
    fontSize: 11,
    textAlign: "center",
    marginTop: spacing.md,
  },
});
