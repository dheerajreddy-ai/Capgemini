/**
 * Firebase auth — aligned with SuperFit's lazy-load pattern.
 *
 * CRITICAL: Never import the Firebase SDK at module top-level outside this
 * file. Use require('./firebase') inside real-auth branches only. Mock mode
 * and Jest must never load the SDK.
 *
 * The isMockAuthMode flag (from EXPO_PUBLIC_FORCE_MOCK_AUTH or missing env
 * vars) gates every real Firebase call, mirroring SuperFit's approach.
 */

// ── env flags (safe to read at top-level — no SDK) ────────────────────────
const apiKey = process.env.EXPO_PUBLIC_FIREBASE_API_KEY;
const forceMock = process.env.EXPO_PUBLIC_FORCE_MOCK_AUTH;

export const firebaseWebConfig = apiKey
  ? {
      apiKey,
      authDomain: process.env.EXPO_PUBLIC_FIREBASE_AUTH_DOMAIN ?? "",
      projectId: process.env.EXPO_PUBLIC_FIREBASE_PROJECT_ID ?? "",
      storageBucket: process.env.EXPO_PUBLIC_FIREBASE_STORAGE_BUCKET ?? "",
      messagingSenderId:
        process.env.EXPO_PUBLIC_FIREBASE_MESSAGING_SENDER_ID ?? "",
      appId: process.env.EXPO_PUBLIC_FIREBASE_APP_ID ?? "",
    }
  : null;

export const isMockAuthMode =
  firebaseWebConfig === null ||
  forceMock === "1" ||
  forceMock === "true";

// ── lazy helpers (only called in real-auth branches) ──────────────────────

function getFirebaseAuth() {
  if (!firebaseWebConfig) throw new Error("Firebase not configured.");
  // require() so Jest / mock mode never loads the SDK at import time.
  const { getApp, getApps, initializeApp } = require("firebase/app");
  const { getAuth } = require("firebase/auth");
  const app =
    getApps().length === 0 ? initializeApp(firebaseWebConfig) : getApp();
  return getAuth(app);
}

/** Run the native Google sign-in and return the Firebase ID token. */
export async function signInWithGoogle(): Promise<string> {
  if (isMockAuthMode) {
    // Mock: return a token the backend will accept when ALLOW_MOCK_AUTH=true.
    const payload = encodeURIComponent(
      JSON.stringify({ uid: "mock_google_user", email: "dev@frame.ai" })
    );
    return `mock_token_${payload}`;
  }

  const { GoogleAuthProvider, signInWithPopup } = require("firebase/auth");
  const auth = getFirebaseAuth();
  const result = await signInWithPopup(auth, new GoogleAuthProvider());
  return result.user.getIdToken();
}

/** Get a fresh ID token for attaching to API requests. */
export async function getIdToken(): Promise<string | null> {
  if (isMockAuthMode) {
    const payload = encodeURIComponent(
      JSON.stringify({ uid: "mock_google_user", email: "dev@frame.ai" })
    );
    return `mock_token_${payload}`;
  }
  const auth = getFirebaseAuth();
  return auth.currentUser ? auth.currentUser.getIdToken() : null;
}

export async function signOut(): Promise<void> {
  if (isMockAuthMode) return;
  const { signOut: fbSignOut } = require("firebase/auth");
  await fbSignOut(getFirebaseAuth());
}

export function subscribeToAuth(
  cb: (user: { uid: string; email: string | null } | null) => void
): () => void {
  if (isMockAuthMode) {
    // Immediately emit a mock user so the auth store transitions to authed.
    setTimeout(
      () => cb({ uid: "mock_google_user", email: "dev@frame.ai" }),
      0
    );
    return () => {};
  }
  const { onAuthStateChanged } = require("firebase/auth");
  return onAuthStateChanged(getFirebaseAuth(), cb);
}
