"""Application configuration.

All settings are loaded from environment variables (never hardcoded).
On Railway these are injected via the service's Variables tab; locally they
come from a `.env` file (see `.env.example`). Pydantic validates every value
at startup, so a missing or malformed secret fails fast instead of at runtime.
"""

from __future__ import annotations

from functools import lru_cache
from typing import Literal

from pydantic import Field, PostgresDsn, field_validator
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
    ENVIRONMENT: Literal["development", "staging", "production"] = "development"
    DEBUG: bool = False
    API_V1_PREFIX: str = "/api/v1"

    # ── Database ──────────────────────────────────────────────
    # Railway provides DATABASE_URL automatically when you attach Postgres.
    DATABASE_URL: PostgresDsn
    DB_POOL_SIZE: int = 10
    DB_MAX_OVERFLOW: int = 20
    DB_POOL_RECYCLE_SECONDS: int = 1800

    # ── Auth (Firebase) ───────────────────────────────────────
    # Paste the full service-account JSON as a single-line env var.
    FIREBASE_CREDENTIALS_JSON: str = ""
    FIREBASE_PROJECT_ID: str = ""

    # ── CORS ──────────────────────────────────────────────────
    # Comma-separated list of allowed origins (the mobile app's API host,
    # web dashboard, etc.). Never use "*" in production.
    CORS_ORIGINS: str = ""

    # ── Rate limiting ─────────────────────────────────────────
    RATE_LIMIT_PER_MINUTE: int = 60

    @field_validator("DATABASE_URL", mode="before")
    @classmethod
    def _normalize_db_scheme(cls, v: str) -> str:
        # Railway/Heroku hand out `postgres://` but SQLAlchemy + psycopg3
        # expect the explicit `postgresql+psycopg://` driver scheme.
        if isinstance(v, str):
            if v.startswith("postgres://"):
                v = v.replace("postgres://", "postgresql+psycopg://", 1)
            elif v.startswith("postgresql://"):
                v = v.replace("postgresql://", "postgresql+psycopg://", 1)
        return v

    @property
    def cors_origin_list(self) -> list[str]:
        return [o.strip() for o in self.CORS_ORIGINS.split(",") if o.strip()]

    @property
    def is_production(self) -> bool:
        return self.ENVIRONMENT == "production"


@lru_cache
def get_settings() -> Settings:
    """Cached singleton so the env is parsed exactly once per process."""
    return Settings()  # type: ignore[call-arg]
