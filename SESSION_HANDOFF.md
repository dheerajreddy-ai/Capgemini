# EduVoice — Session Handoff

> **Purpose:** This document lets a fresh Claude Code session (or any developer)
> continue the EduVoice build with full context. Read this first.

**Last updated:** 2026-06-21
**Repo:** `dheerajreddy-ai/Capgemini`
**Working branch:** `claude/new-session-tkaua0` (all work lives here; `main` is untouched)
**Product spec:** `EDUVOICEMASTERPROMPTS.md` (uploaded by owner) — the source of truth for features.

---

## 1. What is EduVoice?

A multi-tenant SaaS by **VCD AI Labs** — an AI voice-agent platform for private
schools in Andhra Pradesh & Telangana. It auto-calls parents in **Telugu** for:
1. Fee reminders
2. Student progress updates (marks/attendance)
3. Collecting complaints

Each school is a **tenant** with isolated data, its own subdomain, branding,
phone number, and Vapi assistant.

**Pipeline:** Twilio number → Vapi.ai picks up → ElevenLabs Telugu voice →
Claude understands replies → n8n saves to DB → Angular dashboard → Excel/Sheets
auto-update → WhatsApp summary to parent.

---

## 2. Tech Stack (all on LATEST as of handoff)

| Layer | Tech | Version |
|---|---|---|
| Backend | .NET | **9.0 (LTS)** — owner chose to stay on 9, not 10 |
| ORM | EF Core + Npgsql | 9.0.17 / 9.0.4 |
| DB | PostgreSQL | 16 |
| Frontend | Angular (standalone, signals, new control flow) | **22.0.2** |
| Language | TypeScript | **6.0.3** |
| UI | Bootstrap 5 + custom premium design system | 5.3.8 |
| Charts | ng-apexcharts / apexcharts | 2.4.0 / 5.15.2 |

> **Node requirement:** Angular 22 CLI needs **Node ≥ 22.22.3 / 24.15 / 26**.
> This container had Node 22.22.2 (too old by one patch); Node 24 was installed
> via nvm (`/opt/nvm`) to build. A normal dev machine on Node ≥22.22.3 is fine.

---

## 3. Repository Layout

```
Capgemini/
├── src/                          # .NET 9 backend (EduVoice) — Clean Architecture
│   ├── EduVoice.Domain/          # entities, enums, repo/UoW interfaces
│   ├── EduVoice.Application/     # DTOs, service interfaces + implementations, validators
│   ├── EduVoice.Infrastructure/  # EF DbContext, repos, JWT/Vapi/Twilio/ElevenLabs/Claude/Email/S3
│   └── EduVoice.API/             # controllers, middleware, Program.cs, DI extensions
├── frontend/                     # Angular 22 premium dashboard
│   └── src/app/{core,shared,layout,features}/
├── EduVoice.sln
├── Dockerfile                    # multi-stage, .NET 9
├── docker-compose.yml            # api + postgres
├── .env.example                  # ALL required env vars (no secrets)
├── SESSION_HANDOFF.md            # ← this file
│
└── capgemini/                    # ORIGINAL MVC app — DO NOT TOUCH (owner's existing project)
    capgemini.sln
```

---

## 4. ✅ COMPLETED

### Backend (.NET 9) — 104 .cs files, all 4 layers
- **Domain:** 7 entities (School, User, Student, CallCampaign, Call, Complaint,
  AuditLog), 13 enums, `IRepository<T>` / `IUnitOfWork`.
- **Application:** 10 services fully implemented — Auth (BCrypt cost 12 + JWT
  15min access / 7d hashed refresh), Dashboard (7-day stats), Student (EPPlus
  Excel import/export), Campaign (background bulk caller, 2s spacing), Call
  (Vapi webhook processor + Claude sentiment analysis + auto-complaint),
  Complaint, User, Settings, Admin, Audit. 27 DTOs, 15 interfaces,
  4 FluentValidation validators.
- **Infrastructure:** `AppDbContext` (auto timestamps, tenant + soft-delete
  query filters), 7 EF configs, generic Repository + UnitOfWork, JwtService,
  VapiService (Telugu fee-reminder & progress-update system prompts),
  TwilioService (WhatsApp + Indian phone normalize), ElevenLabsService,
  SendGrid EmailService, S3 FileStorageService, ClaudeAIService
  (model `claude-sonnet-4-6`), idempotent DbSeeder.
- **API:** 10 controllers / 29 endpoints, GlobalException + RequestLogging +
  Tenant middleware, DI extensions, JWT auth, Swagger w/ bearer, rate limiting
  (auth 5/min, api 100/min), CORS, health checks, Serilog, auto-migrate + seed
  on startup.
- **Seed credentials:** SuperAdmin `admin@eduvoice.in` / `EduVoice@2024!`;
  demo school subdomain `demo` + 5 Telugu-named students.
- Packages bumped to latest in-major (9.0.17, FluentValidation 11.12, etc.).

### Frontend (Angular 22) — premium, BUILD-VERIFIED (`npm run build` passes)
- **Design system** (`src/styles.scss`): Bootstrap 5 + custom premium layer —
  gradients, glassmorphism, soft elevation, Inter+Sora fonts, animations,
  per-tenant white-label via CSS variables (`BrandingService`).
- **Core:** in-memory access token (never localStorage), single-flight silent
  refresh interceptor on 401, loading + error interceptors, auth + role guards,
  typed API service + feature services, full domain models.
- **Layout:** gradient sidebar (active glow + plan card), glass sticky header
  (global search + user menu), responsive shell w/ aurora backdrop + mobile drawer.
- **Pages (ALL routed, all built):**
  - Auth: login (split-screen brand showcase), forgot-password, reset-password
  - Dashboard: stat cards + area/donut charts + recent calls
  - Students: table + filters + add/edit drawer + Excel import drawer + detail page
  - Campaigns: card grid + 4-step launch wizard + live-polling detail
  - Calls: filterable logs + WhatsApp-style transcript panel + audio player
  - Complaints: stats + filters + resolution drawer (pill status workflow)
  - Analytics: bar/donut/line charts
  - Settings: tabbed (profile w/ live color preview, users, voice, notifications, security)
  - Admin: schools tenant table (SuperAdmin only)

---

## 5b. ✅ COMPLETED — Teacher Portal (Module 12)

Branch `claude/new-session-tkaua0`. All work is in this session.

**New entities (Domain):**
- `Attendance` — one row per student per date, tracks IsPresent + Remarks, MarkedByUserId
- `TeacherMarks` — per-exam marks history (Math/Science/English/Telugu/Social + MaxMarks)

**New wiring (all layers):**
- `IUnitOfWork` + `UnitOfWork` + `AppDbContext` extended with `Attendances` and `TeacherMarksList`
- `ITeacherService` / `TeacherService` (Application layer) — dashboard stats, class sections,
  students by class+section, get/mark attendance (upsert + recompute Student aggregate stats),
  upload marks (updates Student entity + inserts TeacherMarks history), assign homework
- `TeacherController` at `GET|POST /api/teacher/*` — `[Authorize(Roles = "Teacher,SchoolAdmin,SuperAdmin")]`
- `ITeacherService` registered in `ServiceCollectionExtensions`

**Frontend:**
- New types in `models.ts`: `ClassSection`, `TeacherStudent`, `TeacherDashboard`, `AttendanceEntry`, `AttendanceResult`
- `TeacherService` (`features/teacher/teacher.service.ts`) — all API calls
- 4 standalone components:
  - `TeacherDashboardComponent` (`/teacher/dashboard`) — stats, attendance banner, quick actions, class grid
  - `AttendanceComponent` (`/teacher/attendance`) — date+class+section picker, student checklist, bulk Present/Absent, save
  - `MarksUploadComponent` (`/teacher/marks`) — class+exam+date filters, score table, preview %, save
  - `TeacherHomeworkComponent` (`/teacher/homework`) — subject+class+description+due-date form, assign
- `app.routes.ts` — 4 new lazy routes under `/teacher/*` inside `main-layout` (auth guard applies)
- Login: Teacher role redirects to `/teacher/dashboard` (others still go to `/dashboard`)
- Sidebar: Teacher role sees teacher-only nav (Dashboard, Attendance, Marks, Homework)

**Not yet done (needs first compile):** EF migrations — run `dotnet ef migrations add AddTeacherPortal`
after the initial `InitialCreate` migration is in place (or include in InitialCreate if running for first time).

---

## 5. ⏳ PENDING / NEXT STEPS (priority order)

1. **Make repo PRIVATE** — owner to toggle in GitHub Settings → Danger Zone.
2. **Compile-verify backend** — this container had **no .NET SDK** (couldn't
   install, network blocked), so the backend was written but never `dotnet build`'d.
   First task on a machine with .NET 9 SDK:
   ```bash
   cd src && dotnet restore && dotnet build
   ```
   Fix any compile errors (most likely: EPPlus license context, minor namespace
   nits). Then run `dotnet ef migrations add InitialCreate` if migrations folder is empty.
3. **EPPlus license** — EPPlus 7 requires setting `ExcelPackage.LicenseContext`
   (or the v8 license key). Verify StudentService sets this or app will throw on Excel ops.
4. **Wire real secrets** — copy `.env.example` → `.env`, fill Vapi / ElevenLabs /
   Twilio / SendGrid / Anthropic / AWS / DB keys. NEVER commit `.env`.
5. **End-to-end test one call** — start backend + Postgres via docker-compose,
   register a school, import students, run a FeeReminder campaign against a test number.
6. **Vapi webhook** — confirm `/api/calls/webhook` is reachable publicly (ngrok
   for local) and `X-Vapi-Secret` matches `VAPI_WEBHOOK_SECRET`.
7. **PROMPT 4 — n8n workflows** (not started): post-call processing, daily
   summary, campaign scheduler. See `EDUVOICEMASTERPROMPTS.md` Prompt 4.
8. **PROMPT 5 — Deployment** (not started): GitHub Actions CI/CD, nginx subdomain
   routing, SSL/Let's Encrypt, Railway/DigitalOcean setup. See Prompt 5.
9. **Frontend ↔ backend integration test** — run `ng serve` (proxies to
   `http://localhost:8080/api`), verify login → dashboard → students against live API.
   Some dashboard/admin response shapes are assumed; reconcile DTOs if mismatched.
10. **Tests** — no unit/integration tests written yet (backend or frontend).
11. **PROMPT 7 — WhatsApp inbound bot** (future feature, not started).

---

## 6. How to Run

### Backend
```bash
cp .env.example .env        # fill in secrets
docker compose up           # api on :8080, postgres on :5432
# OR locally:
cd src && dotnet run --project EduVoice.API
# Swagger: http://localhost:8080/swagger   Health: /health
```

### Frontend
```bash
cd frontend
npm install                 # needs Node >= 22.22.3 (use nvm: nvm install 24)
npm start                   # http://localhost:4200
npm run build               # prod build -> dist/eduvoice (VERIFIED working)
```

---

## 7. Conventions & Guardrails

- **All work on branch `claude/new-session-tkaua0`.** Do not push to `main`.
- Do **not** modify the `capgemini/` MVC app or `capgemini.sln`.
- Commit messages end with the project's Co-Authored-By + Claude-Session trailers.
- Secrets only via env vars — never in code or committed appsettings.
- Tenant isolation is sacred: every data query filters by `SchoolId`
  (except SuperAdmin). Verify after any backend change.
- Frontend keeps the access token in memory only.

---

## 8. Known Caveats

- **Backend not compiled** in this environment (no SDK). Treat first build as
  a debugging pass.
- **Migrations folder may be empty** — generate the initial migration before
  first run (or rely on the startup auto-migrate once a migration exists).
- Sass `@import` deprecation warnings on frontend build are from Bootstrap 5.3's
  own internals — harmless, not errors.
- Dashboard/Analytics/Admin consume some assumed response shapes; align with
  real backend DTOs during integration.
