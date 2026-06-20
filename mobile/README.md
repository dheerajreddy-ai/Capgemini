# FRAME — Mobile

React Native + Expo (TypeScript) app. Phase 1 ships the auth shell: Google
Sign-In via Firebase, the backend token exchange, and the sign-in screen from
the approved prototype.

## Structure

```
app/                 Expo Router routes
├── _layout.tsx      root: boots auth, gates public vs (app) stacks
├── index.tsx        Sign-In screen (public)
└── (app)/           authenticated stack
    ├── _layout.tsx
    └── home.tsx     placeholder home (proves the round-trip)
src/
├── theme.ts         design tokens (colors, spacing, radius, type)
├── services/
│   ├── api.ts       typed client; attaches Firebase Bearer token
│   └── firebase.ts  Google → Firebase credential → ID token
└── store/
    └── useAuthStore.ts  Zustand auth lifecycle
```

## Setup

```bash
cd mobile
npm install
cp .env.example .env     # fill Firebase + Google web client IDs + API URL
npx expo start
```

Google Sign-In needs a development build (not Expo Go) because of the native
`@react-native-google-signin` module:

```bash
npx expo run:android   # or: npx expo run:ios
```

## Config

All identifiers come from env via `app.config.ts` → `extra` (never hardcoded).
See `.env.example`. These are client identifiers (safe to ship); server secrets
live only in the backend.

## Auth flow

1. User taps **Continue with Google** → native Google sheet.
2. Google ID token → Firebase credential → Firebase signs in.
3. `onAuthStateChanged` fires → store calls `POST /auth/login` with the Firebase
   ID token as a Bearer header.
4. Backend verifies, provisions/returns the user → app routes to `(app)/home`.
