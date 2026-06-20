/**
 * Global auth state (Zustand).
 *
 * Owns the lifecycle: listen to Firebase, exchange the token with our backend
 * (`POST /auth/login`) to provision/fetch the app user, and expose status to
 * the router so it can gate screens.
 */

import { create } from "zustand";

import { api, setTokenProvider } from "@/services/api";
import {
  getIdToken,
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

type LoginResponse = { user: AppUser; is_new_user: boolean };

type AuthState = {
  status: "loading" | "authenticated" | "unauthenticated";
  user: AppUser | null;
  error: string | null;
  init: () => () => void;
  signIn: () => Promise<void>;
  signOut: () => Promise<void>;
};

export const useAuthStore = create<AuthState>((set, get) => ({
  status: "loading",
  user: null,
  error: null,

  /** Call once at app root; returns an unsubscribe fn. */
  init: () => {
    setTokenProvider(getIdToken);
    const unsub = subscribeToAuth(async (firebaseUser) => {
      if (!firebaseUser) {
        set({ status: "unauthenticated", user: null });
        return;
      }
      try {
        const { user } = await api.post<LoginResponse>("/auth/login");
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
      await signInWithGoogle();
      // subscribeToAuth handles the backend exchange + state transition.
    } catch (e: any) {
      set({ error: e.message ?? "Sign-in failed. Please try again." });
    }
  },

  signOut: async () => {
    await firebaseSignOut();
    set({ status: "unauthenticated", user: null });
  },
}));
