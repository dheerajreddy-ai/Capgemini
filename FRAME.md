# FRAME — AI Cinematography Storyboard App

> Describe a scene in plain words → FRAME translates it into real
> cinematographic language, generates a director-grade storyboard, and teaches
> you why each shot works.

Working name: **FRAME**. Target user: complete beginners with no film training.

## Confirmed stack

| Layer            | Technology                          |
|------------------|-------------------------------------|
| Mobile           | React Native + Expo (TypeScript)    |
| Auth             | Firebase Auth (Google Sign-In)      |
| Backend          | FastAPI (Python) on Railway         |
| Database         | PostgreSQL on Railway               |
| AI — prompts     | Gemini 2.5 Flash *(Phase 2)*        |
| AI — images      | Nano Banana Pro *(Phase 3)*         |
| Image storage    | Cloudflare R2 *(Phase 3)*           |

## Repository layout

```
backend/        FastAPI service (see backend/README.md)
mobile/         React Native + Expo app
design/         High-fidelity UI prototype (open design/ui-mockup.html)
FRAME.md        This file — product + roadmap overview
```

## Roadmap

| Phase | Focus                              | Status        |
|-------|------------------------------------|---------------|
| 1     | Foundation: auth, DB, deploy       | ✅ In progress |
| 2     | Gemini prompt builder              | Planned       |
| 3     | Nano Banana images + async queue   | Planned       |
| 4     | History, credits, sharing          | Planned       |
| 5     | Stripe payments                    | Planned       |
| 6     | Tests, security audit, load test   | Planned       |
| 7     | App Store / Play launch            | Planned       |
| 8     | Scale (AWS/GCP, CDN, replicas)     | Planned       |

## Phase 1 — what's built

**Backend**
- Enterprise FastAPI structure (api / core / db / models / repositories /
  services / schemas).
- Firebase ID-token verification + lazy user provisioning.
- PostgreSQL models (`users`, `storyboards`, `shots`) with UUID keys, soft
  deletes, timestamps; Alembic initial migration.
- Security: per-IP rate limiting, security headers, typed error envelopes,
  structured JSON logging, request IDs.
- Dockerfile + `railway.toml` for one-click Railway deploy. Tests passing.

**Mobile**
- Expo Router app shell with auth gating.
- Google Sign-In flow (Firebase) → backend `/auth/login` exchange.
- Sign-in screen matching the approved prototype; placeholder authed home.
- Shared design tokens, typed API client, Zustand auth store.

## Getting started

- Backend: see [`backend/README.md`](backend/README.md).
- Mobile: see [`mobile/README.md`](mobile/README.md).
- Design: open [`design/ui-mockup.html`](design/ui-mockup.html) in a browser.
