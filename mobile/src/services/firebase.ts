/**
 * Firebase auth wiring.
 *
 * Production Google sign-in flow on a real device:
 *   1. Native Google Sign-In sheet returns a Google ID token.
 *   2. We exchange it for a Firebase credential and sign in.
 *   3. Firebase hands us an ID token (JWT) that the backend verifies.
 *
 * All identifiers come from env (app.config / EAS secrets), never hardcoded.
 */

import { GoogleSignin } from "@react-native-google-signin/google-signin";
import Constants from "expo-constants";
import { initializeApp } from "firebase/app";
import {
  GoogleAuthProvider,
  getAuth,
  onAuthStateChanged,
  signInWithCredential,
  signOut as fbSignOut,
  type User as FirebaseUser,
} from "firebase/auth";

const extra = Constants.expoConfig?.extra ?? {};

const firebaseApp = initializeApp({
  apiKey: extra.firebaseApiKey,
  authDomain: extra.firebaseAuthDomain,
  projectId: extra.firebaseProjectId,
  appId: extra.firebaseAppId,
});

export const auth = getAuth(firebaseApp);

GoogleSignin.configure({
  // Web client ID from the Firebase / Google Cloud console.
  webClientId: extra.googleWebClientId,
});

/** Run the native Google flow and return the signed-in Firebase user. */
export async function signInWithGoogle(): Promise<FirebaseUser> {
  await GoogleSignin.hasPlayServices();
  const { data } = await GoogleSignin.signIn();
  const idToken = data?.idToken;
  if (!idToken) throw new Error("Google sign-in did not return an ID token.");

  const credential = GoogleAuthProvider.credential(idToken);
  const result = await signInWithCredential(auth, credential);
  return result.user;
}

export async function signOut(): Promise<void> {
  await fbSignOut(auth);
  try {
    await GoogleSignin.signOut();
  } catch {
    /* already signed out */
  }
}

/** Fresh ID token for the backend Authorization header. */
export async function getIdToken(): Promise<string | null> {
  const user = auth.currentUser;
  return user ? user.getIdToken() : null;
}

export function subscribeToAuth(cb: (user: FirebaseUser | null) => void) {
  return onAuthStateChanged(auth, cb);
}

export type { FirebaseUser };
