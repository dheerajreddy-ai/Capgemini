/**
 * Global auth state (Zustand) — aligned with SuperFit's sync pattern.
 *
 * After Firebase sign-in, calls POST /auth/sync (not /login) to provision
 * the user row on first call. All other endpoints depend on that row.
 */

import { create } from "zustand";

import { api, setTokenProvider } from "@/services/api";
import {
  getIdToken,
  isMockAuthMode,
  signInWithGoogle,
  signOut as firebaseSignOut,
  subscribeToAuth,
} from "@/services/firebase";

export type AppUser = {
  id: string;
  email: string;
  display_name: string | null;
  avatar_url: string | null;
  plan: "free" | "starter" | "pro" | "studio";
  credits: number;
};

type SyncResponse = { user: AppUser; is_new_user: boolean };

type AuthState = {
  status: "loading" | "authenticated" | "unauthenticated";
  user: AppUser | null;
  error: string | null;
  init: () => () => void;
  signIn: () => Promise<void>;
  signOut: () => Promise<void>;
};

export const useAuthStore = create<AuthState>((set) => ({
  status: "loading",
  user: null,
  error: null,

  init: () => {
    setTokenProvider(getIdToken);

    const unsub = subscribeToAuth(async (firebaseUser) => {
      if (!firebaseUser) {
        set({ status: "unauthenticated", user: null });
        return;
      }
      try {
        // /auth/sync creates the DB row on first call — must run before anything else.
        const { user } = await api.post<SyncResponse>("/auth/sync");
        set({ status: "authenticated", user, error: null });
      } catch (e: any) {
        set({ status: "unauthenticated", user: null, error: e.message });
      }
    });

    return unsub;
  },

  signIn: async () => {
    set({ error: null });
    try {
      if (isMockAuthMode) {
        // Mock: subscribeToAuth already emits a mock user; just sync it.
        const { user } = await api.post<SyncResponse>("/auth/sync");
        set({ status: "authenticated", user, error: null });
        return;
      }
      await signInWithGoogle();
      // subscribeToAuth handles the /auth/sync exchange + state update.
    } catch (e: any) {
      set({ error: e.message ?? "Sign-in failed. Please try again." });
    }
  },

  signOut: async () => {
    await firebaseSignOut();
    set({ status: "unauthenticated", user: null });
  },
}));
