/**
 * Storyboard result screen — full shot list with letterbox cards.
 * Route: /(app)/storyboard/[id]
 */

import { useLocalSearchParams, useRouter } from "expo-router";
import React, { useCallback, useEffect, useRef, useState } from "react";
import {
  ActivityIndicator,
  Alert,
  RefreshControl,
  ScrollView,
  Share,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { ShotCard } from "../../../src/components/ShotCard";
import { StoryboardOut, storyboardApi } from "../../../src/services/storyboard";
import { colors, radius, spacing } from "../../../src/theme";

const POLL_INTERVAL_MS = 5000;
const POLL_TIMEOUT_MS = 3 * 60 * 1000; // stop polling after 3 minutes

function imagesGenerating(board: StoryboardOut): boolean {
  return board.images_status === "pending" || board.images_status === "generating";
}

export default function StoryboardScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();

  const [board, setBoard] = useState<StoryboardOut | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const pollTimer = useRef<ReturnType<typeof setTimeout> | null>(null);
  const pollStart = useRef<number>(0);

  const load = useCallback(
    async (silent = false) => {
      if (!id) return;
      if (!silent) setLoading(true);
      setError(null);
      try {
        const data = await storyboardApi.getOne(id);
        setBoard(data);
        return data;
      } catch (err: any) {
        setError(err?.message ?? "Failed to load storyboard.");
        return null;
      } finally {
        setLoading(false);
        setRefreshing(false);
      }
    },
    [id],
  );

  // Auto-poll while background image generation is in progress.
  const schedulePoll = useCallback(() => {
    if (pollTimer.current) clearTimeout(pollTimer.current);
    if (Date.now() - pollStart.current > POLL_TIMEOUT_MS) return;

    pollTimer.current = setTimeout(async () => {
      const data = await load(true);
      if (data && imagesGenerating(data)) {
        schedulePoll();
      }
    }, POLL_INTERVAL_MS);
  }, [load]);

  useEffect(() => {
    pollStart.current = Date.now();
    load().then((data) => {
      if (data && imagesGenerating(data)) {
        schedulePoll();
      }
    });
    return () => {
      if (pollTimer.current) clearTimeout(pollTimer.current);
    };
  }, [load, schedulePoll]);

  function onRefresh() {
    setRefreshing(true);
    load(true);
  }

  async function handleShare() {
    if (!board) return;
    const lines = [
      `📽 ${board.title ?? "Storyboard"}`,
      board.director_note ? `"${board.director_note}"` : "",
      "",
      ...board.shots.map(
        (s) =>
          `Shot ${s.shot_number}: ${s.shot_name ?? s.shot_type} — ${s.camera_angle}, ${s.lens}`,
      ),
      "",
      "Generated with FRAME — AI Cinematography",
    ].filter(Boolean);
    await Share.share({ message: lines.join("\n") });
  }

  async function handleDelete() {
    if (!id) return;
    Alert.alert(
      "Delete storyboard?",
      "This cannot be undone.",
      [
        { text: "Cancel", style: "cancel" },
        {
          text: "Delete",
          style: "destructive",
          onPress: async () => {
            try {
              await storyboardApi.delete(id);
              router.back();
            } catch (err: any) {
              Alert.alert("Error", err?.message ?? "Could not delete.");
            }
          },
        },
      ],
    );
  }

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator color={colors.gold} size="large" />
        <Text style={styles.loadingText}>Loading storyboard…</Text>
      </View>
    );
  }

  if (error || !board) {
    return (
      <View style={styles.center}>
        <Text style={styles.errorText}>{error ?? "Not found."}</Text>
        <TouchableOpacity onPress={() => load()} style={styles.retryBtn}>
          <Text style={styles.retryText}>Retry</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <ScrollView
      style={styles.root}
      contentContainerStyle={styles.scroll}
      refreshControl={
        <RefreshControl
          refreshing={refreshing}
          onRefresh={onRefresh}
          tintColor={colors.gold}
        />
      }
    >
      {/* Image generation progress banner */}
      {imagesGenerating(board) && (
        <View style={styles.generatingBanner}>
          <ActivityIndicator size="small" color={colors.gold} />
          <Text style={styles.generatingText}>
            Generating storyboard images… updating automatically
          </Text>
        </View>
      )}

      {/* Board header */}
      <View style={styles.header}>
        <Text style={styles.title}>{board.title ?? "Untitled Scene"}</Text>
        {board.director_note ? (
          <Text style={styles.directorNote}>"{board.director_note}"</Text>
        ) : null}

        <View style={styles.metaRow}>
          {board.scene_style && (
            <View style={styles.styleBadge}>
              <Text style={styles.styleText}>{board.scene_style}</Text>
            </View>
          )}
          <Text style={styles.shotCount}>
            {board.shots.length} shot{board.shots.length !== 1 ? "s" : ""}
          </Text>
        </View>

        {/* Actions */}
        <View style={styles.actions}>
          <TouchableOpacity
            style={styles.actionBtn}
            activeOpacity={0.75}
            onPress={handleShare}
          >
            <Text style={styles.actionBtnText}>Share</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.actionBtn, styles.actionBtnDestructive]}
            activeOpacity={0.75}
            onPress={handleDelete}
          >
            <Text style={styles.actionBtnDestructiveText}>Delete</Text>
          </TouchableOpacity>
        </View>
      </View>

      {/* Scene description context */}
      <View style={styles.descCard}>
        <Text style={styles.descLabel}>SCENE</Text>
        <Text style={styles.descText}>{board.scene_description}</Text>
      </View>

      {/* Divider */}
      <View style={styles.divider} />
      <Text style={styles.shotsHeading}>SHOT LIST</Text>

      {/* Shot cards */}
      {board.shots
        .slice()
        .sort((a, b) => a.shot_number - b.shot_number)
        .map((shot, i) => (
          <ShotCard
            key={shot.id}
            shot={shot}
            index={i}
            imagesGenerating={imagesGenerating(board)}
          />
        ))}

      <View style={{ height: 40 }} />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  root: {
    flex: 1,
    backgroundColor: colors.bg,
  },
  scroll: {
    paddingHorizontal: spacing.md,
    paddingTop: spacing.xl,
  },
  center: {
    flex: 1,
    backgroundColor: colors.bg,
    alignItems: "center",
    justifyContent: "center",
    gap: spacing.md,
    padding: spacing.xl,
  },
  loadingText: {
    fontSize: 14,
    color: colors.muted,
    marginTop: spacing.sm,
  },
  errorText: {
    fontSize: 15,
    color: colors.muted,
    textAlign: "center",
  },
  retryBtn: {
    borderWidth: 1,
    borderColor: colors.gold,
    borderRadius: radius.pill,
    paddingVertical: 8,
    paddingHorizontal: 24,
  },
  retryText: {
    fontSize: 13,
    fontWeight: "600",
    color: colors.gold,
  },
  generatingBanner: {
    flexDirection: "row",
    alignItems: "center",
    gap: spacing.sm,
    backgroundColor: "rgba(232,182,90,0.08)",
    borderWidth: 1,
    borderColor: colors.goldDeep,
    borderRadius: radius.md,
    padding: spacing.md,
    marginBottom: spacing.md,
  },
  generatingText: {
    flex: 1,
    fontSize: 13,
    color: colors.gold,
    lineHeight: 18,
  },
  header: {
    marginBottom: spacing.lg,
  },
  title: {
    fontSize: 24,
    fontWeight: "700",
    color: colors.text,
    marginBottom: 8,
  },
  directorNote: {
    fontSize: 14,
    fontStyle: "italic",
    color: colors.muted,
    marginBottom: spacing.md,
    lineHeight: 20,
  },
  metaRow: {
    flexDirection: "row",
    alignItems: "center",
    gap: spacing.sm,
    marginBottom: spacing.md,
  },
  styleBadge: {
    backgroundColor: "rgba(232,182,90,0.12)",
    borderWidth: 1,
    borderColor: colors.goldDeep,
    borderRadius: radius.pill,
    paddingHorizontal: 12,
    paddingVertical: 3,
  },
  styleText: {
    fontSize: 11,
    fontWeight: "600",
    color: colors.gold,
    letterSpacing: 1,
  },
  shotCount: {
    fontSize: 13,
    color: colors.faint,
  },
  actions: {
    flexDirection: "row",
    gap: spacing.sm,
  },
  actionBtn: {
    borderWidth: 1,
    borderColor: colors.line2,
    borderRadius: radius.pill,
    paddingVertical: 7,
    paddingHorizontal: 20,
  },
  actionBtnText: {
    fontSize: 13,
    fontWeight: "600",
    color: colors.text,
  },
  actionBtnDestructive: {
    borderColor: "rgba(232,90,90,0.3)",
  },
  actionBtnDestructiveText: {
    fontSize: 13,
    fontWeight: "600",
    color: "#E85A5A",
  },
  descCard: {
    backgroundColor: colors.surface,
    borderRadius: radius.md,
    padding: spacing.md,
    borderWidth: 1,
    borderColor: colors.line,
    marginBottom: spacing.lg,
  },
  descLabel: {
    fontSize: 10,
    fontWeight: "700",
    color: colors.faint,
    letterSpacing: 1.5,
    marginBottom: 6,
  },
  descText: {
    fontSize: 14,
    lineHeight: 21,
    color: colors.muted,
  },
  divider: {
    height: 1,
    backgroundColor: colors.line,
    marginBottom: spacing.md,
  },
  shotsHeading: {
    fontSize: 10,
    fontWeight: "700",
    color: colors.faint,
    letterSpacing: 2,
    marginBottom: spacing.md,
  },
});
