/**
 * Dynamic Expo config — env vars use EXPO_PUBLIC_* prefix (matching SuperFit).
 * Expo exposes EXPO_PUBLIC_* to the client bundle automatically, so no extra
 * mapping needed. This file just passes them through to extra for legacy access.
 */

import type { ExpoConfig } from "expo/config";
import base from "./app.json";

export default (): ExpoConfig => ({
  ...(base.expo as ExpoConfig),
  extra: {
    ...base.expo.extra,
    // All EXPO_PUBLIC_* vars are already available via process.env in the app.
    // extra mirrors them here for libraries that read Constants.expoConfig.extra.
    apiBaseUrl: process.env.EXPO_PUBLIC_API_URL ?? "http://localhost:8000/api/v1",
    firebaseProjectId: process.env.EXPO_PUBLIC_FIREBASE_PROJECT_ID ?? "saifit-25ac6",
  },
});
