/**
 * Dynamic Expo config — injects environment values into `extra` at build time
 * so no identifiers are hardcoded in source. Reads from process.env (local
 * `.env` via your shell, or EAS secrets in CI/CD).
 */

import type { ExpoConfig } from "expo/config";

import base from "./app.json";

export default (): ExpoConfig => ({
  ...(base.expo as ExpoConfig),
  extra: {
    ...base.expo.extra,
    apiBaseUrl: process.env.API_BASE_URL ?? "http://localhost:8000/api/v1",
    firebaseApiKey: process.env.FIREBASE_API_KEY,
    firebaseAuthDomain: process.env.FIREBASE_AUTH_DOMAIN,
    firebaseProjectId: process.env.FIREBASE_PROJECT_ID,
    firebaseAppId: process.env.FIREBASE_APP_ID,
    googleWebClientId: process.env.GOOGLE_WEB_CLIENT_ID,
  },
});
