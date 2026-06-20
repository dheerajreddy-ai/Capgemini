/**
 * Design tokens — the single source of truth for the FRAME look.
 * Mirrors the approved prototype: dark-first cinema black, one warm-gold
 * accent, generous spacing, editorial type scale.
 */

export const colors = {
  bg: "#08080A",
  surface: "#101013",
  surface2: "#17171C",
  surface3: "#1F1F26",
  line: "rgba(255,255,255,0.07)",
  line2: "rgba(255,255,255,0.12)",
  text: "#F4F3F1",
  muted: "#9A988F",
  faint: "#6A685F",
  gold: "#E8B65A",
  goldSoft: "#F0CE8E",
  goldDeep: "#B7843A",
  onGold: "#1A1407",
} as const;

export const spacing = {
  xs: 6,
  sm: 10,
  md: 16,
  lg: 22,
  xl: 30,
  xxl: 44,
} as const;

export const radius = {
  sm: 12,
  md: 16,
  lg: 20,
  pill: 100,
} as const;

export const type = {
  display: { fontFamily: "Space Grotesk", fontWeight: "700" as const },
  serif: { fontFamily: "Instrument Serif" },
  body: { fontFamily: "Inter" },
} as const;
