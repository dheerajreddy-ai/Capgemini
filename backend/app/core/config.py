"""Application configuration — aligned with SuperFit conventions.

All settings are loaded from environment variables (never hardcoded).
On Railway these are injected via the service's Variables tab; locally they
come from a `.env` file (see `.env.example`). Pydantic validates every value
at startup, so a missing or malformed secret fails fast instead of at runtime.
"""

from __future__ import annotations

from functools import lru_cache
from typing import Literal

from pydantic import PostgresDsn, field_validator
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=False,
        extra="ignore",
    )

    # ── App ───────────────────────────────────────────────────
    APP_NAME: str = "FRAME API"
    ENV: Literal["dev", "staging", "production"] = "dev"
    DEBUG: bool = False
    API_V1_PREFIX: str = "/api/v1"

    # ── Database ──────────────────────────────────────────────
    # Railway provides DATABASE_URL automatically when you attach Postgres.
    DATABASE_URL: PostgresDsn
    DB_POOL_SIZE: int = 10
    DB_MAX_OVERFLOW: int = 20
    DB_POOL_RECYCLE_SECONDS: int = 1800

    # ── Auth (Firebase) ───────────────────────────────────────
    # Same Firebase project as SuperFit (saifit-25ac6).
    # Paste the full service-account JSON as a single env var value —
    # Railway handles the escaping. The service handles \n in private_key.
    FIREBASE_SERVICE_ACCOUNT: str = ""
    FIREBASE_PROJECT_ID: str = "saifit-25ac6"

    # When true, the backend accepts mock_token_* tokens (dev/test only).
    # Hardcoded false in production regardless of this flag.
    ALLOW_MOCK_AUTH: bool = False

    # ── CORS ──────────────────────────────────────────────────
    CORS_ORIGINS: str = "http://localhost:8081,http://localhost:8082,http://localhost:19006"

    # ── Rate limiting ─────────────────────────────────────────
    ENABLE_RATE_LIMIT: bool = True
    RATE_LIMIT_REQUESTS_PER_MINUTE: int = 120
    RATE_LIMIT_BURST: int = 30

    # ── AI — Gemini 2.5 Flash (Phase 2) ──────────────────────
    # Get key from Google AI Studio: https://aistudio.google.com/app/apikey
    # Leave empty to run in demo mode (returns sample shot list).
    GEMINI_API_KEY: str = ""
    GEMINI_MODEL: str = "gemini-2.5-flash"

    # ── AI — Nano Banana Pro image generation (Phase 3) ──────
    # Leave empty to skip image generation (shots keep placeholder UI).
    NANO_BANANA_API_KEY: str = ""
    NANO_BANANA_API_URL: str = "https://api.nanobananapro.com"
    NANO_BANANA_MODEL: str = "stable-diffusion-xl-base"
    NANO_BANANA_IMAGE_WIDTH: int = 1432   # 2.39:1 cinematic ratio
    NANO_BANANA_IMAGE_HEIGHT: int = 600

    # ── Observability ─────────────────────────────────────────
    SENTRY_DSN: str = ""

    # ── Storage (Cloudflare R2) — Phase 3 ────────────────────
    R2_ACCOUNT_ID: str = ""
    R2_ACCESS_KEY_ID: str = ""
    R2_SECRET_ACCESS_KEY: str = ""
    R2_BUCKET_NAME: str = ""
    ASSET_CDN_BASE: str = ""
    ASSET_SIGNED_URL_TTL_SECONDS: int = 3600

    @field_validator("DATABASE_URL", mode="before")
    @classmethod
    def _normalize_db_scheme(cls, v: str) -> str:
        # Railway hands out `postgres://` but SQLAlchemy + psycopg2 needs
        # `postgresql+psycopg2://`. Same fix SuperFit uses.
        if isinstance(v, str):
            if v.startswith("postgres://"):
                v = v.replace("postgres://", "postgresql+psycopg2://", 1)
            elif v.startswith("postgresql+psycopg://"):
                # Normalize psycopg3 scheme to psycopg2 (our driver).
                v = v.replace("postgresql+psycopg://", "postgresql+psycopg2://", 1)
            elif v.startswith("postgresql://"):
                v = v.replace("postgresql://", "postgresql+psycopg2://", 1)
        return v

    @property
    def cors_origin_list(self) -> list[str]:
        return [o.strip() for o in self.CORS_ORIGINS.split(",") if o.strip()]

    @property
    def is_production(self) -> bool:
        return self.ENV == "production"

    @property
    def allow_mock(self) -> bool:
        """Mock auth is ONLY permitted outside of production."""
        return self.ALLOW_MOCK_AUTH and not self.is_production


@lru_cache
def get_settings() -> Settings:
    """Cached singleton so the env is parsed exactly once per process."""
    return Settings()  # type: ignore[call-arg]
