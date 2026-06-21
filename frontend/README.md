# EduVoice — Frontend (Angular 20 + Bootstrap 5)

Premium dashboard for EduVoice, the AI voice-agent platform that calls
parents in Telugu for fee reminders, progress updates, and complaints.

## Stack
- **Angular 20** (standalone components, signals, new control flow)
- **Bootstrap 5** + a custom premium design-system layer (`src/styles.scss`)
- **ApexCharts** (`ng-apexcharts`) for analytics
- **date-fns** for relative times
- Bootstrap Icons, Inter + Sora typefaces

## Getting started
```bash
cd frontend
npm install
npm start            # ng serve on http://localhost:4200
```
The dev server proxies API calls to `environment.apiUrl`
(`http://localhost:8080/api` by default — the .NET backend in `../src`).

## Build
```bash
npm run build        # production build to dist/eduvoice
```

## Structure
```
src/app/
  core/        auth (token/guards/interceptors), services, models
  shared/      reusable components (stat-card, badges, drawer, toast…) + pipes
  layout/      sidebar, header, main shell
  features/    auth, dashboard, students, campaigns, calls,
               complaints, analytics, settings, admin
```

## Design system
All theming flows through CSS custom properties on `:root`
(`--ev-primary`, gradients, elevation, radii). The `BrandingService`
overrides these at runtime per tenant, so each school sees its own brand
colour and logo (white-label).

## Security
- Access token kept **in memory** (never localStorage)
- Refresh token persisted for session continuity; auth interceptor performs
  single-flight silent refresh on 401 and retries the original request
- Route + role guards protect the dashboard and admin areas
