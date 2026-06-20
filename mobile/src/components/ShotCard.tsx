import * as Clipboard from "expo-clipboard";
import React, { useEffect, useRef, useState } from "react";
import {
  Animated,
  Image,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { ShotOut } from "../services/storyboard";
import { colors, radius, spacing } from "../theme";

interface ShotCardProps {
  shot: ShotOut;
  index: number;
  imagesGenerating?: boolean;
}

function MetaRow({ label, value }: { label: string; value: string | null }) {
  if (!value) return null;
  return (
    <View style={styles.metaRow}>
      <Text style={styles.metaLabel}>{label}</Text>
      <Text style={styles.metaValue}>{value}</Text>
    </View>
  );
}

function ImageSkeleton() {
  const opacity = useRef(new Animated.Value(0.3)).current;

  useEffect(() => {
    Animated.loop(
      Animated.sequence([
        Animated.timing(opacity, { toValue: 0.7, duration: 900, useNativeDriver: true }),
        Animated.timing(opacity, { toValue: 0.3, duration: 900, useNativeDriver: true }),
      ]),
    ).start();
  }, [opacity]);

  return (
    <Animated.View style={[styles.imagePlaceholder, { opacity }]}>
      <Text style={styles.placeholderLabel}>Generating image…</Text>
    </Animated.View>
  );
}

export function ShotCard({ shot, index, imagesGenerating = false }: ShotCardProps) {
  const [expanded, setExpanded] = useState(false);
  const [copied, setCopied] = useState(false);

  async function copyPrompt() {
    if (!shot.prompt) return;
    await Clipboard.setStringAsync(shot.prompt);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }

  return (
    <View style={styles.card}>
      {/* Letterbox top bar */}
      <View style={styles.letterboxTop} />

      {/* Storyboard image, skeleton, or static placeholder */}
      {shot.image_url ? (
        <Image source={{ uri: shot.image_url }} style={styles.image} />
      ) : imagesGenerating ? (
        <ImageSkeleton />
      ) : (
        <View style={styles.imagePlaceholder}>
          <Text style={styles.placeholderNumber}>
            {String(shot.shot_number).padStart(2, "0")}
          </Text>
          <Text style={styles.placeholderLabel}>
            {shot.shot_type ?? "Shot"}
          </Text>
        </View>
      )}

      {/* Letterbox bottom bar */}
      <View style={styles.letterboxBottom} />

      {/* Shot info */}
      <View style={styles.body}>
        <View style={styles.headerRow}>
          <View style={styles.badge}>
            <Text style={styles.badgeText}>
              SHOT {String(shot.shot_number).padStart(2, "0")}
            </Text>
          </View>
          {shot.mood && (
            <View style={styles.moodBadge}>
              <Text style={styles.moodText}>{shot.mood}</Text>
            </View>
          )}
          {shot.duration && (
            <Text style={styles.duration}>{shot.duration}</Text>
          )}
        </View>

        <Text style={styles.shotName}>{shot.shot_name ?? "Untitled Shot"}</Text>

        {/* Core meta */}
        <View style={styles.metaGrid}>
          <MetaRow label="TYPE" value={shot.shot_type} />
          <MetaRow label="ANGLE" value={shot.camera_angle} />
          <MetaRow label="MOVEMENT" value={shot.camera_movement} />
          <MetaRow label="LENS" value={shot.lens} />
          <MetaRow label="LIGHTING" value={shot.lighting} />
        </View>

        {/* Expand / collapse for explanation + prompt */}
        <TouchableOpacity
          activeOpacity={0.75}
          onPress={() => setExpanded((e) => !e)}
          style={styles.expandBtn}
        >
          <Text style={styles.expandBtnText}>
            {expanded ? "Hide details ↑" : "See details ↓"}
          </Text>
        </TouchableOpacity>

        {expanded && (
          <>
            {shot.explanation && (
              <View style={styles.section}>
                <Text style={styles.sectionTitle}>WHY THIS SHOT WORKS</Text>
                <Text style={styles.explanationText}>{shot.explanation}</Text>
              </View>
            )}

            {shot.prompt && (
              <View style={styles.section}>
                <View style={styles.promptHeader}>
                  <Text style={styles.sectionTitle}>AI IMAGE PROMPT</Text>
                  <TouchableOpacity onPress={copyPrompt} activeOpacity={0.75}>
                    <Text style={styles.copyBtn}>
                      {copied ? "✓ Copied" : "Copy"}
                    </Text>
                  </TouchableOpacity>
                </View>
                <View style={styles.promptBox}>
                  <Text style={styles.promptText}>{shot.prompt}</Text>
                </View>
              </View>
            )}
          </>
        )}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    overflow: "hidden",
    marginBottom: spacing.md,
    borderWidth: 1,
    borderColor: colors.line,
  },
  letterboxTop: {
    height: 16,
    backgroundColor: "#000",
  },
  letterboxBottom: {
    height: 16,
    backgroundColor: "#000",
  },
  image: {
    width: "100%",
    aspectRatio: 2.39,
    backgroundColor: colors.surface3,
  },
  imagePlaceholder: {
    aspectRatio: 2.39,
    backgroundColor: colors.surface3,
    alignItems: "center",
    justifyContent: "center",
    gap: 4,
  },
  placeholderNumber: {
    fontSize: 32,
    fontWeight: "700",
    color: colors.line2,
    letterSpacing: 4,
  },
  placeholderLabel: {
    fontSize: 11,
    color: colors.faint,
    letterSpacing: 2,
    textTransform: "uppercase",
  },
  body: {
    padding: spacing.md,
  },
  headerRow: {
    flexDirection: "row",
    alignItems: "center",
    gap: spacing.sm,
    marginBottom: spacing.sm,
  },
  badge: {
    backgroundColor: colors.gold,
    borderRadius: radius.sm,
    paddingHorizontal: 8,
    paddingVertical: 3,
  },
  badgeText: {
    fontSize: 10,
    fontWeight: "700",
    color: colors.onGold,
    letterSpacing: 1.5,
  },
  moodBadge: {
    borderWidth: 1,
    borderColor: colors.goldDeep,
    borderRadius: radius.sm,
    paddingHorizontal: 8,
    paddingVertical: 3,
  },
  moodText: {
    fontSize: 10,
    fontWeight: "600",
    color: colors.gold,
    letterSpacing: 1,
  },
  duration: {
    marginLeft: "auto",
    fontSize: 11,
    color: colors.faint,
  },
  shotName: {
    fontSize: 17,
    fontWeight: "600",
    color: colors.text,
    marginBottom: spacing.md,
  },
  metaGrid: {
    gap: 6,
    marginBottom: spacing.sm,
  },
  metaRow: {
    flexDirection: "row",
    gap: spacing.sm,
  },
  metaLabel: {
    fontSize: 10,
    fontWeight: "700",
    color: colors.faint,
    letterSpacing: 1.2,
    width: 80,
    paddingTop: 1,
  },
  metaValue: {
    fontSize: 13,
    color: colors.muted,
    flex: 1,
  },
  expandBtn: {
    marginTop: spacing.sm,
    paddingVertical: 6,
    alignSelf: "flex-start",
  },
  expandBtnText: {
    fontSize: 12,
    fontWeight: "600",
    color: colors.gold,
  },
  section: {
    marginTop: spacing.md,
  },
  sectionTitle: {
    fontSize: 10,
    fontWeight: "700",
    color: colors.faint,
    letterSpacing: 1.5,
    marginBottom: 6,
  },
  explanationText: {
    fontSize: 14,
    lineHeight: 21,
    color: colors.muted,
  },
  promptHeader: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    marginBottom: 6,
  },
  copyBtn: {
    fontSize: 12,
    fontWeight: "600",
    color: colors.gold,
  },
  promptBox: {
    backgroundColor: colors.surface3,
    borderRadius: radius.sm,
    padding: spacing.md,
    borderWidth: 1,
    borderColor: colors.line,
  },
  promptText: {
    fontSize: 12,
    lineHeight: 18,
    color: colors.muted,
    fontFamily: "monospace",
  },
});
