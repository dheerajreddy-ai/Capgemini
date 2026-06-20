/**
 * Thin API client for the FRAME backend.
 *
 * - Reads the base URL from Expo config (per-environment).
 * - Attaches the Firebase ID token as a Bearer header on every call.
 * - Unwraps the standard `{ success, data }` / `{ success, error }` envelope
 *   so screens deal in plain data or a typed ApiError.
 */

import Constants from "expo-constants";

const BASE_URL: string =
  (Constants.expoConfig?.extra?.apiBaseUrl as string) ??
  "http://localhost:8000/api/v1";

export class ApiError extends Error {
  code: string;
  status: number;
  constructor(code: string, message: string, status: number) {
    super(message);
    this.code = code;
    this.status = status;
  }
}

type TokenProvider = () => Promise<string | null>;

let getToken: TokenProvider = async () => null;

/** Wire the auth layer in once at startup (see store/useAuthStore). */
export function setTokenProvider(provider: TokenProvider): void {
  getToken = provider;
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = await getToken();
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(init.headers as Record<string, string>),
  };
  if (token) headers.Authorization = `Bearer ${token}`;

  const res = await fetch(`${BASE_URL}${path}`, { ...init, headers });

  let body: any = null;
  try {
    body = await res.json();
  } catch {
    /* non-JSON response */
  }

  if (!res.ok || body?.success === false) {
    const err = body?.error ?? {};
    throw new ApiError(
      err.code ?? "unknown_error",
      err.message ?? `Request failed (${res.status})`,
      res.status,
    );
  }

  return body?.data as T;
}

export const api = {
  get: <T>(path: string) => request<T>(path, { method: "GET" }),
  post: <T>(path: string, data?: unknown) =>
    request<T>(path, { method: "POST", body: data ? JSON.stringify(data) : undefined }),
  del: <T>(path: string) => request<T>(path, { method: "DELETE" }),
};
