# FRAME API — Backend

Enterprise FastAPI backend for the FRAME cinematography app. Phase 1 delivers
the foundation: Firebase-authenticated users, PostgreSQL persistence, and
production hardening, deployable to Railway.

## Stack

| Concern        | Choice                                  |
|----------------|-----------------------------------------|
| Framework      | FastAPI + Uvicorn/Gunicorn              |
| Database       | PostgreSQL via SQLAlchemy 2.0 + Alembic |
| Auth           | Firebase ID-token verification          |
| Validation     | Pydantic v2                             |
| Rate limiting  | SlowAPI (per-IP)                        |
| Logging        | structlog (JSON in prod)                |
| Deploy         | Docker → Railway                        |

## Architecture

```
app/
├── api/v1/            HTTP layer (routes, dependencies, router)
├── core/              config, logging, security headers, exceptions, middleware
├── db/                engine, session, declarative base + mixins
├── models/            SQLAlchemy ORM models (users, storyboards, shots)
├── repositories/      data-access layer (no ORM leaks into services/routes)
├── schemas/           Pydantic request/response contracts
├── services/          integrations & business logic (Firebase; AI in Phase 2+)
├── rate_limit.py      shared limiter instance
└── main.py            app factory + middleware wiring
```

Request path: **middleware (request-id, security headers, CORS) → rate limit →
route → auth dependency (Firebase verify) → repository → DB**. Errors are
funneled through typed handlers that never leak internals to clients.

## Security posture (Phase 1)

- No secrets in source — everything via env vars (`.env.example` documents them).
- Firebase verifies token signature, expiry, issuer, audience server-side.
- All DB access through the ORM (parameterized) — no raw SQL, no injection.
- UUID primary keys; soft deletes (no hard-deleting user data).
- Security headers (HSTS, nosniff, frame-deny) on every response.
- Per-IP rate limiting to cap abuse and downstream AI cost.
- Standardized error envelope; stack traces logged, never returned.
- Interactive docs disabled in production.

## Local development

```bash
cd backend
python -m venv .venv && source .venv/bin/activate
pip install -r requirements.txt
cp .env.example .env          # fill DATABASE_URL + Firebase creds

# start a local Postgres (example)
# docker run -d --name frame-db -e POSTGRES_PASSWORD=frame \
#   -e POSTGRES_USER=frame -e POSTGRES_DB=frame -p 5432:5432 postgres:16

alembic upgrade head          # apply migrations
uvicorn app.main:app --reload # http://localhost:8000/docs
```

## Tests

```bash
pytest -q
```

## Deploy to Railway

1. Create a project, add the **PostgreSQL** plugin (sets `DATABASE_URL`).
2. Point the service at `backend/` (Dockerfile build via `railway.toml`).
3. Set env vars: `ENVIRONMENT=production`, `FIREBASE_CREDENTIALS_JSON`,
   `FIREBASE_PROJECT_ID`, `CORS_ORIGINS`.
4. Deploy. The container runs `alembic upgrade head` then Gunicorn.
   Healthcheck: `/api/v1/health`.

## Endpoints (Phase 1)

| Method | Path                  | Auth | Purpose                              |
|--------|-----------------------|------|--------------------------------------|
| GET    | `/api/v1/health`      | —    | Liveness                             |
| GET    | `/api/v1/health/ready`| —    | Readiness (checks DB)                |
| POST   | `/api/v1/auth/login`  | Bearer | Verify Firebase token, provision user |
| GET    | `/api/v1/auth/me`     | Bearer | Current user profile                 |
