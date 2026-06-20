"""Health & readiness probes.

`/health` is a cheap liveness check Railway hits to know the process is up.
`/health/ready` additionally verifies the database is reachable — used for
readiness gating during deploys.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends
from sqlalchemy import text
from sqlalchemy.orm import Session

from app import __version__
from app.core.config import get_settings
from app.db.session import get_db

router = APIRouter(tags=["health"])


@router.get("/health", summary="Liveness probe")
def health() -> dict:
    settings = get_settings()
    return {
        "status": "ok",
        "service": settings.APP_NAME,
        "version": __version__,
        "environment": settings.ENVIRONMENT,
    }


@router.get("/health/ready", summary="Readiness probe")
def ready(db: Session = Depends(get_db)) -> dict:
    db.execute(text("SELECT 1"))
    return {"status": "ready", "database": "ok"}
