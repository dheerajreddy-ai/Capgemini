/**
 * Root layout — boots auth and gates navigation between the public (sign-in)
 * and protected (app) stacks based on auth status.
 */

import { Stack, useRouter, useSegments } from "expo-router";
import { useEffect } from "react";
import { StatusBar } from "expo-status-bar";

import { useAuthStore } from "@/store/useAuthStore";
import { colors } from "@/theme";

export default function RootLayout() {
  const init = useAuthStore((s) => s.init);
  const status = useAuthStore((s) => s.status);
  const segments = useSegments();
  const router = useRouter();

  useEffect(() => {
    const unsub = init();
    return unsub;
  }, [init]);

  useEffect(() => {
    if (status === "loading") return;
    const inApp = segments[0] === "(app)";
    if (status === "authenticated" && !inApp) {
      router.replace("/(app)/home");
    } else if (status === "unauthenticated" && inApp) {
      router.replace("/");
    }
  }, [status, segments, router]);

  return (
    <>
      <StatusBar style="light" />
      <Stack
        screenOptions={{
          headerShown: false,
          contentStyle: { backgroundColor: colors.bg },
          animation: "fade",
        }}
      />
    </>
  );
}
