import React from "react";
import { ScrollView, StyleSheet, Text, TouchableOpacity, View } from "react-native";
import { colors, radius, spacing } from "../theme";

const STYLES = [
  { label: "Cinematic", icon: "🎬" },
  { label: "Moody", icon: "🌑" },
  { label: "Warm", icon: "🌅" },
  { label: "Dramatic", icon: "⚡" },
  { label: "Vintage", icon: "🎞" },
  { label: "Cold", icon: "❄️" },
  { label: "Noir", icon: "🕵️" },
  { label: "Documentary", icon: "📽" },
] as const;

type StyleLabel = (typeof STYLES)[number]["label"];

interface StylePickerProps {
  value: string;
  onChange: (style: StyleLabel) => void;
}

export function StylePicker({ value, onChange }: StylePickerProps) {
  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      contentContainerStyle={styles.row}
    >
      {STYLES.map((s) => {
        const active = value === s.label;
        return (
          <TouchableOpacity
            key={s.label}
            activeOpacity={0.75}
            onPress={() => onChange(s.label)}
            style={[styles.chip, active && styles.chipActive]}
          >
            <Text style={styles.icon}>{s.icon}</Text>
            <Text style={[styles.label, active && styles.labelActive]}>
              {s.label}
            </Text>
          </TouchableOpacity>
        );
      })}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  row: {
    paddingHorizontal: spacing.md,
    gap: spacing.sm,
  },
  chip: {
    flexDirection: "row",
    alignItems: "center",
    gap: 6,
    paddingVertical: 8,
    paddingHorizontal: 14,
    borderRadius: radius.pill,
    borderWidth: 1,
    borderColor: colors.line2,
    backgroundColor: colors.surface2,
  },
  chipActive: {
    borderColor: colors.gold,
    backgroundColor: "rgba(232,182,90,0.12)",
  },
  icon: {
    fontSize: 14,
  },
  label: {
    fontSize: 13,
    fontWeight: "500",
    color: colors.muted,
  },
  labelActive: {
    color: colors.gold,
  },
});
