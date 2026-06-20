/**
 * Describe Scene screen — user fills in scene details and triggers generation.
 * Matches the "Describe Your Scene" mockup: dark card, gold accents, style picker.
 */

import { useRouter } from "expo-router";
import React, { useState } from "react";
import {
  ActivityIndicator,
  Alert,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from "react-native";
import { StylePicker } from "../../src/components/StylePicker";
import { storyboardApi } from "../../src/services/storyboard";
import { useAuthStore } from "../../src/store/useAuthStore";
import { colors, radius, spacing } from "../../src/theme";

export default function DescribeScreen() {
  const router = useRouter();
  const user = useAuthStore((s) => s.user);

  const [sceneDescription, setSceneDescription] = useState("");
  const [sceneStyle, setSceneStyle] = useState("Cinematic");
  const [locationDesc, setLocationDesc] = useState("");
  const [characterDesc, setCharacterDesc] = useState("");
  const [loading, setLoading] = useState(false);

  const charCount = sceneDescription.length;
  const isReady = charCount >= 20 && !loading;

  async function handleGenerate() {
    if (!isReady) return;
    setLoading(true);
    try {
      const result = await storyboardApi.generate({
        scene_description: sceneDescription,
        scene_style: sceneStyle,
        location_desc: locationDesc || null,
        character_desc: characterDesc || null,
      });
      router.push(`/(app)/storyboard/${result.id}`);
    } catch (err: any) {
      Alert.alert(
        "Generation failed",
        err?.message ?? "Something went wrong. Please try again.",
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <KeyboardAvoidingView
      style={styles.root}
      behavior={Platform.OS === "ios" ? "padding" : undefined}
    >
      <ScrollView
        contentContainerStyle={styles.scroll}
        keyboardShouldPersistTaps="handled"
        showsVerticalScrollIndicator={false}
      >
        {/* Header */}
        <View style={styles.header}>
          <Text style={styles.title}>Describe Your Scene</Text>
          <Text style={styles.subtitle}>
            Write in plain English — no film knowledge required
          </Text>
          {user && (
            <View style={styles.creditsBadge}>
              <Text style={styles.creditsText}>
                {user.credits} credit{user.credits !== 1 ? "s" : ""} remaining
              </Text>
            </View>
          )}
        </View>

        {/* Scene description */}
        <View style={styles.section}>
          <Text style={styles.label}>SCENE DESCRIPTION *</Text>
          <TextInput
            style={[styles.textArea, charCount < 20 && charCount > 0 && styles.textAreaWarning]}
            placeholder="e.g. A detective walks into an abandoned warehouse at midnight, torch in hand, slowly realising she is not alone…"
            placeholderTextColor={colors.faint}
            value={sceneDescription}
            onChangeText={setSceneDescription}
            multiline
            maxLength={2000}
            textAlignVertical="top"
          />
          <View style={styles.charRow}>
            <Text style={charCount < 20 ? styles.charWarn : styles.charOk}>
              {charCount < 20
                ? `${20 - charCount} more characters needed`
                : `${charCount} / 2000`}
            </Text>
          </View>
        </View>

        {/* Visual style */}
        <View style={[styles.section, { paddingHorizontal: 0 }]}>
          <Text style={[styles.label, { paddingHorizontal: spacing.md }]}>
            VISUAL STYLE
          </Text>
          <StylePicker value={sceneStyle} onChange={setSceneStyle} />
        </View>

        {/* Optional fields */}
        <View style={styles.section}>
          <Text style={styles.label}>LOCATION (optional)</Text>
          <TextInput
            style={styles.input}
            placeholder="e.g. Abandoned industrial warehouse, Detroit, broken windows, graffiti walls"
            placeholderTextColor={colors.faint}
            value={locationDesc}
            onChangeText={setLocationDesc}
            maxLength={500}
          />
        </View>

        <View style={styles.section}>
          <Text style={styles.label}>CHARACTERS (optional)</Text>
          <TextInput
            style={styles.input}
            placeholder="e.g. Detective Sara, 40s, trench coat; Unknown figure in shadows"
            placeholderTextColor={colors.faint}
            value={characterDesc}
            onChangeText={setCharacterDesc}
            maxLength={500}
          />
        </View>

        {/* Generate button */}
        <TouchableOpacity
          activeOpacity={0.82}
          onPress={handleGenerate}
          disabled={!isReady}
          style={[styles.genBtn, !isReady && styles.genBtnDisabled]}
        >
          {loading ? (
            <ActivityIndicator color={colors.onGold} />
          ) : (
            <>
              <Text style={styles.genBtnText}>Generate Shot List</Text>
              <Text style={styles.genBtnSub}>uses 1 credit · ~15 seconds</Text>
            </>
          )}
        </TouchableOpacity>

        <View style={{ height: 40 }} />
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  root: {
    flex: 1,
    backgroundColor: colors.bg,
  },
  scroll: {
    paddingTop: spacing.xl,
  },
  header: {
    paddingHorizontal: spacing.md,
    marginBottom: spacing.xl,
  },
  title: {
    fontSize: 26,
    fontWeight: "700",
    color: colors.text,
    marginBottom: 6,
  },
  subtitle: {
    fontSize: 14,
    color: colors.muted,
    marginBottom: spacing.md,
  },
  creditsBadge: {
    alignSelf: "flex-start",
    borderWidth: 1,
    borderColor: colors.goldDeep,
    borderRadius: radius.pill,
    paddingHorizontal: 12,
    paddingVertical: 4,
  },
  creditsText: {
    fontSize: 12,
    fontWeight: "600",
    color: colors.gold,
  },
  section: {
    paddingHorizontal: spacing.md,
    marginBottom: spacing.lg,
  },
  label: {
    fontSize: 10,
    fontWeight: "700",
    color: colors.faint,
    letterSpacing: 1.5,
    marginBottom: 8,
  },
  textArea: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.line2,
    borderRadius: radius.md,
    padding: spacing.md,
    minHeight: 130,
    fontSize: 15,
    lineHeight: 22,
    color: colors.text,
  },
  textAreaWarning: {
    borderColor: "#E87B5A",
  },
  charRow: {
    marginTop: 4,
    alignItems: "flex-end",
  },
  charWarn: {
    fontSize: 11,
    color: "#E87B5A",
  },
  charOk: {
    fontSize: 11,
    color: colors.faint,
  },
  input: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.line2,
    borderRadius: radius.md,
    padding: spacing.md,
    fontSize: 14,
    color: colors.text,
  },
  genBtn: {
    marginHorizontal: spacing.md,
    backgroundColor: colors.gold,
    borderRadius: radius.md,
    paddingVertical: spacing.lg,
    alignItems: "center",
    gap: 4,
  },
  genBtnDisabled: {
    backgroundColor: colors.surface3,
  },
  genBtnText: {
    fontSize: 16,
    fontWeight: "700",
    color: colors.onGold,
  },
  genBtnSub: {
    fontSize: 11,
    color: "rgba(26,20,7,0.65)",
    fontWeight: "500",
  },
});
